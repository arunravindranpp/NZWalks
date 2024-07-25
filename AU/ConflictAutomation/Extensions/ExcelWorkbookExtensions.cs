using OfficeOpenXml;

namespace ConflictAutomation.Extensions;

public static class ExcelWorkbookExtensions
{
    public static ExcelWorksheet CopyWorksheet(this ExcelWorkbook excelWorkbook, 
        string sourceWorksheetName, string targetWorksheetName) =>
            excelWorkbook.Worksheets.Copy(sourceWorksheetName, targetWorksheetName);
}
