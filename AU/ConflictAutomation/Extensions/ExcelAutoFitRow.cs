using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace ConflictAutomation.Extensions;

#pragma warning disable CA1416 // Validate platform compatibility
public static class ExcelAutoFitRow
{
    // For a col width of 60.87 (col B+C+D in Research Unit tab) these values were empirically measured: 
    // x: Text Length  y: Enlarging factor  
    // --------------  -------------------  
    //            279                 1.45              
    //           1606                 1.20
    //
    // Applying Point-Slope Form equation: (y - y0) = m(x - x0)
    // m = (y - y0) / (x - x0) ,  where (x0, y0) = (279, 1.45) and (x, y) = (1606, 1.17)
    // Then, the formula for the Enlarging factor is: y = y0 + ( m * (textLength - x0) )
    // plus a safety margin.
    private const float SAFETY_MARGIN = 1.0f;
    private static float EnlargingFactor(int textLength)
    {
        float x0 = 279.0f;  float y0 = 1.45f;
        float x = 1606.0f;  float y = 1.20f;
        float m = (y - y0) / (x - x0);
        float result = y0 + (m * (((float)textLength) - x0));
        result *= SAFETY_MARGIN;
        return Math.Max(1.0f, result);  // Never shrink; only enlarge.
    }


    public static double AutoFitRowHeight(this ExcelWorksheet worksheet, int rowNumber,
        char startCol = 'A', char endCol = 'Z',
        List<(char, char)> allMergedCols = null,
        bool forceWrapText = true, double minHeight = 15)
    {
        if (forceWrapText)
        {
            worksheet.Cells[rowNumber, startCol.ToColNumber(), rowNumber, endCol.ToColNumber()].Style.WrapText = true;
        }

        startCol = char.ToUpper(startCol);
        endCol = char.ToUpper(endCol);
        allMergedCols ??= [];

        double autoHeight = minHeight;

        char colName = startCol;
        while (colName <= endCol)
        {
            double totalExcelColWidth = worksheet.TotalColWidth(colName, allMergedCols, out int deltaCol);

            var cell = worksheet.Cells[rowNumber, colName.ToColNumber()];
            autoHeight = Math.Max(autoHeight, MeasureTextHeight(text: cell.Text, font: cell.Style.Font, totalExcelColWidth));

            colName = (char)(((int)colName) + deltaCol);
        }

        var row = worksheet.Row(rowNumber);
        row.CustomHeight = true;
        row.Height = autoHeight;

        return row.Height;
    }


    private static double TotalColWidth(this ExcelWorksheet worksheet, char colName, List<(char, char)> allMergedCols, out int deltaCol)
    {
        double result = 0.0;

        (char, char) mergedCols = allMergedCols.FirstOrDefault(
                                                colPair => colPair.Contains(colName, StringComparison.OrdinalIgnoreCase));

        deltaCol = 0;
        if (mergedCols.Item1 == 0 && mergedCols.Item2 == 0)
        {
            result = worksheet.Column(colName.ToColNumber()).Width;
            deltaCol = 1;
        }
        else
        {
            char startMergedColName = mergedCols.Item1;
            char endMergedColName = mergedCols.Item2;

            for (var mergedColName = startMergedColName; mergedColName <= endMergedColName; mergedColName++)
            {
                result += worksheet.Column(mergedColName.ToColNumber()).Width;
                deltaCol++;
            }
        }

        return result;
    }


    private static bool Contains(this (char, char) pair, char item, StringComparison stringComparison) =>
        stringComparison switch
        {
            StringComparison.CurrentCultureIgnoreCase
            or StringComparison.InvariantCultureIgnoreCase
            or StringComparison.OrdinalIgnoreCase => (pair.Item1 <= item) && (item <= pair.Item2),

            _ => (char.ToUpper(pair.Item1) <= char.ToUpper(item)) && (char.ToUpper(item) <= char.ToUpper(pair.Item2)),
        };


    private static int ToColNumber(this char colName) => (colName - 'A') + 1;


    // Source:
    // https://stackoverflow.com/questions/41639278/autofit-row-height-of-merged-cell-in-epplus
    // Adapted from code in post by Ben Gripka on 2017-05-12
    private static double MeasureTextHeight(string text, ExcelFont font, double excelColWidth)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0.0;
        }

        var graphics = Graphics.FromHwnd(IntPtr.Zero);
        var textSizeInPixels = graphics.MeasureString(
                                    text, 
                                    font: MakeFont(font, EnlargingFactor(text.Length)), 
                                    width: ExcelColWidthToPixels(excelColWidth), 
                                    format: new StringFormat { 
                                                FormatFlags = StringFormatFlags.MeasureTrailingSpaces 
                                            }
                               );

        double excelRowHeight = PixelsToExcelRowHeight(Convert.ToInt32(textSizeInPixels.Height));

        return Math.Min(excelRowHeight, 409.0);  // Maximum Excel Row Height is 409
    }


    private static Font MakeFont(ExcelFont font, float enlargingFactor = 1.0f)
    {
        var fontSize = font.Size * enlargingFactor;
        var fontStyle = (font.Bold ? FontStyle.Bold : FontStyle.Regular) | (font.Italic ? FontStyle.Italic : FontStyle.Regular);
        return new Font(font.Name, fontSize, fontStyle);
    }


    // 8.43 in Excel Column Width = 64 Pixels
    private static int ExcelColWidthToPixels(double excelWidth) => Convert.ToInt32(excelWidth * 64.0 / 8.43);


    // 20 Pixels = 15 in Excel Row Height
    private static double PixelsToExcelRowHeight(int pixels) => pixels * 15.0 / 20.0;
}
#pragma warning restore CA1416 // Validate platform compatibility
