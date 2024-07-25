using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace ConflictAutomation.Extensions;

public static class ExcelRangeBaseExtensions
{
    private const string CSHARP_NEW_LINE = "\\n";
    private const string EXCEL_NEW_LINE = "\\r\\n";


    public static ExcelRangeBase WriteHyperlink(this ExcelRangeBase cell, string text, string targetReference)
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


    public static ExcelRangeBase WriteMultipleLines(this ExcelRangeBase cell, string multipleLinesText)
    {
        cell.RichText.Add(FormatMultipleLinesInExcel(multipleLinesText));
        cell.Style.WrapText = true;
        //cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
        return cell;
    }


    public static ExcelRangeBase SetFontSize(this ExcelRangeBase range, float fontSize)
    {
        range.Style.Font.Size = fontSize;
        return range;
    }


    public static ExcelRangeBase SetFontBold(this ExcelRangeBase range, bool isBold)
    {
        range.Style.Font.Bold = isBold;
        return range;
    }


    public static ExcelRangeBase SetBackgroundColor(this ExcelRangeBase range, Color color)
    {
        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        range.Style.Fill.BackgroundColor.SetColor(color);
        return range;
    }


    public static ExcelRangeBase SetFontColor(this ExcelRangeBase range, Color color)
    {
        range.Style.Font.Color.SetColor(color);
        return range;
    }


    private static string FormatMultipleLinesInExcel(string text)
    {
        string result = text.Replace(CSHARP_NEW_LINE, EXCEL_NEW_LINE);
        return result;
    }
}
