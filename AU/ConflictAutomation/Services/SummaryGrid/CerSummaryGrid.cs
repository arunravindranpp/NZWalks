using ConflictAutomation.Constants;
using OfficeOpenXml;

namespace ConflictAutomation.Services.SummaryGrid;

public class CerSummaryGrid(ExcelWorksheet worksheet, int titleRow) :
    SummaryGrid(worksheet, titleRow, _title, _colHeaders, _headerHeightInRows)
{
    private static readonly string _title = CAUConstants.MSG_SECTION_CER;
    private static readonly List<string> _colHeaders =
        ["Keyword used", "Accounting Cycle Date", "Engagement Open Date From", "Engagement Open Date To",
         "Client Name", "DUNS Name", "DUNS Location", "DUNS Number", "GCSP Name", "Engagement Partner Name",
         "Ultimate Parent Name", "Ultimate Parent DUNS", "Account Channel", "Audit Relationship",
         "Service Line(s)"];
    private static readonly int _headerHeightInRows = 2;
}
