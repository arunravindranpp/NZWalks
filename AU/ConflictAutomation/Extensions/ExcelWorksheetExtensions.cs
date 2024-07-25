using OfficeOpenXml;

namespace ConflictAutomation.Extensions;

public static class ExcelWorksheetExtensions
{
    public static string GetCellContents(this ExcelWorksheet worksheet, string cellAddress) =>
        worksheet?.Cells?[cellAddress]?.Value?.ToString() ?? string.Empty;
}
