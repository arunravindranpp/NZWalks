using ConflictAutomation.Models.ResearchSummaryEngine;
using OfficeOpenXml;

namespace ConflictAutomation.Extensions;

public static class UnitGridWorksheetExtensions
{
    public static string TabName(this ExcelWorksheet unitGridWorksheet) => unitGridWorksheet.Name;

    public static string PartyInvolved(this ExcelWorksheet unitGridWorksheet) => unitGridWorksheet.GetCellContents("B1");

    public static string Country(this ExcelWorksheet unitGridWorksheet) => unitGridWorksheet.GetCellContents("D2");

    public static string Summary(this ExcelWorksheet unitGridWorksheet) => unitGridWorksheet.GetCellContents("B3");

    public static string RoleAndComments(this ExcelWorksheet unitGridWorksheet) =>
        unitGridWorksheet.Role() +
        (string.IsNullOrWhiteSpace(unitGridWorksheet.Comments()) ? string.Empty : $"\n{unitGridWorksheet.Comments()}");

    public static string TabReference(this ResearchSummaryEntry researchSummaryEntry, string targetCellReference = "A5") =>
            $"#'{researchSummaryEntry.WorksheetTabName}'!{targetCellReference}";


    private static string Role(this ExcelWorksheet unitGridWorksheet) => unitGridWorksheet.GetCellContents("D1");

    private static string Comments(this ExcelWorksheet unitGridWorksheet) => unitGridWorksheet.GetCellContents("F1");
}
