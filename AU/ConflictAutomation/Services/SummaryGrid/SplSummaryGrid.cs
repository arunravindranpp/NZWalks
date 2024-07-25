using OfficeOpenXml;

namespace ConflictAutomation.Services.SummaryGrid;

public class SplSummaryGrid(ExcelWorksheet worksheet, int titleRow) :
    SummaryGrid(worksheet, titleRow, _title, _colHeaders, _headerHeightInRows, _widthInColsWhenNoHeader)
{
    private static readonly string _title = "SPL, PACE CRR, Local & External Databases, MDM, Google";
    private static readonly List<string> _colHeaders = [];
    private static readonly int _headerHeightInRows = 0;
    private static readonly int _widthInColsWhenNoHeader = 8;
}
