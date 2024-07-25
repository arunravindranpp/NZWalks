using ConflictAutomation.Constants;
using ConflictAutomation.Extensions;
using ConflictAutomation.Models.FinScan;
using ConflictAutomation.Utilities;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace ConflictAutomation.Services.FinScan;

public class FinScanListProfileReportExcelFormatter
{
    private const uint HEADER_FOREGROUND_COLOR = 0xffffffff;
    private const uint HEADER_BACKGROUND_COLOR = 0xff4485b8;

    private const int REPORT_COL_COUNT = 10;

    private const float HEIGHT_CELL_MULTIPLE_LINES = 240;
    private const float MAX_HEIGHT = 409;

    private const int MAX_CHARACTERS_PER_CEL = 32767;

    private const string MSG_NO_DATA = "-";
    private const char DATE_PARTS_SEPARATOR = '-';

    private static Color colorHeaderForeground => Color.FromArgb(unchecked((int)HEADER_FOREGROUND_COLOR));
    private static Color colorHeaderBackground => Color.FromArgb(unchecked((int)HEADER_BACKGROUND_COLOR));

    private readonly string _outputFilePath;
    private readonly FinScanListProfileReport _finScanListProfileReport;
    private int _currentRow = 0;

    private ExcelWorksheet _excelWorksheet;


    public FinScanListProfileReportExcelFormatter(string outputFilePath, FinScanListProfileReport finScanListProfileReport)
    {
        _outputFilePath = outputFilePath;
        _finScanListProfileReport = finScanListProfileReport;
        FileSystem.DeleteFileIfExisting(_outputFilePath);
    }


    public string Run()
    {
        FileSystem.DeleteFileIfExisting(_outputFilePath);

        if ((_finScanListProfileReport is null) || (_finScanListProfileReport.Items.Count < 1))
        {
            return string.Empty;
        }

        if(_finScanListProfileReport.SearchMatches.IsNullOrEmpty())
        {
            return string.Empty;
        }

        if(_finScanListProfileReport.NamesSearched.IsNullOrEmpty())
        {
            return string.Empty;
        }


        using (var excelPackage = new ExcelPackage(_outputFilePath))
        {
            foreach (var finScanListProfileReportItem in _finScanListProfileReport.Items)
            {
                string tabName = $"{_finScanListProfileReport.Items.IndexOf(finScanListProfileReportItem) + 1:00}";
                _excelWorksheet = excelPackage.Workbook.Worksheets.Add(tabName);
                _currentRow = 0;
                
                WriteTitle();
                WriteNamesSearched();
                WriteReportItemIndexSection(finScanListProfileReportItem);
                WriteReportType(finScanListProfileReportItem);

                RunApplicableReportLayout(finScanListProfileReportItem);
            }

            excelPackage.Save();
        }
            
        return Path.Exists(_outputFilePath) ? _outputFilePath : string.Empty;
    }


    private void RunApplicableReportLayout(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        switch (finScanListProfileReportItem.ListId)
        {
            case FinScanConstants.LISTID_DJWL:
                RunWithDjwlLayout(finScanListProfileReportItem);
                break;

            case FinScanConstants.LISTID_DJSOC:
                RunWithDjsocLayout(finScanListProfileReportItem);
                break;

            case FinScanConstants.LISTID_KH50:
            case FinScanConstants.LISTID_KHCO:
                RunWithKharonLayout(finScanListProfileReportItem);
                break;

            default: 
                break;
        }
    }


    private void RunWithDjwlLayout(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        WriteDjwlHeaderSection(finScanListProfileReportItem);
        WriteDjwlDatesSection(finScanListProfileReportItem);
        WriteDjwlDescriptionsSection(finScanListProfileReportItem);
        WriteDjwlSanctionsReferencesSection(finScanListProfileReportItem);

        switch (finScanListProfileReportItem.OriginalScriptName)
        {
            case FinScanConstants.MSG_INDIVIDUAL:
                WriteDjwlNamesSectionForIndividual(finScanListProfileReportItem);
                break;
            case FinScanConstants.MSG_ENTITY:
                WriteDjwlNamesSectionForEntity(finScanListProfileReportItem);
                break;
        }

        WriteDjwlAddressesSection(finScanListProfileReportItem);
        WriteDjwlIdsSection(finScanListProfileReportItem);
        WriteDjwlCountriesSection(finScanListProfileReportItem);

        if (finScanListProfileReportItem.BirthPlaceColl is not null)
        {
            WriteDjwlBirthPlaceSectionIndividual(finScanListProfileReportItem);
        }

        WriteDjwlAssociatesSection(finScanListProfileReportItem);
        WriteDjwlProfileNotesSection(finScanListProfileReportItem);

        if (finScanListProfileReportItem.ImagesColl is not null)
        {
            WriteDjwlImagesSectionForIndividual(finScanListProfileReportItem);
        }

        WriteDjwlSourcesSection(finScanListProfileReportItem);
    }


