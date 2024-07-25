using ConflictAutomation.Constants;
using ConflictAutomation.enums;
using ConflictAutomation.Extensions;
using ConflictAutomation.Models.PreScreening;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace ConflictAutomation.Services.PreScreening;

#pragma warning disable IDE0063 // Use simple 'using' statement
#pragma warning disable IDE0059 // Unnecessary assignment of a value

public class PreScreeningWriter(string destinationFilePath)
{
    private const string MSG_NO_RECORDS = "-";  // "No records found";
    private const string MSG_NO_DATA = "-";  // "NA";
    private const double SINGLE_ROW_HEIGHT = 13.5;

    private readonly Color _sectionBackgroundColor = ColorTranslator.FromHtml(CAUConstants.COLOR_ORANGE);
    private readonly Color _headerBackgroundColor = ColorTranslator.FromHtml(CAUConstants.COLOR_LIGHT_GRAY);
    private readonly Color _dataBackgroundColor = ColorTranslator.FromHtml(CAUConstants.COLOR_TEAL);

    private readonly string _destinationFilePath = destinationFilePath;
    private ExcelWorksheet _worksheet;
    private PreScreeningInfo _preScreeningInfo;


    public void Run(PreScreeningInfo preScreeningInfo, bool isPursuitTab=false,bool isResubmitted=false)
    {
        ArgumentNullException.ThrowIfNull(preScreeningInfo);
        _preScreeningInfo = preScreeningInfo;
        using (ExcelPackage package = new(_destinationFilePath))
        {
            _worksheet = package.GetWorksheet(isPursuitTab || isResubmitted ? CAUConstants.MASTER_WORKBOOK_PRESCREENING_INFO_PURSUIT_TAB : CAUConstants.MASTER_WORKBOOK_PRESCREENING_INFO_TAB);

            if (isPursuitTab || isResubmitted)
            {
                _worksheet.Hidden = eWorkSheetHidden.Visible;
                if (isResubmitted)
                {
                    _worksheet.Name = CAUConstants.MASTER_WORKBOOK_PRESCREENING_INFO_PREVIOUS;
                }
            }
            ArgumentNullException.ThrowIfNull(_worksheet);

            int row = 3;
            row = WriteTriggersForCheck(++row, col: 2);

            ++row;
            row = WriteNotes(++row, col: 2);

            ++row;
            row = WriteTeamMembers(++row, col: 2);

            ++row;
            row = WriteAdditionalParties(++row, col: 2);

            ++row;
            row = WriteHostileQuestion(++row, col: 2);

            ++row;
            row = WriteLimitationsToAct(++row, col: 2);

            ++row;
            row = WriteAnotherConflictCheck(++row, col: 2);

            ++row;
            row = WriteDisputeLitigationInvolvement(++row, col: 2);

            ++row;
            row = WriteHighProfileEngagement(++row, col: 2);

            ++row;
            row = WriteConsentToContactCounterparty(++row, col: 2);
            
            package.Save();
        }
    }


    private int WriteTriggersForCheck(int row, int col)
    {
        WriteSection(row, col, "Triggers For Check");        
        WriteHeaders(++row, ++col, ["Trigger Type", "Details"]);
        
        if(_preScreeningInfo.ListTriggersForCheck.IsNullOrEmpty())
        {
            WriteNoRecordsFound(++row, col, colCount: 2);
            return row;
        }

        foreach (var triggerForCheck in _preScreeningInfo.ListTriggersForCheck)
        {
            WriteData(++row, col, [GetContentsOrNaIfEmpty(triggerForCheck.TriggerType), 
                                   GetContentsOrNaIfEmpty(triggerForCheck.Details)]);
        }
        return row;
    }


    private int WriteNotes(int row, int col)
    {
        WriteSection(row, col, "Notes");
        WriteHeaders(++row, ++col, ["Created", "Created By", "Category", "Comments"]);

        if (_preScreeningInfo.ListNotes.IsNullOrEmpty())
        {
            WriteNoRecordsFound(++row, col, colCount: 4);
            return row;
        }

        foreach (var note in _preScreeningInfo.ListNotes)
        {
            WriteData(++row, col, [note.Created, note.CreatedBy, note.Category, note.Comments]);
        }
        return row;
    }


