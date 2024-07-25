using ConflictAutomation.Extensions;
using OfficeOpenXml;

namespace ConflictAutomation.Services.MasterWorkbookParser;

public class MasterWorkbookParser(ExcelWorkbook workbook)
{
    private ExcelWorkbook Workbook { get; init; } = workbook;
    private ExcelWorksheets Worksheets => Workbook.Worksheets;


    public List<ExcelWorksheet> UnitGridWorksheets => Worksheets.Where(w => IsAUnitGridWorksheet(w)).ToList();

    public List<string> UnitGridWorksheetTabNames => UnitGridWorksheets.Select(w => w.TabName()).ToList();

    
    private static bool IsAUnitGridWorksheet(ExcelWorksheet w) =>
        w.GetCellContents("A1").FullTrim().Equals("Entity / Individual Name", StringComparison.OrdinalIgnoreCase)
        && w.GetCellContents("B1").FullTrim().NotEquals(string.Empty, StringComparison.OrdinalIgnoreCase)
        && w.GetCellContents("C1").FullTrim().Equals("Role (PACE)", StringComparison.OrdinalIgnoreCase)
        && w.Name.FullTrim().NotEquals("(Entity Tmp)", StringComparison.OrdinalIgnoreCase);
}