    private static string FormatDate(string year, string month, string day,
        char datePartsSeparator = DATE_PARTS_SEPARATOR, string msgNoData = MSG_NO_DATA)
    {
        if (string.IsNullOrWhiteSpace($"{day}{month}{year}"))
        {
            return msgNoData;
        }

        string result;
        try
        {
            List<string> dateParts = [day, month, year.Right(2)];
            result = string.Join(datePartsSeparator, dateParts.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
        catch
        {
            result = msgNoData;
        }
        return result;
    }


    private static string FormatDate(string yyyymmdd,
        char datePartsSeparator = DATE_PARTS_SEPARATOR, string msgNoData = MSG_NO_DATA)
    {
        if (string.IsNullOrWhiteSpace(yyyymmdd))
        {
            return msgNoData;
        }

        string result;
        try
        {
            List<string> dateParts = [yyyymmdd.Right(2), yyyymmdd.Mid(4, 2), yyyymmdd.Left(4)];
            result = string.Join(datePartsSeparator, dateParts.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
        catch
        {
            result = msgNoData;
        }
        return result;
    }


    private void SkipLine(int delta = 1)
    {
        _currentRow += delta;
    }


    private void Write(string reference, string value, float fontSize = CAUConstants.DEFAULT_FONT_SIZE)
    {
        _excelWorksheet.Cells[$"{reference}"].Value = value;
        _excelWorksheet.Cells[$"{reference}"].SetFontSize(fontSize);
    }


    private void WriteBanner(
        string startingCellAddress, string text, int columnCount,
        float fontSize = CAUConstants.DEFAULT_FONT_SIZE, bool isBold = false,
        Color? fontColor = null, Color? backgroundColor = null)
    {
        fontColor ??= Color.Black;
        backgroundColor ??= Color.White;

        Write(startingCellAddress, text);
        var startingCell = _excelWorksheet.Cells[startingCellAddress];
        var range = startingCell.Offset(0, 0, 1, columnCount);
        range.Merge = true;
        range.SetBackgroundColor((Color)backgroundColor!)
             .SetFontSize(fontSize)
             .SetFontBold(isBold)
        .SetFontColor((Color)fontColor!);
    }


    private void WriteNA(string startingCellAddress, int columnCount, float fontSize = CAUConstants.DEFAULT_FONT_SIZE)
    {
        WriteBanner(startingCellAddress, FinScanConstants.MSG_NA, columnCount, fontSize);
    }


    private static void WriteColumnContents(
        ExcelRange startingCell, List<string> listContents, float fontSize = CAUConstants.DEFAULT_FONT_SIZE)
    {
        foreach (var content in listContents)
        {
            if (content is not null)
            {
                startingCell.Offset(0, listContents.IndexOf(content)).Value = content;
            }
        }

        var range = startingCell.Offset(0, 0, 1, listContents.Count);
        range.Style.Font.Size = fontSize;
    }


    private void WriteColumnContents(
        string startingCellAddress, List<string> listContents, float fontSize = CAUConstants.DEFAULT_FONT_SIZE)
    {
        var startingCell = _excelWorksheet.Cells[startingCellAddress];
        WriteColumnContents(startingCell, listContents, fontSize);
    }


    private void WriteColumnHeaders(
        string startingCellAddress, List<string> headers, float fontSize = CAUConstants.DEFAULT_FONT_SIZE)
    {
        var startingCell = _excelWorksheet.Cells[startingCellAddress];
        WriteColumnContents(startingCell, headers, fontSize);

        var range = startingCell.Offset(0, 0, 1, headers.Count);
        range.SetBackgroundColor(colorHeaderBackground)
             .SetFontSize(fontSize)
             .SetFontBold(true)
             .SetFontColor(colorHeaderForeground);
    }


    private void WriteMultipleLinesLongText(string startingCellAddress, string multipleLineText, float fontSize)
    {
        var startingCell = _excelWorksheet.Cells[startingCellAddress];
        startingCell.Offset(0, REPORT_COL_COUNT).EntireRow.Height = MAX_HEIGHT;
        startingCell.Offset(1, REPORT_COL_COUNT).EntireRow.Height = HEIGHT_CELL_MULTIPLE_LINES;
        startingCell.Offset(0, 0, 2, REPORT_COL_COUNT).Merge = true;
        startingCell.SetFontSize(fontSize);
        startingCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
        startingCell.Offset(0, 0).WriteMultipleLines(multipleLineText);
    }


    private void WriteTitle()
    {
        WriteBanner($"A{++_currentRow}", FinScanConstants.MSG_REPORT_TITLE, REPORT_COL_COUNT,
            CAUConstants.TITLE_FONT_SIZE, true, colorHeaderForeground, colorHeaderBackground);
    }


    private void WriteNamesSearched()
    {
        SkipLine();
        WriteBanner($"A{++_currentRow}", FinScanConstants.MSG_NAMES_SEARCHED, REPORT_COL_COUNT, CAUConstants.DEFAULT_FONT_SIZE,
            isBold: true, colorHeaderForeground, colorHeaderBackground);
        WriteBanner($"A{++_currentRow}", 
            string.Join(FinScanConstants.STRING_SEPARATOR, _finScanListProfileReport.NamesSearched.Distinct()), 
            REPORT_COL_COUNT, CAUConstants.DEFAULT_FONT_SIZE);
        
        _excelWorksheet.AutoFitRowHeight(_currentRow,
            startCol: 'A', endCol: 'H', allMergedCols: [('A', (char)('A' + REPORT_COL_COUNT - 1))]);
    }


    private void WriteReportType(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        SkipLine();
        WriteBanner($"A{++_currentRow}", FinScanConstants.MSG_REPORT_TYPE, REPORT_COL_COUNT, CAUConstants.DEFAULT_FONT_SIZE,
            isBold: true, colorHeaderForeground, colorHeaderBackground);
        WriteBanner($"A{++_currentRow}", ReportType(finScanListProfileReportItem.ListId), REPORT_COL_COUNT, CAUConstants.DEFAULT_FONT_SIZE);        
    }


    private static string ReportType(string listId) => listId switch
    {
        FinScanConstants.LISTID_DJWL => FinScanConstants.MSG_DJWL,
        FinScanConstants.LISTID_DJSOC => FinScanConstants.MSG_DJSOC,
        FinScanConstants.LISTID_KH50 => FinScanConstants.MSG_KH50,
        FinScanConstants.LISTID_KHCO => FinScanConstants.MSG_KHCO,
        _ => $"{listId} (?)"
    };


    private void WriteReportItemIndexSection(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        SkipLine();
        WriteBanner($"A{++_currentRow}", _finScanListProfileReport.ItemIndex(finScanListProfileReportItem),
                    REPORT_COL_COUNT, CAUConstants.DEFAULT_FONT_SIZE, isBold: true);
    }


    private void WriteDjwlHeaderSection(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        string uid = string.IsNullOrWhiteSpace(finScanListProfileReportItem.Uid) ? string.Empty
                        : $"{new string(' ', 4)}(UID {finScanListProfileReportItem.Uid})";
        SkipLine();
        WriteBanner($"A{++_currentRow}", $"{finScanListProfileReportItem.Name}{uid}", REPORT_COL_COUNT,
                    CAUConstants.LARGE_FONT_SIZE, true, colorHeaderForeground, colorHeaderBackground);

        Write($"A{++_currentRow}", FinScanConstants.MSG_ENTITY_NAME);
        var name = finScanListProfileReportItem.Name.Replace(FinScanConstants.MSG_DOW_JONES_UNAVAILABLE, string.Empty).Trim();
        _excelWorksheet.Cells[$"C{_currentRow}:E{_currentRow + 3}"]
            .MergeAndWrap(name, CAUConstants.DEFAULT_FONT_SIZE, ExcelVerticalAlignment.Top);

        Write($"F{_currentRow}", FinScanConstants.MSG_GENDER);
        Write($"I{_currentRow}", finScanListProfileReportItem.Gender);

        Write($"F{++_currentRow}", FinScanConstants.MSG_RECORD_TYPE);
        Write($"I{_currentRow}", finScanListProfileReportItem.RecordType);

        Write($"F{++_currentRow}", FinScanConstants.MSG_UID);
        Write($"I{_currentRow}", finScanListProfileReportItem.Uid);

        Write($"F{++_currentRow}", FinScanConstants.MSG_STATUS);
        Write($"I{_currentRow}", finScanListProfileReportItem.Status);

        Write($"A{++_currentRow}", FinScanConstants.MSG_LOAD_DATE);
        _excelWorksheet.Cells[$"C{_currentRow}:E{_currentRow}"]
            .MergeAndWrap(finScanListProfileReportItem.LoadDate, CAUConstants.DEFAULT_FONT_SIZE);

        Write($"F{_currentRow}", FinScanConstants.MSG_ORIGINAL_SCRIPT_NAME);
        Write($"I{_currentRow}", finScanListProfileReportItem.OriginalScriptName);

        Write($"A{++_currentRow}", FinScanConstants.MSG_VERSION);
        Write($"C{_currentRow}", finScanListProfileReportItem.Version);

        Write($"F{_currentRow}", FinScanConstants.MSG_DELETED);
        Write($"I{_currentRow}", finScanListProfileReportItem.Deleted);

        _currentRow++;
    }    


    private void WriteDjwlDatesSection(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        SkipLine();
        List<string> headers = [FinScanConstants.MSG_COL_DATE_TYPE,
            null,
            null,
            FinScanConstants.MSG_COL_DAY,
            FinScanConstants.MSG_COL_MONTH,
            FinScanConstants.MSG_COL_YEAR,
            FinScanConstants.MSG_COL_DATE_NOTES,
            null,
            null,
            null];
        // In the previous statement, null means Empty cell.
        WriteColumnHeaders($"A{++_currentRow}", headers);

        if (finScanListProfileReportItem.DatesColl.IsNullOrEmpty())
        {
            WriteNA($"A{++_currentRow}", headers.Count);
            return;
        }

        foreach (var date in finScanListProfileReportItem.DatesColl)
        {
            ++_currentRow;
            _excelWorksheet.Cells[$"A{_currentRow}:C{_currentRow}"].MergeAndWrap(date.DateType);
            _excelWorksheet.Cells[$"D{_currentRow}"].Value = date.DateValue.Day;
            _excelWorksheet.Cells[$"E{_currentRow}"].Value = date.DateValue.Month;
            _excelWorksheet.Cells[$"F{_currentRow}"].Value = date.DateValue.Year;
            _excelWorksheet.Cells[$"G{_currentRow}:J{_currentRow}"].MergeAndWrap(date.DateValue.Dnotes);
            _excelWorksheet.AutoFitRowHeight(_currentRow,
                startCol: 'A', endCol: 'J', allMergedCols: [('A', 'C'), ('G', 'J')], 
                forceWrapText: true, minHeight: CAUConstants.STD_MIN_HEIGHT);
        }
    }


    private void WriteDjwlDescriptionsSection(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        SkipLine();
        List<string> headers = [FinScanConstants.MSG_COL_DESCRIPTION_1,
            null,
            null,            
            FinScanConstants.MSG_COL_DESCRIPTION_2,
            null,
            null,
            FinScanConstants.MSG_COL_DESCRIPTION_3,
            null,
            null,
            null];
        // In the previous statement, null means Empty cell.
        WriteColumnHeaders($"A{++_currentRow}", headers, CAUConstants.SMALL_FONT_SIZE);

        if (finScanListProfileReportItem.DescriptionsColl.IsNullOrEmpty())
        {
            WriteNA($"A{++_currentRow}", headers.Count, CAUConstants.SMALL_FONT_SIZE);
            return;
        }

        foreach (var description in finScanListProfileReportItem.DescriptionsColl)
        {
            ++_currentRow;
            _excelWorksheet.Cells[$"A{_currentRow}:C{_currentRow}"]
                .MergeAndWrap(description.Description1, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.Cells[$"D{_currentRow}:F{_currentRow}"]
                .MergeAndWrap(description.Description2, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.Cells[$"G{_currentRow}:J{_currentRow}"]
                .MergeAndWrap(description.Description3, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.AutoFitRowHeight(_currentRow,
                startCol: 'A', endCol: 'J', allMergedCols: [('A', 'C'), ('D', 'F'), ('G', 'J')]);
        }
    }


    private void WriteDjwlSanctionsReferencesSection(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        SkipLine();
        List<string> headers = [FinScanConstants.MSG_COL_SANCTION_DESCRIPTION_1,
                                null,
                                FinScanConstants.MSG_COL_SANCTION_DESCRIPTION_2,
                                null,
                                FinScanConstants.MSG_COL_SANCTION_NAME,
                                null,
                                null,
                                FinScanConstants.MSG_COL_SINCE_DATE,
                                FinScanConstants.MSG_COL_STATUS,
                                FinScanConstants.MSG_COL_TO_DATE];
        // In the previous statement, null means Empty cell.
        WriteColumnHeaders($"A{++_currentRow}", headers, CAUConstants.SMALL_FONT_SIZE);

        if (finScanListProfileReportItem.SanctionsReferencesColl.IsNullOrEmpty())
        {
            WriteNA($"A{++_currentRow}", headers.Count);
            return;
        }

        foreach (var sanctionRef in finScanListProfileReportItem.SanctionsReferencesColl)
        {
            ++_currentRow;
            _excelWorksheet.Cells[$"A{_currentRow}:B{_currentRow}"]
                .MergeAndWrap(sanctionRef.Description1, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.Cells[$"C{_currentRow}:D{_currentRow}"]
                .MergeAndWrap(sanctionRef.Description2, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.Cells[$"E{_currentRow}:G{_currentRow}"]
                .MergeAndWrap(sanctionRef.Name, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.Cells[$"H{_currentRow}"].WriteData(
                FormatDate(sanctionRef.SinceYear, sanctionRef.SinceMonth, sanctionRef.SinceDay), CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.Cells[$"I{_currentRow}"].WriteData(sanctionRef.Status, CAUConstants.SMALL_FONT_SIZE); 
            _excelWorksheet.Cells[$"J{_currentRow}"].WriteData(
                FormatDate(sanctionRef.ToYear, sanctionRef.ToMonth, sanctionRef.ToDay), CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.AutoFitRowHeight(_currentRow,
                startCol: 'A', endCol: 'J', allMergedCols: [('A', 'B'), ('C', 'D'), ('E', 'G')]);
        }
    }


    private void WriteDjwlNamesSectionForIndividual(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        SkipLine();
        List<string> headers = [FinScanConstants.MSG_COL_NAME_TYPE,
            FinScanConstants.MSG_COL_TITLE_HONORIFIC,
            FinScanConstants.MSG_COL_FIRST_NAME,
            FinScanConstants.MSG_COL_MIDDLE_NAME,
            FinScanConstants.MSG_COL_SURNAME,
            FinScanConstants.MSG_COL_SUFFIX,
            FinScanConstants.MSG_COL_MAIDEN_NAME,
            FinScanConstants.MSG_COL_ENTITY_NAME,
            FinScanConstants.MSG_COL_ORIGINAL_SCRIPT_NAME,
            FinScanConstants.MSG_COL_SINGLE_STRING_NAME];
        WriteColumnHeaders($"A{++_currentRow}", headers, CAUConstants.SMALL_FONT_SIZE);

        if (finScanListProfileReportItem.NamesColl.IsNullOrEmpty())
        {
            WriteNA($"A{++_currentRow}", headers.Count, CAUConstants.SMALL_FONT_SIZE);
            return;
        }

        foreach (var name in finScanListProfileReportItem.NamesColl)
        {
            WriteColumnContents($"A{++_currentRow}",
                [name.NameType,
                 name.NameValue.TitleHonorific,
                 name.NameValue.FirstName,
                 name.NameValue.MiddleName,
                 name.NameValue.Surname,
                 name.NameValue.Suffix,
                 name.NameValue.MaidenName,
                 name.NameValue.EntityName,
                 name.NameValue.OriginalScriptName,
                 name.NameValue.SingleStringName],
                CAUConstants.SMALL_FONT_SIZE);
        }
    }


    private void WriteDjwlNamesSectionForEntity(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        SkipLine();
        List<string> headers = [FinScanConstants.MSG_COL_NAME_TYPE,
            null,
            null,
            FinScanConstants.MSG_COL_SUFFIX,
            FinScanConstants.MSG_COL_ENTITY_NAME,
            null,
            null,
            FinScanConstants.MSG_COL_ORIGINAL_SCRIPT_NAME,
            null,
            null];
        // In the previous statement, null means Empty cell.
        WriteColumnHeaders($"A{++_currentRow}", headers, CAUConstants.SMALL_FONT_SIZE);

        if (finScanListProfileReportItem.NamesColl.IsNullOrEmpty())
        {
            WriteNA($"A{++_currentRow}", headers.Count, CAUConstants.SMALL_FONT_SIZE);
            return;
        }

        foreach (var name in finScanListProfileReportItem.NamesColl)
        {
            ++_currentRow;
            _excelWorksheet.Cells[$"A{_currentRow}:C{_currentRow}"]
                .MergeAndWrap(name.NameType, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.Cells[$"D{_currentRow}"].WriteData(name.NameValue.Suffix, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.Cells[$"E{_currentRow}:G{_currentRow}"]
                .MergeAndWrap(name.NameValue.EntityName, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.Cells[$"H{_currentRow}:J{_currentRow}"]
                .MergeAndWrap(name.NameValue.OriginalScriptName, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.AutoFitRowHeight(_currentRow,
                startCol: 'A', endCol: 'J', allMergedCols: [('A', 'C'), ('E', 'G'), ('H', 'J')]);
        }
    }  


    private void WriteDjwlAddressesSection(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        SkipLine();
        List<string> headers = [FinScanConstants.MSG_COL_ADDRESS_CITY,
            null,
            null,
            FinScanConstants.MSG_COL_ADDRESS_COUNTRY,
            null,
            null,
            FinScanConstants.MSG_COL_ADDRESS_LINE,
            null,
            null,
            null];
        // In the previous statement, null means Empty cell.
        WriteColumnHeaders($"A{++_currentRow}", headers);

        if (finScanListProfileReportItem.AddressesColl.IsNullOrEmpty())
        {
            WriteNA($"A{++_currentRow}", headers.Count);
            return;
        }

        foreach (var address in finScanListProfileReportItem.AddressesColl)
        {
            ++_currentRow;
            _excelWorksheet.Cells[$"A{_currentRow}:C{_currentRow}"].MergeAndWrap(address.AddressCity);
            _excelWorksheet.Cells[$"D{_currentRow}:F{_currentRow}"].MergeAndWrap(address.AddressCountry);
            _excelWorksheet.Cells[$"G{_currentRow}:J{_currentRow}"].MergeAndWrap(address.AddressLine);
            _excelWorksheet.AutoFitRowHeight(_currentRow, startCol: 'A', endCol: 'J', 
                allMergedCols: [('A', 'C'), ('D', 'F'), ('G', 'J')]);
        }
    }


    private void WriteDjwlIdsSection(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        SkipLine();
        List<string> headers = [FinScanConstants.MSG_COL_ID_TYPE,
            null,
            null,
            null,
            FinScanConstants.MSG_COL_VALUE,
            null,
            FinScanConstants.MSG_COL_ID_NOTES,
            null,
            null,
            null];
        // In the previous statement, null means Empty cell.
        WriteColumnHeaders($"A{++_currentRow}", headers);

        if (finScanListProfileReportItem.IdsColl.IsNullOrEmpty())
        {
            WriteNA($"A{++_currentRow}", headers.Count);
            return;
        }

        foreach (var id in finScanListProfileReportItem.IdsColl)
        {
            ++_currentRow;
            _excelWorksheet.Cells[$"A{_currentRow}:D{_currentRow}"].MergeAndWrap(id.IDType);
            _excelWorksheet.Cells[$"E{_currentRow}:F{_currentRow}"].MergeAndWrap(id.IDValue.Value);
            _excelWorksheet.Cells[$"G{_currentRow}:J{_currentRow}"].MergeAndWrap(id.IDValue.IDnotes);
            _excelWorksheet.AutoFitRowHeight(_currentRow, startCol: 'A', endCol: 'J', 
                allMergedCols: [('A', 'D'), ('E', 'F'), ('G', 'J')]);
        }
    }


    private void WriteDjwlCountriesSection(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        SkipLine();
        List<string> headers = [FinScanConstants.MSG_COL_COUNTRY_TYPE,
            null,
            null,
            null,
            null,
            FinScanConstants.MSG_COL_COUNTRY,
            null,
            null,
            null,
            null];
        // In the previous statement, null means Empty cell.
        WriteColumnHeaders($"A{++_currentRow}", headers);

        if (finScanListProfileReportItem.CountriesColl.IsNullOrEmpty())
        {
            WriteNA($"A{++_currentRow}", headers.Count);
            return;
        }

        foreach (var country in finScanListProfileReportItem.CountriesColl)
        {
            ++_currentRow;
            _excelWorksheet.Cells[$"A{_currentRow}:E{_currentRow}"].MergeAndWrap(country.CountryType);
            _excelWorksheet.Cells[$"F{_currentRow}:J{_currentRow}"].MergeAndWrap(country.CountryValue.Country);
            _excelWorksheet.AutoFitRowHeight(_currentRow, startCol: 'A', endCol: 'J',
                allMergedCols: [('A', 'E'), ('F', 'J')]);
        }
    }


    private void WriteDjwlBirthPlaceSectionIndividual(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        SkipLine();
        List<string> headers = [FinScanConstants.MSG_COL_BIRTH_PLACE, null];
        // In the previous statement, null means Empty cell.
        WriteColumnHeaders($"A{++_currentRow}", headers);

        if (finScanListProfileReportItem.BirthPlaceColl.IsNullOrEmpty())
        {
            WriteNA($"A{++_currentRow}", headers.Count);
            return;
        }

        foreach (var birthPlace in finScanListProfileReportItem.BirthPlaceColl)
        {
            WriteColumnContents($"A{++_currentRow}", [birthPlace.name, null]);
            // In the previous statement, null means Empty cell.
        }
    }


    private void WriteDjwlAssociatesSection(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        SkipLine();
        List<string> headers = [FinScanConstants.MSG_COL_ASSOCIATE_NAME,
            null,
            null,
            FinScanConstants.MSG_COL_EX,
            FinScanConstants.MSG_COL_UID,
            FinScanConstants.MSG_COL_RELATIONSHIP,
            null,
            FinScanConstants.MSG_COL_TYPE,
            null,
            FinScanConstants.MSG_COL_PEP];
        // In the previous statement, null means Empty cell.
        WriteColumnHeaders($"A{++_currentRow}", headers, CAUConstants.SMALL_FONT_SIZE);

        if (finScanListProfileReportItem.AssociatesColl.IsNullOrEmpty())
        {
            WriteNA($"A{++_currentRow}", headers.Count, CAUConstants.SMALL_FONT_SIZE);
            return;
        }

        foreach (var associate in finScanListProfileReportItem.AssociatesColl)
        {
            string pep = ((associate.pep.FullTrim().Equals(FinScanConstants.MSG_YES, StringComparison.CurrentCultureIgnoreCase))
                                             || (associate.pep.FullTrim().Equals(FinScanConstants.MSG_NO, StringComparison.CurrentCultureIgnoreCase)))
                         ? associate.pep : string.Empty;

            ++_currentRow;
            _excelWorksheet.Cells[$"A{_currentRow}:C{_currentRow}"]
                .MergeAndWrap(associate.singleStringName, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.Cells[$"D{_currentRow}"].WriteData(associate.ex, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.Cells[$"E{_currentRow}"].WriteData(associate.id, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.Cells[$"F{_currentRow}:G{_currentRow}"]
                .MergeAndWrap(associate.relationship, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.Cells[$"H{_currentRow}:I{_currentRow}"]
                .MergeAndWrap(associate.type, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.Cells[$"J{_currentRow}"].WriteData(pep, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.AutoFitRowHeight(_currentRow, startCol: 'A', endCol: 'J',
                allMergedCols: [('A', 'C'), ('F', 'G'), ('H', 'I')]);
        }
    }


    private void WriteDjwlProfileNotesSection(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        SkipLine();
        WriteBanner($"A{++_currentRow}", FinScanConstants.MSG_COL_PROFILE_NOTES, REPORT_COL_COUNT,
                    CAUConstants.DEFAULT_FONT_SIZE, isBold: true, colorHeaderForeground, colorHeaderBackground);

        string profileNotes = string.IsNullOrWhiteSpace(finScanListProfileReportItem.ProfileNotes) ? FinScanConstants.MSG_NA :
                                finScanListProfileReportItem.ProfileNotes.RemovePrefixes("\n");
        if (profileNotes.Length > MAX_CHARACTERS_PER_CEL)
        {
            // When a cell is assigned more than 32767 
            // characters, the workbook becomes corrupted.
            profileNotes = profileNotes[..MAX_CHARACTERS_PER_CEL];
        }
        WriteMultipleLinesLongText($"A{++_currentRow}", profileNotes, CAUConstants.SMALL_FONT_SIZE);
        ++_currentRow;
    }


    private void WriteDjwlImagesSectionForIndividual(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        SkipLine();
        List<string> headers = [FinScanConstants.MSG_COL_IMAGES,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null];
        // In the previous statement, null means Empty cell.
        WriteColumnHeaders($"A{++_currentRow}", headers);

        if (finScanListProfileReportItem.ImagesColl.IsNullOrEmpty())
        {
            WriteNA($"A{++_currentRow}", headers.Count);
            return;
        }


        foreach (var image in finScanListProfileReportItem.ImagesColl)
        {
            var startingCell = _excelWorksheet.Cells[$"A{++_currentRow}"];
            startingCell.WriteHyperlink(image.URL, image.URL);
            startingCell.Offset(0, 0, 1, REPORT_COL_COUNT).Merge = true;
            startingCell.SetFontSize(CAUConstants.SMALL_FONT_SIZE);
        }
    }


    private void WriteDjwlSourcesSection(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        SkipLine();
        List<string> headers = [FinScanConstants.MSG_COL_SOURCE_DESCRIPTION_NAME,
                                null,
                                null,
                                null,
                                null,
                                null,
                                null,
                                null,
                                null,
                                null];
        WriteColumnHeaders($"A{++_currentRow}", headers);

        if (finScanListProfileReportItem.SourcesColl.IsNullOrEmpty())
        {
            WriteNA($"A{++_currentRow}", headers.Count);
            return;
        }

        foreach (var source in finScanListProfileReportItem.SourcesColl)
        {
            var startingCell = _excelWorksheet.Cells[$"A{++_currentRow}"];
            startingCell.WriteHyperlink(source.name, source.name);
            startingCell.Offset(0, 0, 1, REPORT_COL_COUNT).Merge = true;
            startingCell.SetFontSize(CAUConstants.SMALL_FONT_SIZE);
        }
    }


    private void RunWithDjsocLayout(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        // DJSOC List Profile report depends on JSON FinScan Response node: 
        //   searchResults[]\searchMatches[]\listRecordDetail\listRecord\innovative
        // Empty/null sections are not displayed as part of the report.

        WriteDjsocHeaderSection(finScanListProfileReportItem);
        WriteDjsocAddressesSection(finScanListProfileReportItem);
        WriteDjsocDatesSection(finScanListProfileReportItem);
        WriteDjsocEntitiesSection(finScanListProfileReportItem);
        WriteDjsocPersonsSection(finScanListProfileReportItem);
        WriteDjsocIdsSection(finScanListProfileReportItem);
        WriteDjsocTextInformationsSection(finScanListProfileReportItem);
        WriteDjsocTrackingInformationsSection(finScanListProfileReportItem);
        WriteDjsocURLsSection(finScanListProfileReportItem);
    }


    private void WriteDjsocHeaderSection(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        string uid = string.IsNullOrWhiteSpace(finScanListProfileReportItem.Uid) ? string.Empty
                        : $"{new string(' ', 4)}(UID {finScanListProfileReportItem.Uid})";
        SkipLine();
        WriteBanner($"A{++_currentRow}", $"{finScanListProfileReportItem.Name}{uid}", REPORT_COL_COUNT,
                    CAUConstants.LARGE_FONT_SIZE, true, colorHeaderForeground, colorHeaderBackground);
        
        Write($"A{++_currentRow}", FinScanConstants.MSG_LIST_NAME);
        _excelWorksheet.Cells[$"C{_currentRow}:F{_currentRow}"]
            .MergeAndWrap(ReportType(finScanListProfileReportItem.ListId), CAUConstants.DEFAULT_FONT_SIZE);

        Write($"G{_currentRow}", FinScanConstants.MSG_LIST_CODE);
        Write($"I{_currentRow}", finScanListProfileReportItem.ListId.ToUpper());


        Write($"A{++_currentRow}", FinScanConstants.MSG_FULL_NAME);
        var name = finScanListProfileReportItem.Name.Replace(FinScanConstants.MSG_INNOVATIVE_UNAVAILABLE, string.Empty).Trim();
        _excelWorksheet.Cells[$"C{_currentRow}:F{_currentRow}"]
            .MergeAndWrap(name, CAUConstants.DEFAULT_FONT_SIZE);

        Write($"G{_currentRow}", FinScanConstants.MSG_RECORD_TYPE);
        Write($"I{_currentRow}", finScanListProfileReportItem.RecordType);


        Write($"A{++_currentRow}", FinScanConstants.MSG_ACTIVE_STATUS);
        Write($"C{_currentRow}", finScanListProfileReportItem.Status);

        Write($"G{_currentRow}", FinScanConstants.MSG_UID);
        Write($"I{_currentRow}", finScanListProfileReportItem.Uid);


        Write($"A{++_currentRow}", FinScanConstants.MSG_LOAD_DATE);
        _excelWorksheet.Cells[$"C{_currentRow}:F{_currentRow}"]
            .MergeAndWrap(finScanListProfileReportItem.LoadDate, CAUConstants.DEFAULT_FONT_SIZE);

        Write($"G{_currentRow}", FinScanConstants.MSG_LIST_VERSION);
        Write($"I{_currentRow}", finScanListProfileReportItem.Version);

        
        Write($"A{++_currentRow}", FinScanConstants.MSG_DELETED);
        Write($"C{_currentRow}", finScanListProfileReportItem.Deleted);

        _currentRow++;
    }


    private void WriteDjsocAddressesSection(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        if (finScanListProfileReportItem.InnovativeAddressesColl.IsNullOrEmpty())
        {
            return;
        }

        SkipLine();
        List<string> headers = [FinScanConstants.MSG_COL_ADDRESS_TYPE,
            null,
            FinScanConstants.MSG_COL_ADDRESS_LINE_1,
            null,
            null,
            FinScanConstants.MSG_COL_CITY_LINE,
            null,
            null,
            FinScanConstants.MSG_COL_COUNTRY,
            null];
        // In the previous statement, null means Empty cell.
        WriteColumnHeaders($"A{++_currentRow}", headers);

        // if (finScanListProfileReportItem.InnovativeAddressesColl.IsNullOrEmpty())
        // {
        //     WriteNA($"A{++_currentRow}", headers.Count);
        //     return;
        // }

        foreach (var address in finScanListProfileReportItem.InnovativeAddressesColl)
        {
            ++_currentRow;
            
            _excelWorksheet.Cells[$"A{_currentRow}:B{_currentRow}"]
                .MergeAndWrap(address.AddressType, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.Cells[$"C{_currentRow}:E{_currentRow}"]
                .MergeAndWrap(address.AddressLine1, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.Cells[$"F{_currentRow}:H{_currentRow}"]
                .MergeAndWrap(address.CityLine, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.Cells[$"I{_currentRow}:J{_currentRow}"]
                .MergeAndWrap(address.Country, CAUConstants.SMALL_FONT_SIZE);

            _excelWorksheet.AutoFitRowHeight(_currentRow, startCol: 'A', endCol: 'J',
                allMergedCols: [('A', 'B'), ('C', 'E'), ('F', 'H'), ('I', 'J')]);
        }
    }


    private void WriteDjsocDatesSection(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        if (finScanListProfileReportItem.InnovativeDatesColl.IsNullOrEmpty())
        {
            return;
        }

        SkipLine();
        List<string> headers = [FinScanConstants.MSG_COL_DATE_TYPE,
            null,
            null,
            FinScanConstants.MSG_COL_ORIGINAL_TYPE,
            null,
            null,
            null,
            FinScanConstants.MSG_COL_VALUE,
            null,
            null];
        // In the previous statement, null means Empty cell.
        WriteColumnHeaders($"A{++_currentRow}", headers);

        // if (finScanListProfileReportItem.InnovativeDatesColl.IsNullOrEmpty())
        // {
        //     WriteNA($"A{++_currentRow}", headers.Count);
        //     return;
        // }

        foreach (var date in finScanListProfileReportItem.InnovativeDatesColl)
        {
            ++_currentRow;
            _excelWorksheet.Cells[$"A{_currentRow}:C{_currentRow}"].MergeAndWrap(date.Type);
            _excelWorksheet.Cells[$"D{_currentRow}:G{_currentRow}"].MergeAndWrap(date.OriginalType);
            _excelWorksheet.Cells[$"H{_currentRow}:J{_currentRow}"].MergeAndWrap(FormatDate(date.Value));
            _excelWorksheet.AutoFitRowHeight(_currentRow,
                startCol: 'A', endCol: 'J', allMergedCols: [('A', 'C'), ('D', 'G'), ('H', 'J')],
                forceWrapText: true, minHeight: CAUConstants.STD_MIN_HEIGHT);
        }
    }


    private void WriteDjsocEntitiesSection(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        if (finScanListProfileReportItem.InnovativeEntitiesColl.IsNullOrEmpty())
        {
            return;
        }

        List<string> headers = [FinScanConstants.MSG_COL_ENTITY,
            null, null, null, null, null, null, null, null, null];
        // In the previous statement, null means Empty cell.

        // if (finScanListProfileReportItem.InnovativeEntitiesColl.IsNullOrEmpty())
        // {
        //     WriteNA($"A{++_currentRow}", headers.Count);
        //     return;
        // }

        SkipLine();
        WriteColumnHeaders($"A{++_currentRow}", headers);

        foreach (var entity in finScanListProfileReportItem.InnovativeEntitiesColl)
        {
            ++_currentRow;

            _excelWorksheet.Cells[$"A{_currentRow}:J{_currentRow}"].MergeAndWrap(entity);
        }
    }


    private void WriteDjsocPersonsSection(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        if (finScanListProfileReportItem.InnovativePersonsColl.IsNullOrEmpty())
        {
            return;
        }

        List<string> headers = [FinScanConstants.MSG_COL_PERSON,
            null, null, null, null, null, null, null, null, null];
        // In the previous statement, null means Empty cell.

        // if (finScanListProfileReportItem.InnovativePersonsColl.IsNullOrEmpty())
        // {
        //     WriteNA($"A{++_currentRow}", headers.Count);
        //     return;
        // }

        SkipLine();
        WriteColumnHeaders($"A{++_currentRow}", headers);

        foreach (var person in finScanListProfileReportItem.InnovativePersonsColl)
        {
            ++_currentRow;

            _excelWorksheet.Cells[$"A{_currentRow}:J{_currentRow}"].MergeAndWrap(person);
        }
    }


    private void WriteDjsocIdsSection(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        if (finScanListProfileReportItem.InnovativeIDsColl.IsNullOrEmpty())
        {
            return;
        }

        SkipLine();
        List<string> headers = [FinScanConstants.MSG_COL_ID_NUMBER_TYPE,
            null,
            FinScanConstants.MSG_COL_SUBTYPE,
            null,
            FinScanConstants.MSG_COL_VALUE,
            null,
            null,
            FinScanConstants.MSG_COL_COUNTRY_ISSUED, 
            null,
            null];
        // In the previous statement, null means Empty cell.
        WriteColumnHeaders($"A{++_currentRow}", headers);

        // if (finScanListProfileReportItem.InnovativeIDsColl.IsNullOrEmpty())
        // {
        //     WriteNA($"A{++_currentRow}", headers.Count);
        //     return;
        // }

        foreach (var id in finScanListProfileReportItem.InnovativeIDsColl)
        {
            ++_currentRow;
            _excelWorksheet.Cells[$"A{_currentRow}:B{_currentRow}"].MergeAndWrap(id.IDNumberType);
            _excelWorksheet.Cells[$"C{_currentRow}:D{_currentRow}"].MergeAndWrap(id.Subtype);
            _excelWorksheet.Cells[$"E{_currentRow}:G{_currentRow}"].MergeAndWrap(id.Value);
            _excelWorksheet.Cells[$"H{_currentRow}:J{_currentRow}"].MergeAndWrap(id.CountryIssued);
            _excelWorksheet.AutoFitRowHeight(_currentRow, startCol: 'A', endCol: 'J',
                allMergedCols: [('A', 'B'), ('C', 'D'), ('E', 'G'), ('H', 'J')]);
        }
    }


    private void WriteDjsocTextInformationsSection(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        if (finScanListProfileReportItem.InnovativeTextInformationsColl.IsNullOrEmpty())
        {
            return;
        }

        SkipLine();
        List<string> headers = [FinScanConstants.MSG_COL_TEXT_INFO_ORIGINAL_TYPE,
            null,
            null,
            FinScanConstants.MSG_COL_TEXT_INFORMATION,
            null,
            null,
            null,
            null,
            null,
            null];
        // In the previous statement, null means Empty cell.
        WriteColumnHeaders($"A{++_currentRow}", headers);

        // if (finScanListProfileReportItem.InnovativeTextInformationsColl.IsNullOrEmpty())
        // {
        //     WriteNA($"A{++_currentRow}", headers.Count);
        //     return;
        // }

        foreach (var textInfo in finScanListProfileReportItem.InnovativeTextInformationsColl)
        {
            ++_currentRow;

            _excelWorksheet.Cells[$"A{_currentRow}:C{_currentRow}"]
                .MergeAndWrap(textInfo.OriginalType, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.Cells[$"D{_currentRow}:J{_currentRow}"]
                .MergeAndWrap(textInfo.TextInformation, CAUConstants.SMALL_FONT_SIZE);

            _excelWorksheet.AutoFitRowHeight(_currentRow, startCol: 'A', endCol: 'J',
                allMergedCols: [('A', 'C'), ('D', 'J')]);
        }
    }


    private void WriteDjsocTrackingInformationsSection(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        if (finScanListProfileReportItem.InnovativeTrackingInformationsColl.IsNullOrEmpty())
        {
            return;
        }

        SkipLine();
        List<string> headers = [FinScanConstants.MSG_COL_TRACKING_INFORMATION,
            null,
            null,
            FinScanConstants.MSG_COL_VALUE,
            null,
            null,
            FinScanConstants.MSG_COL_ORIGINAL_TYPE,
            null,
            null,
            null];
        // In the previous statement, null means Empty cell.
        WriteColumnHeaders($"A{++_currentRow}", headers);

        // if (finScanListProfileReportItem.InnovativeTrackingInformationsColl.IsNullOrEmpty())
        // {
        //     WriteNA($"A{++_currentRow}", headers.Count);
        //     return;
        // }

        foreach (var trackingInfo in finScanListProfileReportItem.InnovativeTrackingInformationsColl)
        {
            ++_currentRow;

            _excelWorksheet.Cells[$"A{_currentRow}:C{_currentRow}"]
                .MergeAndWrap(trackingInfo.Type);
            _excelWorksheet.Cells[$"D{_currentRow}:F{_currentRow}"]
                .MergeAndWrap(trackingInfo.Value);
            _excelWorksheet.Cells[$"G{_currentRow}:J{_currentRow}"]
                .MergeAndWrap(trackingInfo.OriginalType);

            _excelWorksheet.AutoFitRowHeight(_currentRow, startCol: 'A', endCol: 'J',
                allMergedCols: [('A', 'C'), ('D', 'F'), ('G', 'J')]);
        }
    }


    private void WriteDjsocURLsSection(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        if (finScanListProfileReportItem.InnovativeURLsColl.IsNullOrEmpty())
        {
            return;
        }

        SkipLine();
        List<string> headers = [FinScanConstants.MSG_COL_URL_TYPE,
            null,
            FinScanConstants.MSG_COL_URL_VALUE,
            null,
            null,
            null,
            null,
            null,
            FinScanConstants.MSG_COL_URL_ORIGINAL_TYPE,
            null];
        // In the previous statement, null means Empty cell.
        WriteColumnHeaders($"A{++_currentRow}", headers);

        // if (finScanListProfileReportItem.InnovativeURLsColl.IsNullOrEmpty())
        // {
        //     WriteNA($"A{++_currentRow}", headers.Count);
        //     return;
        // }

        foreach (var url in finScanListProfileReportItem.InnovativeURLsColl)
        {
            ++_currentRow;

            _excelWorksheet.Cells[$"A{_currentRow}:B{_currentRow}"]
                .MergeAndWrap(url.Type, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.Cells[$"C{_currentRow}:H{_currentRow}"]
                .MergeAndWrap(url.Value, CAUConstants.SMALL_FONT_SIZE);
            _excelWorksheet.Cells[$"I{_currentRow}:J{_currentRow}"]
                .MergeAndWrap(url.OriginalType, CAUConstants.SMALL_FONT_SIZE);

            _excelWorksheet.AutoFitRowHeight(_currentRow, startCol: 'A', endCol: 'J',
                allMergedCols: [('A', 'B'), ('C', 'H'), ('I', 'J')]);
        }
    }


    private void RunWithKharonLayout(FinScanListProfileReportItem finScanListProfileReportItem)
    {
        // Same as DJSOC List Profile Report. It also depends on JSON FinScan Response node: 
        //   searchResults[]\searchMatches[]\listRecordDetail\listRecord\innovative
        // Empty/null sections are not displayed as part of the report.
        RunWithDjsocLayout(finScanListProfileReportItem);
    }
}