    private int WriteTeamMembers(int row, int col)
    {
        WriteSection(row, col, "Team Members");
        WriteHeaders(++row, ++col, ["Role", "Team Member", "Preparer", "Date Added"]);

        if (_preScreeningInfo.ListTeamMembers.IsNullOrEmpty())
        {
            WriteNoRecordsFound(++row, col, colCount: 4);
            return row;
        }

        foreach (var teamMember in _preScreeningInfo.ListTeamMembers)
        {
            int columnOrder = col;//Should reset to default position in each iteration
            bool hasEmail = !teamMember.Email.IsNullOrEmpty();
            string description = teamMember.Name + (hasEmail ? $" ({teamMember.Email})" : string.Empty);
            WriteCell(++row, columnOrder, GetContentsOrNaIfEmpty(teamMember.Role), _dataBackgroundColor);
            WriteCell(row, ++columnOrder, GetContentsOrNaIfEmpty(description), _dataBackgroundColor, isBold: false, (hasEmail ? 2 : 1));
            WriteCell(row, ++columnOrder, GetContentsOrNaIfEmpty(teamMember.Preparer), _dataBackgroundColor);
            WriteCell(row, ++columnOrder, GetContentsOrNaIfEmpty(teamMember.DateAdded), _dataBackgroundColor);
        }
        return row;
    }


    private int WriteAdditionalParties(int row, int col)
    {
        WriteSection(row, col, "Questionnaire Additional Parties");
        WriteHeaders(++row, ++col, ["Name", "Position", "Other Information"]);

        if (_preScreeningInfo.ListAdditionalParties.IsNullOrEmpty())
        {
            WriteNoRecordsFound(++row, col, colCount: 3);
            return row;
        }

        foreach (var additionalParty in _preScreeningInfo.ListAdditionalParties)
        {
            WriteData(++row, col, [GetContentsOrNaIfEmpty(additionalParty.Name),
                                   GetContentsOrNaIfEmpty(additionalParty.Position),
                                   GetContentsOrNaIfEmpty(additionalParty.OtherInformation)]);
        }
        return row;
    }


    private int WriteHostileQuestion(int row, int col)
    {
        WriteSection(row, col, "Hostile Question");
        WriteHeaders(++row, ++col, ["Q. No. 1.4 Answer", "Q. No. 1.4 Comments"], direction: DirectionEnum.Vertical);
        string answer = GetContentsOrNaIfEmpty(_preScreeningInfo.HostileQuestion?.Question_1_4_Answer);
        string comments = GetContentsOrNaIfEmpty(_preScreeningInfo.HostileQuestion?.Question_1_4_Comments);
        WriteData(row, ++col, [GetContentOrNA(answer)]);
        WriteData(++row, col, [GetContentOrNA(comments)]);
        return row;
    }
    private static string GetContentOrNA(string content)
    {
        return string.IsNullOrEmpty(content) || content == "N/A" ? "-" : content;
    }


    private int WriteLimitationsToAct(int row, int col)
    {
        WriteSection(row, col,
                     "Are there limitations to act for specific entities or within a market requested by the client?",
                     rowCount: 3);
        WriteHeaders(++row, ++col, ["Yes/No?"], direction: DirectionEnum.Vertical);
        string comments = string.IsNullOrEmpty(_preScreeningInfo.LimitationsToAct?.YesNo) ? "N/A" : _preScreeningInfo.LimitationsToAct?.YesNo;
        WriteData(row, ++col, [GetContentsOrNaIfEmpty(comments)]);
        return row;
    }


    private int WriteAnotherConflictCheck(int row, int col)
    {
        WriteSection(row, col, 
                     "Has another conflict check been performed in connection with this engagement?",
                     rowCount: 3);
        WriteHeaders(++row, ++col, ["Comments"], direction: DirectionEnum.Vertical);
        WriteData(row, ++col, [GetContentsOrNaIfEmpty(_preScreeningInfo.AnotherConflictCheck?.Comments)]);
        return row;
    }


