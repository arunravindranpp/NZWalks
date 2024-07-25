using iText.Layout.Properties;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace ConflictAutomation.Extensions;

public static class ExcelRangeExtensions
{
    private const string CSHARP_NEW_LINE = "\\n";
    private const string EXCEL_NEW_LINE = "\\r\\n";
    private const double STD_ROW_HEIGHT = 15.0;
    private const float DEFAULT_FONT_SIZE = 10.0f;


    public static ExcelRange GetRangeWithSize(this ExcelWorksheet worksheet, string startingCellAddress, int numberOfRows, int numberOfColumns)
    {
        ExcelRange startingCell = worksheet.Cells[startingCellAddress];
        string endingCellAddress = startingCell.Offset(numberOfRows - 1, numberOfColumns - 1).Address;
        return worksheet.Cells[$"{startingCellAddress}:{endingCellAddress}"];
    }


    public static ExcelRange SetBorders(
        this ExcelRange range,
        ExcelBorderStyle internalBorderStyle, ExcelBorderStyle externalBorderStyle)
    {
        range.Style.Border.Top.Style = internalBorderStyle;
        range.Style.Border.Bottom.Style = internalBorderStyle;
        range.Style.Border.Left.Style = internalBorderStyle;
        range.Style.Border.Right.Style = internalBorderStyle;
        range.Style.Border.BorderAround(externalBorderStyle);

        return range;
    }


    public static ExcelRange ClearBorders(this ExcelRange range)
    {
        SetBorders(range, ExcelBorderStyle.None, ExcelBorderStyle.None);
        return range;
    }


    public static ExcelRange ClearContentsAndBorders(this ExcelRange range)
    {
        range.Clear();
        range.ClearBorders();
        return range;
    }


    public static ExcelRange WriteHyperlink(this ExcelRange cell, string text, string targetReference)
    {
        cell.Formula = $"HYPERLINK(\"{targetReference}\", \"{text.Replace("\"", "\"\"")}\")";
        cell.Style.Font.Color.SetColor(Color.Blue);
        cell.Style.Font.UnderLine = true;
        return cell;
    }


    public static ExcelRange WriteHyperlink(this ExcelRange cell, string targetReference)
    {
        cell.Formula = $"HYPERLINK(\"{targetReference}\")";
        cell.Style.Font.Color.SetColor(Color.Blue);
        cell.Style.Font.UnderLine = true;
        return cell;
    }



    public static ExcelRange WriteMultipleLines(this ExcelRange cell, string multipleLinesText)
    {
        cell.RichText.Add(FormatMultipleLinesInExcel(multipleLinesText));
        cell.Style.WrapText = true;
        return cell;
    }


    public static ExcelRange SetBackgroundColor(this ExcelRange range, Color color)
    {
        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        range.Style.Fill.BackgroundColor.SetColor(color);
        return range;
    }


    public static void SetHeightInRows(this ExcelRange range, int rowCount, double? rowHeight = null, bool IsWrapText = true)
    {
        rowHeight ??= STD_ROW_HEIGHT;
        range.EntireRow.Height = ((double) rowHeight!) * rowCount;
        range.Style.WrapText = IsWrapText;
    }


    private static string FormatMultipleLinesInExcel(string text)
    {
        string result = text.Replace(CSHARP_NEW_LINE, EXCEL_NEW_LINE);
        return result;
    }


    public static void ResetEverything(this ExcelRange range, string fontName, int fontSize,
    ExcelHorizontalAlignment horizontalAlignment, ExcelVerticalAlignment verticalAlignment)
    {
        range.Merge = false;
        range.Clear();
        range.Style.Font.Bold = false;
        range.Style.Font.Italic = false;
        range.Style.Font.UnderLine = false;
        range.Style.Font.Strike = false;
        range.Style.WrapText = false;
        range.Style.Font.Name = fontName;
        range.Style.Font.Size = fontSize;
        range.Style.HorizontalAlignment = horizontalAlignment;
        range.Style.VerticalAlignment = verticalAlignment;
        range.Style.Font.Color.SetColor(Color.Black);
        range.SetBackgroundColor(Color.White);
        range.SetBorders(ExcelBorderStyle.None, ExcelBorderStyle.None);
    }


    public static ExcelRange MergeAndWrap(this ExcelRange range, string text = "", float fontSize = DEFAULT_FONT_SIZE,
        ExcelVerticalAlignment verticalAlignment = ExcelVerticalAlignment.Center, 
        ExcelHorizontalAlignment horizontalAlignment = ExcelHorizontalAlignment.Left)
    {
        range.Style.Font.Size = fontSize;
        range.Style.VerticalAlignment = verticalAlignment;
        range.Style.HorizontalAlignment = horizontalAlignment;
        range.Merge = true;
        range.Style.WrapText = true;
        range.Value = text;

        return range;
    }


    public static void WriteData(this ExcelRange range, string text = "", float fontSize = DEFAULT_FONT_SIZE, 
        ExcelVerticalAlignment verticalAlignment = ExcelVerticalAlignment.Center, 
        ExcelHorizontalAlignment horizontalAlignment = ExcelHorizontalAlignment.Left)
    {
        range.Style.Font.Size = fontSize;
        range.Style.VerticalAlignment = verticalAlignment;
        range.Style.HorizontalAlignment = horizontalAlignment;
        range.Value = text;
    }
}
