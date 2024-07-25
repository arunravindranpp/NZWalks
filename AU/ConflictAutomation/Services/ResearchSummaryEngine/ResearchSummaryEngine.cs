using ConflictAutomation.Extensions;
using ConflictAutomation.Models.ResearchSummaryEngine;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;
using System.Text;
using MWP = ConflictAutomation.Services.MasterWorkbookParser;

namespace ConflictAutomation.Services.ResearchSummaryEngine;

public class ResearchSummaryEngine
{
    private const int POTENTIAL_NUMBER_OF_ROWS = 1000;
    private const int COL_OFFSET_PARTY_INVOLVED = 0;
    private const int COL_OFFSET_COUNTRY = 1;
    private const int COL_OFFSET_ROLE = 2;
    private const int COL_OFFSET_SUMMARY = 3;
    private static readonly int NumberOfColumnsInResearchSummary = COL_OFFSET_SUMMARY + 1;

    private readonly MWP.MasterWorkbookParser WorkbookParser;

    public ExcelWorksheet SummaryWorksheet { get; init; }


    public ResearchSummaryEngine(ExcelWorksheet summaryWorksheet)
    {
        ArgumentNullException.ThrowIfNull(summaryWorksheet);
        SummaryWorksheet = summaryWorksheet;
        
        WorkbookParser = new(Workbook);
    }
    


    public List<ResearchSummaryEntry> GetEntries() =>
        WorkbookParser.UnitGridWorksheets?.Select(w => new ResearchSummaryEntry(
                                                            worksheetTabName: w.TabName(),
                                                            partyInvolved: w.PartyInvolved(),
                                                            country: w.Country(),
                                                            role: w.RoleAndComments(),
                                                            summary: w.Summary())).ToList();


    public void WriteEntries(string startingCellAddress, List<ResearchSummaryEntry> researchSummaryEntries)
    {
        ResetResearchSummaryRange(startingCellAddress);

        if (researchSummaryEntries is null || researchSummaryEntries.Count == 0)
        {
            return;
        }

        ExcelRange startingCell = SummaryWorksheet.Cells[startingCellAddress];
        int rowOffset = 0;
        foreach (ResearchSummaryEntry researchSummaryEntry in researchSummaryEntries)
        {
            startingCell.Offset(rowOffset, COL_OFFSET_PARTY_INVOLVED)
                .WriteHyperlink(researchSummaryEntry.PartyInvolved,
                                researchSummaryEntry.TabReference());

            startingCell.Offset(rowOffset, COL_OFFSET_COUNTRY)
                .Value = researchSummaryEntry.Country;

            startingCell.Offset(rowOffset, COL_OFFSET_ROLE)
                .WriteMultipleLines(researchSummaryEntry.Role);
            
            startingCell.Offset(rowOffset, COL_OFFSET_SUMMARY).Style.Font.Size = 
                (researchSummaryEntry.Summary.Length >= 949) ? 7.5f : 8.0f;
            startingCell.Offset(rowOffset, COL_OFFSET_SUMMARY)
                .WriteMultipleLines(researchSummaryEntry.Summary);
            rowOffset++;
        }
        FormatResearchSummary(startingCellAddress,
                              numberOfRows: rowOffset, NumberOfColumnsInResearchSummary);
    }


    public static string GetOverallSummary(List<ResearchSummaryEntry> researchSummaryEntries, string engagementDescription)
    {
        if(researchSummaryEntries.IsNullOrEmpty())
        {
            return string.Empty;
        }
        
        StringBuilder overallSummary = new();
        overallSummary.AppendLine("Summary :");
        overallSummary.AppendLine();

        overallSummary.AppendLine("Scope :");
        overallSummary.AppendLine(engagementDescription);
        overallSummary.AppendLine();
        overallSummary.AppendLine();

        foreach (ResearchSummaryEntry researchSummaryEntry in researchSummaryEntries)
        {

            overallSummary.AppendLine($"{researchSummaryEntry.Role} : {researchSummaryEntry.PartyInvolved}");
            overallSummary.AppendLine(researchSummaryEntry.Summary);
        }

        string result = overallSummary.ToString()
                            .Replace("\r\n", "\n")
                            .RemovePrefixes("\n")
                            .Replace("\n", "<br>");
        return result;
    }


    private ExcelWorkbook Workbook => SummaryWorksheet.Workbook;    


    private void ResetResearchSummaryRange(string startingCellAddress)
    {
        ExcelRange researchSummaryRange = SummaryWorksheet!.GetRangeWithSize(startingCellAddress,
                                            numberOfRows: POTENTIAL_NUMBER_OF_ROWS, NumberOfColumnsInResearchSummary);
        researchSummaryRange.ClearContentsAndBorders();
        researchSummaryRange.Style.Font.Size = 8;
        //researchSummaryRange.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

        AdjustResearchSummaryColumnWidths();
    }


    private void AdjustResearchSummaryColumnWidths()
    {
        SummaryWorksheet.Column('L'.ExcelColumnNumber()).Width = 40;
        SummaryWorksheet.Column('M'.ExcelColumnNumber()).Width = 16.86;
        SummaryWorksheet.Column('N'.ExcelColumnNumber()).Width = 17;
        SummaryWorksheet.Column('O'.ExcelColumnNumber()).Width = 69.71;
    }    


    private void FormatResearchSummary(string startingCellAddress, int numberOfRows, int numberOfColumns)
    {
        ExcelRange startingCell = SummaryWorksheet.Cells[startingCellAddress];

        string endingCellAddress = startingCell.Offset(numberOfRows - 1, numberOfColumns - 1).Address;
        ExcelRange researchSummaryRange = SummaryWorksheet.Cells[$"{startingCellAddress}:{endingCellAddress}"];
        researchSummaryRange.SetBorders(ExcelBorderStyle.Thin, ExcelBorderStyle.Thick);
        //researchSummaryRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
    }    
}