    private int WriteDisputeLitigationInvolvement(int row, int col)
    {
        WriteSection(row, col, "Dispute/Litigation Involvement");
        WriteHeaders(++row, ++col, ["Comments"], direction: DirectionEnum.Vertical);
        WriteData(row, ++col, [GetContentsOrNaIfEmpty(_preScreeningInfo.DisputeLitigationInvolvement?.Comments)]);
        return row;
    }


    private int WriteHighProfileEngagement(int row, int col)
    {
        WriteSection(row, col, "Negative press coverage");
        WriteHeaders(++row, ++col, ["Yes/No?", "Comments"], direction: DirectionEnum.Vertical);
        WriteData(row, ++col, [GetContentsOrNaIfEmpty(_preScreeningInfo.HighProfileEngagement?.YesNo)]);
        WriteData(++row, col, [GetContentsOrNaIfEmpty(_preScreeningInfo.HighProfileEngagement?.Comments)]);
        return row;
    }


    private int WriteConsentToContactCounterparty(int row, int col)
    {
        WriteSection(row, col,
                     "Are there any reasons why the counterparty GCSP/LAP should not be contacted?",                     
                     rowCount: 3);
        WriteHeaders(++row, ++col, ["Comments"], direction: DirectionEnum.Vertical);
        WriteData(row, ++col, [GetContentsOrNaIfEmpty(_preScreeningInfo.ConsentToContactCounterparty?.Comments)]);
        return row;
    }


    private void WriteSection(int row, int col, string sectionTitle, int rowCount = 1)
    {
        WriteCell(row, col, sectionTitle, _sectionBackgroundColor, isBold: true, rowCount);        
    }


    private void WriteHeaders(int row, int col, List<string> headerTitles, DirectionEnum direction = DirectionEnum.Horizontal)
    {
        if(headerTitles.IsNullOrEmpty())
        {
            return;
        }

        foreach(var headerTitle in headerTitles)
        {
            WriteCell(row, col, headerTitle, _headerBackgroundColor, isBold: true);
            if(direction == DirectionEnum.Vertical)
            {
                row++;
            }
            else
            {
                col++;
            }
        }
    }


    private void WriteData(int row, int col, List<string> contents)
    {
        if(contents.IsNullOrEmpty())
        {
            return;
        }

        foreach (var content in contents)
        {
            WriteCell(row, col, content, _dataBackgroundColor);
            col++;
        }
    }


    private void WriteNoRecordsFound(int row, int col, int colCount) => 
        WriteMergedCell(row, col, MSG_NO_RECORDS, colCount);


    private void WriteMergedCell(int row, int col, string text, int colCount)
    {
        WriteData(row, col, [text]);
        var range = _worksheet.Cells[$"{col.ExcelColumnLetter()}{row}:{(col + colCount - 1).ExcelColumnLetter()}{row}"];
        range.Merge = true;
        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        range.SetBorders(ExcelBorderStyle.None, ExcelBorderStyle.Thin);
    }


    private void WriteCell(int row, int col, string text, Color backgroundColor, bool isBold = false, int rowCount = 1)
    {
        var cell = _worksheet.Cells[row, col];
        cell.Value = text;
        cell.Style.Font.Size = 8;
        cell.Style.Font.Bold = isBold;
        cell.SetBackgroundColor(backgroundColor);
        cell.SetBorders(ExcelBorderStyle.None, ExcelBorderStyle.Thin);
        if (rowCount > 1)
        {
            ResizeRangeHeight(cell, rowCount);
        }
    }


    private static void ResizeRangeHeight(ExcelRange range, int rowCount = 1)
    {
        range.Style.WrapText = true;
        range.EntireRow.Height = SINGLE_ROW_HEIGHT * rowCount;
        range.EntireRow.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
    }


    private static string GetContentsOrNaIfEmpty(string text) =>
        string.IsNullOrEmpty(text) || text == "N/A" ? MSG_NO_DATA : text;
}

#pragma warning restore IDE0059 // Unnecessary assignment of a value
#pragma warning restore IDE0063 // Use simple 'using' statement
