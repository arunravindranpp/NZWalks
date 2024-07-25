using ConflictAutomation.Constants;
using OfficeOpenXml;

namespace ConflictAutomation.Services.SummaryGrid;

public class GisSummaryGrid(ExcelWorksheet worksheet, int titleRow) : 
    SummaryGrid(worksheet, titleRow, _title, _colHeaders, _headerHeightInRows)
{
    private static readonly string _title = CAUConstants.MSG_SECTION_GIS;
    private static readonly List<string> _colHeaders =
        ["Keyword used", "Entity Name (Results)", "Alias Name", "Country", "DUNS Number",
         "Mercury ID/ GFIS ID", "Restrictions", "GCSP Name", "Entity Under Audit", "G360 (Yes or No)",
         "GIS ID", "Ultimate Parent Name", "Ultimate Parent DUNS", "Parent Relationships (Yes or No)",
         "Subsidiaries (Yes or No)", "LAP", "PIE", "PIE Affiliate"];
    private static readonly int _headerHeightInRows = 2;    
}
