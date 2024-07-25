using OfficeOpenXml;

namespace ConflictAutomation.Extensions;

public static class ExcelPackageExtensions
{
    public static ExcelWorksheet GetWorksheet(this ExcelPackage package, string worksheetTabName) =>
        package.Workbook.Worksheets.FirstOrDefault(w => w.Name == worksheetTabName);
}
