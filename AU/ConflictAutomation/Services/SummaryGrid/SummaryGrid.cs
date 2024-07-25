using ConflictAutomation.Extensions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace ConflictAutomation.Services.SummaryGrid;

public abstract class SummaryGrid(ExcelWorksheet worksheet, int titleRow, string title,
    List<string> colHeaders, int headerHeightInRows, int widthInColsWhenNoHeader = 1)
{
    private readonly ExcelWorksheet _worksheet = worksheet;
    private readonly int _titleRow = titleRow;
    private readonly string _title = title;
    private readonly List<string> _colHeaders = colHeaders;
    private readonly int _headerHeightInRows = headerHeightInRows;
    private readonly int _widthInColsWhenNoHeader = widthInColsWhenNoHeader;

    protected const char FIRST_COL = 'A';
    protected const char LAST_SECTION_TITLE_COL = 'D';
    protected const int MIN_COL_WIDTH = 21;
    protected const int MAX_COL_WIDTH = 35;
    protected const string MSG_NO_DATA = "-";

    protected readonly Color darkGray = Color.FromArgb(191, 191, 191);
    protected readonly Color brightTeal = Color.FromArgb(183, 222, 232);


    public virtual int WriteTitleAndHeaders()
    {
        WriteTitle();
        return WriteHeaders();  // When there are no headers, this returns the title Row.
    }


    public virtual int WriteBodyWithNoData()
    {
        if (_colHeaders.IsNullOrEmpty())
        {
            _worksheet.Cells[$"{FIRST_COL}{DataStartRow}"].Value = MSG_NO_DATA;
            if(_widthInColsWhenNoHeader > 1)
            {
                DataRange.Merge = true;                                 
            }
        }
        else
        {
            for (char col = FIRST_COL; col <= LastCol; col++)
            {
                _worksheet.Cells[$"{col}{DataStartRow}"].Value = MSG_NO_DATA;
            }
            AutoFitColumns(DataRange);
        }                

        DataRange.SetBackgroundColor(brightTeal);
        DataRange.SetBorders(ExcelBorderStyle.Thin, ExcelBorderStyle.Thin);
        
        return DataStartRow;
    }


    protected int WriteTitle()
    {
        _worksheet.Cells[$"{FIRST_COL}{_titleRow}"].Value = _title;
        _worksheet.Cells[$"{FIRST_COL}{_titleRow}"].Style.Font.Color.SetColor(Color.White);
        _worksheet.Cells[$"{FIRST_COL}{_titleRow}"].Style.Font.Bold = true;
        _worksheet.Cells[$"{FIRST_COL}{_titleRow}"].Style.Font.Size = 9;
        _worksheet.Cells[$"{FIRST_COL}{_titleRow}:{LAST_SECTION_TITLE_COL}{_titleRow}"].Merge = true;
        TitleRange.SetBackgroundColor(Color.Black);
        TitleRange.SetBorders(ExcelBorderStyle.None, ExcelBorderStyle.Thin);

        return _titleRow;
    }


    protected int WriteHeaders()
    {
        if (_colHeaders.IsNullOrEmpty())
        {
            return HeaderRow;
        }

        foreach (var colTitle in _colHeaders)
        {
            char col = (char)(FIRST_COL + _colHeaders.IndexOf(colTitle));
            _worksheet.Cells[$"{col}{HeaderRow}"].Value = colTitle;
        }

        HeadersRange.SetBackgroundColor(darkGray);
        HeadersRange.SetBorders(ExcelBorderStyle.Thin, ExcelBorderStyle.Thin);
        HeadersRange.SetHeightInRows(_headerHeightInRows);
        HeadersRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        AutoFitColumns(HeadersRange);

        return HeaderRow;
    }


    protected static void AutoFitColumns(ExcelRange range) => 
        range.AutoFitColumns(MIN_COL_WIDTH, MAX_COL_WIDTH);


    protected ExcelRange TitleRange => _worksheet.Cells[TitleRangeReference];
    protected string TitleRangeReference => $"{FIRST_COL}{_titleRow}:{LastCol}{_titleRow}";

    protected ExcelRange HeadersRange => _worksheet.Cells[HeadersRangeReference];
    protected string HeadersRangeReference => $"{FIRST_COL}{HeaderRow}:{LastCol}{HeaderRow}";

    protected ExcelRange DataRange => _worksheet.Cells[DataRangeReference];
    protected string DataRangeReference => $"{FIRST_COL}{DataStartRow}:{LastCol}{DataStartRow}";

    protected ExcelRange SummaryGridRange => _worksheet.Cells[SummaryGridRangeReference];
    protected string SummaryGridRangeReference => $"{FIRST_COL}{_titleRow}:{LastCol}{DataStartRow}";

    protected ExcelRange GridRange => _worksheet.Cells[GridRangeReference];
    protected string GridRangeReference => $"{FIRST_COL}{HeaderRow}:{LastCol}{DataStartRow}";

    protected char LastCol => 
        (char)(FIRST_COL + (_colHeaders.IsNullOrEmpty() ? _widthInColsWhenNoHeader : _colHeaders.Count) - 1);

    protected int HeaderRow => _titleRow + (_colHeaders.IsNullOrEmpty() ? 0 : 1);
    protected int DataStartRow => _titleRow + (_colHeaders.IsNullOrEmpty() ? 0 : 1) + 1;
}
