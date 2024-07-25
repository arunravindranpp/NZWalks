using ConflictAutomation.Constants;
using ConflictAutomation.Extensions;
using ConflictAutomation.Mappers;
using ConflictAutomation.Models;
using ConflictAutomation.Models.PreScreening;
using ConflictAutomation.Models.PreScreening.SubClasses;
using ConflictAutomation.Models.ResearchSummaryEngine;
using ConflictAutomation.Services.FinScan;
using ConflictAutomation.Services.Sorting;
using ConflictAutomation.Services.SummaryGrid;
using ConflictAutomation.Utilities.ExcelFileEmbedder;
using ConflictAutomation.Utilities.ExcelFileEmbedder.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PACE;
using Serilog;
using System.Data;
using System.Drawing;
using DataTable = System.Data.DataTable;
using RSE = ConflictAutomation.Services.ResearchSummaryEngine;

namespace ConflictAutomation.Services;

#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable CA1860 // Avoid using 'Enumerable.Any()' extension method
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0063 // Use simple 'using' statement
#pragma warning disable IDE0017 // Simplify object initialization
#pragma warning disable IDE0305 // Simplify collection initialization
#pragma warning disable CA1827 // Do not use Count() or LongCount() when Any() can be used
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable IDE0018 // Inline variable declaration
#pragma warning disable CS0164 // This label has not been referenced
#pragma warning disable CA1866 // Use char overload

public class ExcelOperations
{
    private const int RESULT_DETAILS_SECTIONS_DEFAULT_STARTING_ROW = 17;
    private const int RESULT_DETAILS_SECTIONS_FIRST_SECTION_NUM = RESULT_DETAILS_SECTION_NUM_GIS;
    private const int RESULT_DETAILS_SECTION_NUM_GIS = 6;                     // Starting Row   +0
    private const int RESULT_DETAILS_SECTION_NUM_CER = 7;                     // Starting Row  +50
    private const int RESULT_DETAILS_SECTION_NUM_CRR = 8;                     // Starting Row +100
    private const int RESULT_DETAILS_SECTION_NUM_SPL = 9;                     // Starting Row +150
    private const int RESULT_DETAILS_SECTION_NUM_FINSCAN = 10;                // Starting Row +200
    private const int RESULT_DETAILS_SECTION_NUM_LOCAL_AND_EXTERNAL_DBS = 11;  // Starting Row +250

    private const string RESEARCH_SUMMARY_STARTING_CELL = "L5";

    private static readonly int resultDetailsSectionsHeightInRows =
        Program.KeyValuePairs.GetValueAsInt("RESULT_DETAILS_SECTIONS_HEIGHT_IN_ROWS", defaultValue: 50);
    private static readonly Color darkGray = Color.FromArgb(191, 191, 191);
    private static readonly Color brightTeal = Color.FromArgb(183, 222, 232);

    private static string _summaryRowReference = "";
    private static int _resultDetailsSectionsStartingRow;

    // private static int _debugSaveBotUnitTab_CallNumber = 0;

    public static void SaveSummaryTab(ConflictCheck conflictCheck, CheckerQueue queue, string existingFilePath, string span = "")
    {
        try
        {
            var pursuitCheckPerformed = queue.questionnaireSummary.FirstOrDefault(i => i.Title == "Pursuit Check Performed");
            var contractuallylimit = queue.questionnaireSummary.FirstOrDefault(i => i.Title == "Contractually limit our ability");
            var hostile = queue.questionnaireSummary.Where(i => i.Title == "Hostile" && (i.QuestionNumber == "041349" || i.QuestionNumber == "597835") && (i.Answer == "Yes")).ToList().Count > 0
                ? queue.questionnaireSummary.FirstOrDefault(i => i.Title == "Hostile" && (i.QuestionNumber == "041349" || i.QuestionNumber == "597835") && (i.Answer == "Yes"))
                : queue.questionnaireSummary.FirstOrDefault(i => i.Title == "Hostile" && (i.QuestionNumber == "041349" || i.QuestionNumber == "597835") && (i.Answer == "No"));

            var disputeLitigation = queue.questionnaireSummary.FirstOrDefault(i => i.Title == "Dispute/Litigation");
            var auditPartner = queue.questionnaireSummary.FirstOrDefault(i => i.Title == "Whether to contact counterparty (G)CSP/audit partner?");
            var auction = queue.questionnaireSummary.FirstOrDefault(i => i.Title == "Auctioned");
            var govtEntity = queue.questionnaireSummary.FirstOrDefault(i => i.Title == "Government entity involved");
            int attachmentsCount = 0;
            if (conflictCheck.ConflictCheckType == "Pursuit")
                attachmentsCount = Convert.ToInt32(conflictCheck.ConflictCheckServicesAttachments.Count);
            else
                if (conflictCheck.Assessment?.AttachmentsCount.Length > 0)
                attachmentsCount = Convert.ToInt32(conflictCheck.Assessment.AttachmentsCount);
            using (var package = new ExcelPackage(new FileInfo(existingFilePath)))
            {

                var worksheet = package.Workbook.Worksheets.Where(x => x.Name == CAUConstants.MASTER_WORKBOOK_SUMMARY_TAB).FirstOrDefault();
                if (worksheet == null)
                    return;
                worksheet.Cells["C4"].Value = queue.ConflictCheckType;
                worksheet.Cells["C5"].Value = queue.Region + "/" + queue.CountryName;
                worksheet.Cells["C6"].Value = queue.ClientName;
                worksheet.Cells["C7"].Value = queue.EngagementName;
                worksheet.Cells["C8"].Value = queue.SubServiceLine;
                worksheet.Cells["C8"].Style.WrapText = true;
                worksheet.Cells["C9"].Value = queue.Services;
                worksheet.Cells["C9"].Style.WrapText = true;
                worksheet.Cells["C10"].Value = queue.checkPerformed = pursuitCheckPerformed?.Answer;
                worksheet.Cells["C11"].Value = queue.SubTypeofCheck = conflictCheck.Resubmitted == true ? "Resubmitted check" : "-";
                worksheet.Cells["C12"].Value = queue.AttachmentsinPACE = attachmentsCount > 0 ? "Yes" : "No";
                worksheet.Cells["C13"].Value = queue.ConfidentialPace = queue.Confidential;
                worksheet.Cells["C14"].Value = queue.Contractuallylimit = contractuallylimit?.Answer;
                worksheet.Cells["C15"].Value = queue.DisputeLitigation = disputeLitigation?.Answer;
                worksheet.Cells["C16"].Value = queue.Hostile = hostile?.Answer;
                worksheet.Cells["C17"].Value = queue.Auction = auction?.Answer;
                worksheet.Cells["C18"].Value = queue.AuditPartner = auditPartner?.Answer;
                DateTime localTime = DateTime.Now;
                TimeZoneInfo istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime istTime = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, istTimeZone);
                worksheet.Cells["C19"].Value = queue.TimeStamp = istTime.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cells["C20"].Value = queue.GovtEntity = govtEntity?.Answer;
                worksheet.Cells["C21"].Value = queue.ConflictCheckID;
                worksheet.Cells["C22"].Value = queue.onShore;
                for (int row = 22; row <= 24; row++)
                {
                    worksheet.Cells[$"C{row}"].Style.WrapText = true;
                    worksheet.AutoFitRowHeight(row, 'C', 'C');
                }
                worksheet.Cells["D5"].Value = queue.EngagementDesc;
                worksheet.Cells["D5"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["D5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells["D5"].Style.WrapText = true;
                worksheet.Columns[15].Width = 76.0d;
                //This will just show time taken by AU
                //if (span != "")
                //{
                //    worksheet.Cells["P1"].Value = "ElapsedTime: " + span;
                //}
                package.Save();
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
    }

    public static void SavePrescreeningTab(ConflictCheck conflictCheck, CheckerQueue summary, PreScreeningInfo preScreeningPrev, List<ApprovalRungQuestion> processQuestionnaire, string existingFilePath)
    {
        try
        {
            var additionalParties = summary.questionnaireAdditionalParties ?? new List<QuestionnaireAdditionalParties>();
            var triggers = TriggerForCheckMapper.CreateFrom(conflictCheck.Assessment?.conflictCheckTriggers);
            var ListTeamMembers = TeamMemberMapper.CreateFrom(
                             conflictCheck.Assessment?.TeamMembers,
                             conflictCheck.ConflictCheckTeamMembers);
            if (processQuestionnaire != null)
            {
                if (processQuestionnaire.Any())
                {
                    var triggerForCheckList = new List<TriggerForCheck>();
                    foreach (var item in processQuestionnaire)
                    {
                        TriggerForCheck triggerForCheck = new TriggerForCheck();
                        triggerForCheck.TriggerType = item.QuestionType;
                        if (item.QuestionType == "Question")
                        {
                            triggerForCheck.Details = "Q:" + item.Question + "\n" + "A:" + item.Answer;
                        }
                        else if (item.QuestionType == "Evaluation")
                        {
                            triggerForCheck.Details = "Evaluation:" + item.Section;
                        }
                        triggerForCheckList.Add(triggerForCheck);
                    }
                    if (triggerForCheckList.Any())
                    {
                        var questionTrigger = triggers.FirstOrDefault(i => i.TriggerType == "CONCH");
                        triggers?.Remove(questionTrigger);
                        triggers.AddRange(triggerForCheckList);
                    }
                }
            }
            var notesList = NoteMapper.CreateFrom(conflictCheck.Notes);
            using (var package = new ExcelPackage(new FileInfo(existingFilePath)))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault(x => x.Name == "PreScreening Info");
                if (worksheet == null)
                    return;
                int row = 4;
                var YellowColor = worksheet.Workbook.Styles.CreateNamedStyle("YellowColor");
                YellowColor.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                YellowColor.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                YellowColor.Style.WrapText = true;
                YellowColor.Style.Font.Name = "EYInterstate";
                YellowColor.Style.Font.Size = 8;
                YellowColor.Style.Fill.PatternType = ExcelFillStyle.Solid;
                YellowColor.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                YellowColor.Style.Border.Top.Style = YellowColor.Style.Border.Left.Style = YellowColor.Style.Border.Bottom.Style = YellowColor.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                SetCellBackgroundColor(worksheet.Cells[$"B{row}"], Color.Orange);
                worksheet.Cells[$"B{row}"].Value = "Triggers for Check";
                worksheet.Cells[$"B{row}"].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                ++row;

                var cellTrigger = worksheet.Cells[$"C{row}:D{row}"];
                SetRowCellBackgroundColor(cellTrigger, ColorTranslator.FromHtml("#BFBFBF"));
                worksheet.Cells[$"C{row}"].Value = "Trigger Type";
                worksheet.Cells[$"D{row}"].Value = "Details";
                cellTrigger.Style.Border.Top.Style = cellTrigger.Style.Border.Left.Style = cellTrigger.Style.Border.Bottom.Style = cellTrigger.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ++row;
                int index = 0;
                foreach (var trigger in triggers)
                {
                    var cellTriggerchild = worksheet.Cells[$"C{row}:D{row}"];
                    SetRowCellBackgroundColor(cellTriggerchild, ColorTranslator.FromHtml("#B7DEE8"));
                    worksheet.Cells[$"C{row}"].Value = trigger.TriggerType;
                    worksheet.Cells[$"D{row}"].Value = trigger.Details;
                    cellTriggerchild.Style.Border.Top.Style = cellTriggerchild.Style.Border.Left.Style = cellTriggerchild.Style.Border.Bottom.Style = cellTriggerchild.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    if (preScreeningPrev.ListTriggersForCheck != null && index < preScreeningPrev.ListTriggersForCheck.Count)
                    {
                        var triggePrev = preScreeningPrev.ListTriggersForCheck[index];
                        ++index;
                        if (!string.Equals(trigger.TriggerType, triggePrev.TriggerType, StringComparison.OrdinalIgnoreCase))
                        {
                            worksheet.Cells[$"C{row}"].StyleName = "YellowColor";
                        }
                        if (!string.Equals(trigger.Details, triggePrev.Details, StringComparison.OrdinalIgnoreCase))
                        {
                            worksheet.Cells[$"D{row}"].StyleName = "YellowColor";
                        }
                    }
                    else
                    {
                        worksheet.Cells[$"C{row}:D{row}"].StyleName = "YellowColor";
                    }
                    ++row;
                }
                ++row;
                SetCellBackgroundColor(worksheet.Cells[$"B{row}"], Color.Orange);
                worksheet.Cells[$"B{row}"].Value = "Notes";
                worksheet.Cells[$"B{row}"].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                ++row;
                var cellCreated = worksheet.Cells[$"C{row}:F{row}"];
                SetRowCellBackgroundColor(cellCreated, ColorTranslator.FromHtml("#BFBFBF"));
                worksheet.Cells[$"C{row}"].Value = "Created";
                worksheet.Cells[$"D{row}"].Value = "CreatedBy";
                worksheet.Cells[$"E{row}"].Value = "Category";
                worksheet.Cells[$"F{row}"].Value = "Comments";
                worksheet.Cells[$"C{row}:F{row}"].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                cellCreated.Style.Border.Top.Style = cellCreated.Style.Border.Left.Style = cellCreated.Style.Border.Bottom.Style = cellCreated.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ++row;
                if (!notesList.Any())
                {
                    var cellCreatedchild = worksheet.Cells[$"C{row}:F{row}"];
                    SetRowCellBackgroundColor(cellCreatedchild, ColorTranslator.FromHtml("#B7DEE8"));
                    worksheet.Cells[$"C{row}"].Value = "-";
                    worksheet.Cells[$"D{row}"].Value = "-";
                    worksheet.Cells[$"E{row}"].Value = "-";
                    worksheet.Cells[$"F{row}"].Value = "-";
                    cellCreatedchild.Style.Border.Top.Style = cellCreatedchild.Style.Border.Left.Style = cellCreatedchild.Style.Border.Bottom.Style = cellCreatedchild.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ++row;
                }
                index = 0;
                foreach (var note in notesList)
                {
                    var cellCreatedchild = worksheet.Cells[$"C{row}:F{row}"];
                    SetRowCellBackgroundColor(cellCreatedchild, ColorTranslator.FromHtml("#B7DEE8"));
                    worksheet.Cells[$"C{row}"].Value = note.Created;
                    worksheet.Cells[$"D{row}"].Value = note.CreatedBy;
                    worksheet.Cells[$"E{row}"].Value = note.Category;
                    worksheet.Cells[$"F{row}"].Value = note.Comments;
                    cellCreatedchild.Style.Border.Top.Style = cellCreatedchild.Style.Border.Left.Style = cellCreatedchild.Style.Border.Bottom.Style = cellCreatedchild.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    if (preScreeningPrev.ListNotes != null && index < preScreeningPrev.ListNotes.Count)
                    {
                        var notesPrev = preScreeningPrev.ListNotes[index];
                        ++index;

                        if (!string.Equals(note.Created, notesPrev.Created, StringComparison.OrdinalIgnoreCase))
                        {
                            worksheet.Cells[$"C{row}"].StyleName = "YellowColor";
                        }
                        if (notesPrev.CreatedBy != "-" && !string.Equals(note.CreatedBy, notesPrev.CreatedBy, StringComparison.OrdinalIgnoreCase))
                        {
                            worksheet.Cells[$"D{row}"].StyleName = "YellowColor";
                        }
                        if (notesPrev.Category != "-" && !string.Equals(note.Category, notesPrev.Category, StringComparison.OrdinalIgnoreCase))
                        {
                            worksheet.Cells[$"E{row}"].StyleName = "YellowColor";
                        }
                        if (notesPrev.Comments != "-" && !string.Equals(note.Comments, notesPrev.Comments, StringComparison.OrdinalIgnoreCase))
                        {
                            worksheet.Cells[$"F{row}"].StyleName = "YellowColor";
                        }
                    }
                    else
                    {
                        worksheet.Cells[$"C{row}:F{row}"].StyleName = "YellowColor";
                    }
                    ++row;
                }
                index = 0;
                ++row;
                SetCellBackgroundColor(worksheet.Cells[$"B{row}"], Color.Orange);
                worksheet.Cells[$"B{row}"].Value = "Team Members";
                worksheet.Cells[$"B{row}"].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                ++row;
                var cellRole = worksheet.Cells[$"C{row}:F{row}"];
                SetRowCellBackgroundColor(cellRole, ColorTranslator.FromHtml("#BFBFBF"));
                worksheet.Cells[$"C{row}"].Value = "Role";
                worksheet.Cells[$"D{row}"].Value = "Team Member";
                worksheet.Cells[$"E{row}"].Value = "Preparer";
                worksheet.Cells[$"F{row}"].Value = "Date Added";
                cellRole.Style.Border.Top.Style = cellRole.Style.Border.Left.Style = cellRole.Style.Border.Bottom.Style = cellRole.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ++row;
                if (!ListTeamMembers.Any())
                {
                    var cellRoleChild = worksheet.Cells[$"C{row}:F{row}"];
                    SetRowCellBackgroundColor(cellRoleChild, ColorTranslator.FromHtml("#B7DEE8"));
                    worksheet.Cells[$"C{row}"].Value = "-";
                    worksheet.Cells[$"D{row}"].Value = "-";
                    worksheet.Cells[$"E{row}"].Value = "-";
                    worksheet.Cells[$"F{row}"].Value = "-";
                    cellRoleChild.Style.Border.Top.Style = cellRoleChild.Style.Border.Left.Style = cellRoleChild.Style.Border.Bottom.Style = cellRoleChild.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ++row;
                }
                foreach (var team in ListTeamMembers)
                {
                    var cellRoleChild = worksheet.Cells[$"C{row}:F{row}"];
                    SetRowCellBackgroundColor(cellRoleChild, ColorTranslator.FromHtml("#B7DEE8"));
                    bool hasEmail = !team.Email.IsNullOrEmpty();
                    string teamMember = team.Name + (hasEmail ? $" ({team.Email})" : string.Empty);
                    worksheet.Cells[$"C{row}"].Value = team.Role;
                    worksheet.Cells[$"D{row}"].Value = teamMember;
                    worksheet.Cells[$"E{row}"].Value = team.Preparer;
                    worksheet.Cells[$"F{row}"].Value = string.IsNullOrEmpty(team.DateAdded) ? string.Empty : team.DateAdded;
                    cellRoleChild.Style.Border.Top.Style = cellRoleChild.Style.Border.Left.Style = cellRoleChild.Style.Border.Bottom.Style = cellRoleChild.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    if (preScreeningPrev.ListTeamMembers != null && index < preScreeningPrev.ListTeamMembers.Count)
                    {
                        var teamPrev = preScreeningPrev.ListTeamMembers[index];
                        ++index;
                        if (teamPrev.Role != "-" && !string.Equals(team.Role, teamPrev.Role, StringComparison.OrdinalIgnoreCase))
                        {
                            worksheet.Cells[$"C{row}"].StyleName = "YellowColor";
                        }
                        if (teamPrev.Name != "-" && !string.Equals(teamMember, teamPrev.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            worksheet.Cells[$"D{row}"].StyleName = "YellowColor";
                        }
                        if (teamPrev.Preparer != "-" && !string.Equals(team.Preparer, teamPrev.Preparer, StringComparison.OrdinalIgnoreCase))
                        {
                            worksheet.Cells[$"E{row}"].StyleName = "YellowColor";
                        }
                        if (teamPrev.DateAdded != "-" && !string.Equals(team.DateAdded, teamPrev.DateAdded, StringComparison.OrdinalIgnoreCase))
                        {
                            worksheet.Cells[$"E{row}"].StyleName = "YellowColor";
                        }
                    }
                    else
                    {
                        worksheet.Cells[$"C{row}:F{row}"].StyleName = "YellowColor";
                    }
                    ++row;
                }
                index = 0;
                ++row;
                SetCellBackgroundColor(worksheet.Cells[$"B{row}"], Color.Orange);
                worksheet.Cells[$"B{row}"].Value = "Questionnaire Additional Parties";
                worksheet.Cells[$"B{row}"].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                ++row;

                var cellAdditionalParties = worksheet.Cells[$"C{row}:E{row}"];
                SetRowCellBackgroundColor(cellAdditionalParties, ColorTranslator.FromHtml("#BFBFBF"));
                worksheet.Cells[$"C{row}"].Value = "Name";
                worksheet.Cells[$"D{row}"].Value = "Position";
                worksheet.Cells[$"E{row}"].Value = "Other Information";
                cellAdditionalParties.Style.Border.Top.Style = cellAdditionalParties.Style.Border.Left.Style = cellAdditionalParties.Style.Border.Bottom.Style = cellAdditionalParties.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ++row;
                if (!additionalParties.Any())
                {
                    var cellAPChild = worksheet.Cells[$"C{row}:E{row}"];
                    SetRowCellBackgroundColor(worksheet.Cells[$"C{row}:E{row}"], ColorTranslator.FromHtml("#B7DEE8"));
                    worksheet.Cells[$"C{row}"].Value = "-";
                    worksheet.Cells[$"D{row}"].Value = "-";
                    worksheet.Cells[$"E{row}"].Value = "-";
                    cellAPChild.Style.Border.Top.Style = cellAPChild.Style.Border.Left.Style = cellAPChild.Style.Border.Bottom.Style = cellAPChild.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ++row;
                }
                foreach (var parties in additionalParties)
                {
                    var cellAPChild = worksheet.Cells[$"C{row}:E{row}"];
                    SetRowCellBackgroundColor(worksheet.Cells[$"C{row}:E{row}"], ColorTranslator.FromHtml("#B7DEE8"));
                    worksheet.Cells[$"C{row}"].Value = parties.Name = string.IsNullOrEmpty(parties.Name) ? "-" : parties.Name;
                    worksheet.Cells[$"D{row}"].Value = parties.Position = string.IsNullOrEmpty(parties.Position) ? "-" : parties.Position;
                    worksheet.Cells[$"E{row}"].Value = parties.OtherInformation = string.IsNullOrEmpty(parties.OtherInformation) ? "-" : parties.OtherInformation;
                    cellAPChild.Style.Border.Top.Style = cellAPChild.Style.Border.Left.Style = cellAPChild.Style.Border.Bottom.Style = cellAPChild.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    if (preScreeningPrev.ListAdditionalParties != null && index < preScreeningPrev.ListAdditionalParties.Count)
                    {
                        var addiPrev = preScreeningPrev.ListAdditionalParties[index];
                        addiPrev.OtherInformation = string.IsNullOrEmpty(addiPrev.OtherInformation) ? "-" : addiPrev.OtherInformation;
                        ++index;
                        if (!string.Equals(parties.Name, addiPrev.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            worksheet.Cells[$"C{row}"].StyleName = "YellowColor";
                        }
                        if (!string.Equals(parties.Position, addiPrev.Position, StringComparison.OrdinalIgnoreCase))
                        {
                            worksheet.Cells[$"D{row}"].StyleName = "YellowColor";
                        }
                        if (!string.Equals(parties.OtherInformation, addiPrev.OtherInformation, StringComparison.OrdinalIgnoreCase))
                        {
                            worksheet.Cells[$"E{row}"].StyleName = "YellowColor";
                        }
                    }
                    else
                    {
                        worksheet.Cells[$"C{row}:E{row}"].StyleName = "YellowColor";
                    }
                    ++row;
                }
                index = 0;
                ++row;
                var hostile = summary.questionnaireSummary.Where(i => i.Title == "Hostile" && (i.QuestionNumber == "041349" || i.QuestionNumber == "597835") && (i.Answer == "Yes")).ToList().Count > 0
                   ? summary.questionnaireSummary.FirstOrDefault(i => i.Title == "Hostile" && (i.QuestionNumber == "041349" || i.QuestionNumber == "597835") && (i.Answer == "Yes"))
                   : summary.questionnaireSummary.FirstOrDefault(i => i.Title == "Hostile" && (i.QuestionNumber == "041349" || i.QuestionNumber == "597835") && (i.Answer == "No"));

                if (hostile == null)
                {
                    hostile = new QuestionnaireSummary();
                    hostile.Answer = hostile.Explanation = "-";
                }
                if (hostile != null)
                {
                    hostile.Answer = string.IsNullOrEmpty(hostile.Answer) || hostile.Answer == "N/A" ? "-" : hostile.Answer;
                    hostile.Explanation = string.IsNullOrEmpty(hostile.Explanation) || hostile.Explanation == "N/A" ? "-" : hostile.Explanation;

                    if (preScreeningPrev.HostileQuestion != null)
                    {
                        preScreeningPrev.HostileQuestion.Question_1_4_Answer = preScreeningPrev.HostileQuestion.Question_1_4_Answer == "N/A" ? "-" : preScreeningPrev.HostileQuestion.Question_1_4_Answer;
                        preScreeningPrev.HostileQuestion.Question_1_4_Comments = preScreeningPrev.HostileQuestion?.Question_1_4_Comments == "N/A" ? "-" : preScreeningPrev.HostileQuestion?.Question_1_4_Comments;
                    }
                    else
                    {
                        preScreeningPrev.HostileQuestion = new Models.PreScreening.SubClasses.HostileQuestion();
                        preScreeningPrev.HostileQuestion.Question_1_4_Answer = "-";
                        preScreeningPrev.HostileQuestion.Question_1_4_Comments = "-";
                    }
                    SetCellBackgroundColor(worksheet.Cells[$"B{row}"], Color.Orange);
                    worksheet.Cells[$"B{row}"].Value = "Hostile Question";
                    worksheet.Cells[$"B{row}"].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ++row;
                    worksheet.Cells[$"C{row}"].Value = "Q. No. 1.4 Answer";
                    SetCellBackgroundColor(worksheet.Cells[$"C{row}"], ColorTranslator.FromHtml("#BFBFBF"));
                    worksheet.Cells[$"D{row}"].Value = hostile.Answer;
                    SetCellBackgroundColor(worksheet.Cells[$"D{row}"], ColorTranslator.FromHtml("#B7DEE8"));
                    if (!(hostile.Answer == "-" && preScreeningPrev.HostileQuestion?.Question_1_4_Answer == "-") && !string.Equals(hostile.Answer, preScreeningPrev.HostileQuestion?.Question_1_4_Answer, StringComparison.OrdinalIgnoreCase))
                    {
                        worksheet.Cells[$"D{row}"].StyleName = "YellowColor";
                    }
                    var cellhostileAns = worksheet.Cells[$"C{row}:D{row}"];
                    cellhostileAns.Style.Border.Top.Style = cellhostileAns.Style.Border.Left.Style = cellhostileAns.Style.Border.Bottom.Style = cellhostileAns.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ++row;
                    worksheet.Cells[$"C{row}"].Value = "Q. No. 1.4 Comments";
                    SetCellBackgroundColor(worksheet.Cells[$"C{row}"], ColorTranslator.FromHtml("#BFBFBF"));
                    worksheet.Cells[$"D{row}"].Value = hostile.Explanation;
                    SetCellBackgroundColor(worksheet.Cells[$"D{row}"], ColorTranslator.FromHtml("#B7DEE8"));
                    if (!(hostile.Explanation == "-" && preScreeningPrev.HostileQuestion?.Question_1_4_Comments == "-") && !string.Equals(hostile.Explanation, preScreeningPrev.HostileQuestion?.Question_1_4_Comments, StringComparison.OrdinalIgnoreCase))
                    {
                        worksheet.Cells[$"D{row}"].StyleName = "YellowColor";
                    }
                    var cellhostileCom = worksheet.Cells[$"C{row}:D{row}"];
                    cellhostileCom.Style.Border.Top.Style = cellhostileCom.Style.Border.Left.Style = cellhostileCom.Style.Border.Bottom.Style = cellhostileCom.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ++row;
                }
                ++row;
                var contractuallylimit = summary.questionnaireSummary.FirstOrDefault(i => i.Title == "Contractually limit our ability");
                if (contractuallylimit != null)
                {
                    contractuallylimit.Answer = string.IsNullOrEmpty(contractuallylimit.Answer) || contractuallylimit.Answer == "N/A" ? "-" : contractuallylimit.Answer;
                    if (preScreeningPrev.LimitationsToAct != null)
                    {
                        preScreeningPrev.LimitationsToAct.YesNo = string.IsNullOrEmpty(preScreeningPrev.LimitationsToAct?.YesNo) || preScreeningPrev.LimitationsToAct?.YesNo == "N/A" ? "-" : preScreeningPrev.LimitationsToAct?.YesNo;
                    }
                    else
                    {
                        preScreeningPrev.LimitationsToAct = new LimitationsToAct();
                        preScreeningPrev.LimitationsToAct.YesNo = "-";
                    }
                    SetCellBackgroundColor(worksheet.Cells[$"B{row}"], Color.Orange);
                    worksheet.Cells[$"B{row}"].Value = "Are there limitations to act for specific entities or within a market requested by the client?";
                    worksheet.Cells[$"B{row}"].Style.WrapText = true;
                    worksheet.Cells[$"B{row}"].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ++row;
                    worksheet.Cells[$"C{row}"].Value = "Yes/No?";
                    SetCellBackgroundColor(worksheet.Cells[$"C{row}"], ColorTranslator.FromHtml("#BFBFBF"));
                    worksheet.Cells[$"D{row}"].Value = contractuallylimit.Answer;
                    SetCellBackgroundColor(worksheet.Cells[$"D{row}"], ColorTranslator.FromHtml("#B7DEE8"));
                    if (!(contractuallylimit.Answer == "-" && preScreeningPrev.LimitationsToAct?.YesNo == "-") && !string.Equals(contractuallylimit.Answer, preScreeningPrev.LimitationsToAct?.YesNo, StringComparison.OrdinalIgnoreCase))
                    {
                        worksheet.Cells[$"D{row}"].StyleName = "YellowColor";
                    }
                    var cellContractually = worksheet.Cells[$"C{row}:D{row}"];
                    cellContractually.Style.Border.Top.Style = cellContractually.Style.Border.Left.Style = cellContractually.Style.Border.Bottom.Style = cellContractually.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ++row;
                }
                ++row;
                var pursuitCheckPerformed = summary.questionnaireSummary.FirstOrDefault(i => i.Title == "Pursuit Check Performed");
                if (pursuitCheckPerformed != null)
                {
                    string anotherCheckComments = "-";
                    if (preScreeningPrev.AnotherConflictCheck != null)
                    {
                        if (!string.IsNullOrEmpty(preScreeningPrev.AnotherConflictCheck.Comments))
                            preScreeningPrev.AnotherConflictCheck?.Comments.Replace("N/A", "-");
                        else
                            preScreeningPrev.AnotherConflictCheck.Comments = anotherCheckComments;
                    }
                    SetCellBackgroundColor(worksheet.Cells[$"B{row}"], Color.Orange);
                    worksheet.Cells[$"B{row}"].Value = "Has another conflict check been performed in connection with this engagement?";
                    worksheet.Cells[$"B{row}"].EntireRow.Height = 25;
                    worksheet.Cells[$"B{row}"].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ++row;
                    worksheet.Cells[$"C{row}"].Value = "Comments";
                    SetCellBackgroundColor(worksheet.Cells[$"C{row}"], ColorTranslator.FromHtml("#BFBFBF"));
                    worksheet.Cells[$"D{row}"].Value = pursuitCheckPerformed.Explanation = string.IsNullOrEmpty(pursuitCheckPerformed.Explanation) ? "-" : pursuitCheckPerformed.Explanation;
                    SetCellBackgroundColor(worksheet.Cells[$"D{row}"], ColorTranslator.FromHtml("#B7DEE8"));
                    if (!(pursuitCheckPerformed.Explanation == "-" && preScreeningPrev.AnotherConflictCheck?.Comments == "-") && !string.Equals(pursuitCheckPerformed.Explanation, preScreeningPrev.AnotherConflictCheck?.Comments, StringComparison.OrdinalIgnoreCase))
                    {
                        worksheet.Cells[$"D{row}"].StyleName = "YellowColor";
                    }
                    var cellPursuitCheck = worksheet.Cells[$"C{row}:D{row}"];
                    cellPursuitCheck.Style.Border.Top.Style = cellPursuitCheck.Style.Border.Left.Style = cellPursuitCheck.Style.Border.Bottom.Style = cellPursuitCheck.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ++row;
                }
                ++row;
                var disputeLitigation = summary.questionnaireSummary.FirstOrDefault(i => i.Title == "Dispute/Litigation");
                if (disputeLitigation != null)
                {
                    disputeLitigation.Explanation = string.IsNullOrEmpty(disputeLitigation.Explanation) || disputeLitigation.Explanation == "N/A" ? "-" : disputeLitigation.Explanation;
                    SetCellBackgroundColor(worksheet.Cells[$"B{row}"], Color.Orange);
                    worksheet.Cells[$"B{row}"].Value = "Dispute/Litigation Involvement";
                    worksheet.Cells[$"B{row}"].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ++row;
                    worksheet.Cells[$"C{row}"].Value = "Comments";
                    SetCellBackgroundColor(worksheet.Cells[$"C{row}"], ColorTranslator.FromHtml("#BFBFBF"));
                    worksheet.Cells[$"D{row}"].Value = disputeLitigation.Explanation;
                    SetCellBackgroundColor(worksheet.Cells[$"D{row}"], ColorTranslator.FromHtml("#B7DEE8"));
                    if (!(disputeLitigation.Explanation == "-" && preScreeningPrev.DisputeLitigationInvolvement?.Comments == "-") && !string.Equals(disputeLitigation.Explanation, preScreeningPrev.DisputeLitigationInvolvement?.Comments, StringComparison.OrdinalIgnoreCase))
                    {
                        worksheet.Cells[$"D{row}"].StyleName = "YellowColor";
                    }
                    var cellDispute = worksheet.Cells[$"C{row}:D{row}"];
                    cellDispute.Style.Border.Top.Style = cellDispute.Style.Border.Left.Style = cellDispute.Style.Border.Bottom.Style = cellDispute.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ++row;
                }
                ++row;
                var highProfile = summary.questionnaireSummary.FirstOrDefault(i => i.Title == "High Profile");
                if (highProfile != null)
                {
                    highProfile.Answer = string.IsNullOrEmpty(highProfile.Answer) || highProfile.Answer == "N/A" ? "-" : highProfile.Answer;
                    highProfile.Explanation = string.IsNullOrEmpty(highProfile.Explanation) || highProfile.Explanation == "N/A" ? "-" : highProfile.Explanation;
                    SetCellBackgroundColor(worksheet.Cells[$"B{row}"], Color.Orange);
                    worksheet.Cells[$"B{row}"].Value = "Negative press coverage";
                    worksheet.Cells[$"B{row}"].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ++row;
                    worksheet.Cells[$"C{row}"].Value = "Yes/No?";
                    SetCellBackgroundColor(worksheet.Cells[$"C{row}"], ColorTranslator.FromHtml("#BFBFBF"));
                    worksheet.Cells[$"D{row}"].Value = highProfile.Answer;
                    SetCellBackgroundColor(worksheet.Cells[$"D{row}"], ColorTranslator.FromHtml("#B7DEE8"));
                    if (!(highProfile.Answer == "-" && preScreeningPrev.HighProfileEngagement?.YesNo == "-") && !string.Equals(highProfile.Answer, preScreeningPrev.HighProfileEngagement?.YesNo, StringComparison.OrdinalIgnoreCase))
                    {
                        worksheet.Cells[$"D{row}"].StyleName = "YellowColor";
                    }
                    var cellHighprofile = worksheet.Cells[$"C{row}:D{row}"];
                    cellHighprofile.Style.Border.Top.Style = cellHighprofile.Style.Border.Left.Style = cellHighprofile.Style.Border.Bottom.Style = cellHighprofile.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ++row;
                    worksheet.Cells[$"C{row}"].Value = "Comments";
                    SetCellBackgroundColor(worksheet.Cells[$"C{row}"], ColorTranslator.FromHtml("#BFBFBF"));
                    worksheet.Cells[$"D{row}"].Value = highProfile.Explanation;
                    SetCellBackgroundColor(worksheet.Cells[$"D{row}"], ColorTranslator.FromHtml("#B7DEE8"));
                    if (!(highProfile.Explanation == "-" && preScreeningPrev.HighProfileEngagement?.Comments == "-") && !string.Equals(highProfile.Explanation, preScreeningPrev.HighProfileEngagement?.Comments, StringComparison.OrdinalIgnoreCase))
                    {
                        worksheet.Cells[$"D{row}"].StyleName = "YellowColor";
                    }
                    var cellHighprofilecomment = worksheet.Cells[$"C{row}:D{row}"];
                    cellHighprofilecomment.Style.Border.Top.Style = cellHighprofilecomment.Style.Border.Left.Style = cellHighprofilecomment.Style.Border.Bottom.Style = cellHighprofilecomment.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ++row;
                }
                ++row;
                var auditPartner = summary.questionnaireSummary.FirstOrDefault(i => i.Title == "Whether to contact counterparty (G)CSP/audit partner?");
                if (auditPartner != null)
                {
                    auditPartner.Explanation = string.IsNullOrEmpty(auditPartner.Explanation) || auditPartner.Explanation == "N/A" ? "-" : auditPartner.Explanation;
                    SetCellBackgroundColor(worksheet.Cells[$"B{row}"], Color.Orange);
                    worksheet.Cells[$"B{row}"].Value = "Are there any reasons why the counterparty GCSP/LAP should not be contacted?";
                    worksheet.Cells[$"B{row}"].EntireRow.Height = 25;
                    worksheet.Cells[$"B{row}"].Style.WrapText = true;
                    worksheet.Cells[$"B{row}"].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    ++row;
                    worksheet.Cells[$"C{row}"].Value = "Comments";
                    SetCellBackgroundColor(worksheet.Cells[$"C{row}"], ColorTranslator.FromHtml("#BFBFBF"));
                    worksheet.Cells[$"D{row}"].Value = auditPartner.Explanation;
                    SetCellBackgroundColor(worksheet.Cells[$"D{row}"], ColorTranslator.FromHtml("#B7DEE8"));
                    if (preScreeningPrev.ConsentToContactCounterparty != null)
                    {
                        if (!(auditPartner.Explanation == "-" && preScreeningPrev.ConsentToContactCounterparty?.Comments == "-") && !string.Equals(auditPartner.Explanation, preScreeningPrev.ConsentToContactCounterparty?.Comments, StringComparison.OrdinalIgnoreCase))
                        {
                            worksheet.Cells[$"D{row}"].StyleName = "YellowColor";
                        }
                    }
                    var cellAuditPartner = worksheet.Cells[$"C{row}:D{row}"];
                    cellAuditPartner.Style.Border.Top.Style = cellAuditPartner.Style.Border.Left.Style = cellAuditPartner.Style.Border.Bottom.Style = cellAuditPartner.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ++row;
                }
                ++row;
                worksheet.Column(3).Style.WrapText = true;
                worksheet.Column(4).Style.WrapText = true;
                worksheet.Column(5).Style.WrapText = true;
                worksheet.Column(6).Style.WrapText = true;
                package.Save();
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
    }
    public static void SaveNuancesTab(string region, string country, RegionalEntity regionalEntity, string existingFilePath)
    {
        try
        {

            using (var package = new ExcelPackage(new FileInfo(existingFilePath)))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault(x => x.Name == "Nuances");
                if (worksheet == null)
                    return;
                worksheet.Cells["C4"].Value = $"{region}/{country}";
                int iPresentRow = 6;
                var regionDeviationList = regionalEntity.regionalDeviations.Where(item => item.Region.Contains(country, StringComparison.OrdinalIgnoreCase)).ToList();
                var supplementaryGuidesList = regionalEntity.supplementaryGuides.Where(item => item.Region.Contains(country, StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var item in regionDeviationList)
                {
                    iPresentRow++;
                    var properties = new[]
                                     {
                                        new { Column = "B", Value = item.Area },
                                        new { Column = "C", Value = item.Region },
                                        new { Column = "D", Value = item.WorkAllocationByGDS },
                                        new { Column = "E", Value = item.AdditionalLocalDB },
                                        new { Column = "F", Value = item.GCSPConsultaion },
                                        new { Column = "G", Value = item.SignoffGDS },
                                        new { Column = "H", Value = item.OtherNuances }
                                    };

                    foreach (var prop in properties)
                    {
                        var cell = worksheet.Cells[$"{prop.Column}{iPresentRow}"];
                        var value = prop.Value ?? string.Empty;
                        // To solve Issue #1022660
                        value = (value.Equals("ü") || value.Equals("✓")) ? "Yes" : value;
                        value = value.Equals("û") ? "No" : value;
                        // value = value.Equals("???") ? "Conditional" : value;
                        //---------------
                        cell.Value = value;
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        cell.Style.Font.Size = 8;
                        cell.Style.Font.Bold = true;
                    }
                }
                iPresentRow += 2;
                if (supplementaryGuidesList.Any())
                {
                    var headerRow = worksheet.Cells[$"B{iPresentRow}:K{iPresentRow}"];
                    headerRow.Style.Font.Size = 10;
                    SetCellProperties("B", "Service Line");
                    SetCellProperties("C", "Work-allocation");
                    SetCellProperties("D", "Pre-screening");
                    SetCellProperties("E", "Info request to Engagement Teams (ETs)");
                    SetCellProperties("F", "Research");
                    SetCellProperties("G", "Counterparty (G)CSP/Audit Partner liaison");
                    SetCellProperties("H", "Conclusion");
                    SetCellProperties("I", "Sign-off");
                    SetCellProperties("J", "Supporting Document(mercuryEntity) Please refer to documents uploaded on Sharepoint");
                    SetCellProperties("K", "Contact Names");
                    void SetCellProperties(string column, string value)
                    {
                        var cell = worksheet.Cells[$"{column}{iPresentRow}"];
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        cell.Style.WrapText = true;
                        cell.Value = value;
                        SetCellBackgroundColor(cell, Color.FromArgb(196, 215, 155));
                    }
                }
                foreach (var item in supplementaryGuidesList)
                {
                    iPresentRow++;
                    SetCellProperties("B", item.ServiceLine);
                    SetCellProperties("C", item.WorkAllocation);
                    SetCellProperties("D", item.PreScreening);
                    SetCellProperties("E", item.InfoRequestET);
                    SetCellProperties("F", item.Research);
                    SetCellProperties("G", item.Counterparty);
                    SetCellProperties("H", item.Conclusion);
                    SetCellProperties("I", item.Signoff);
                    SetCellProperties("J", item.SupportingDocuments);
                    SetCellProperties("K", item.ContactNames);
                    void SetCellProperties(string column, string value)
                    {
                        var cell = worksheet.Cells[$"{column}{iPresentRow}"];
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        cell.Style.Font.Size = 8;
                        cell.Style.WrapText = true;
                        cell.Value = value;
                        SetCellBackgroundColor(cell, Color.FromArgb(183, 222, 232));
                    }
                }
                package.Save();
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
    }
    public static void SaveBotUnitTab(CheckerQueue queue, List<ResearchSummary> AdditionalResearch,
    string existingFilePath, bool isAdditionalGUP, out int AUUnitRowindex, bool generateLegends = false, bool Rework = false)
    {
        // _debugSaveBotUnitTab_CallNumber++;
        // Console.WriteLine($"{new string('-', 30)}\nSaveBotUnitTab(), call #{_debugSaveBotUnitTab_CallNumber}");
        // DebugShowResearchSummaryEntries("input", queue.researchSummary);

        AUUnitRowindex = 0;
        Color gisIColor = Color.FromArgb(196, 189, 151);
        Color defaultColor = Color.FromArgb(235, 241, 222);
        try
        {
            using (var package = new ExcelPackage(new FileInfo(existingFilePath)))
            {

                var worksheet = package.Workbook.Worksheets.Where(x => x.Name == "AU Unit Grid").FirstOrDefault();
                if (worksheet == null)
                    return;
                int startRow = 5;  // Adjust the starting row as needed
                int endRow = startRow + queue.researchSummary.Count;  // Calculate the ending row based on the number of items

                // Define the entire range
                var cellRange = worksheet.Cells[startRow, 2, endRow, 17];

                // Set all borders for the entire range
                var border = cellRange.Style.Border;
                border.Top.Style = border.Left.Style = border.Bottom.Style = border.Right.Style = ExcelBorderStyle.Thin;
                int i = 5;
                int row = 0;

                if (AdditionalResearch.Count > 0 && (!isAdditionalGUP))
                {
                    queue.researchSummary.AddRange(AdditionalResearch);

                    queue.researchSummary = queue.researchSummary.CAUSort();
                    foreach (var item in queue.researchSummary)
                    {
                        ++i;
                        ++row;
                        item.BotUnitRowNo = AUUnitRowindex = i;

                        if (!Rework || (Rework && item.Rework)) //Do not modify rows
                        {
                            string orgRoleNameB = item.Role.Replace("GIS", "").Replace("CER", "").Replace("GUP of GUP of ", "GUP of ").Replace("GUP of  GUP of ", "GUP of ");
                            List<string> uniqueRoleValuesB = orgRoleNameB.Trim().Split(',').Distinct().ToList();
                            string UniqueRoleNameB = string.Empty;

                            if (uniqueRoleValuesB.Count > 1)
                                UniqueRoleNameB = string.Join(", ", uniqueRoleValuesB);
                            else
                                UniqueRoleNameB = uniqueRoleValuesB[0];

                            WriteHyperlink(worksheet, $"B{i}", item.WorksheetNo, $"'{item.WorksheetNo}'!A1");
                            worksheet.Cells[$"B{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            worksheet.Cells[$"C{i}"].Value = item.EntityName;
                            worksheet.Cells[$"D{i}"].Value = item.GISID;
                            worksheet.Cells[$"E{i}"].Value = item.MDMID;
                            worksheet.Cells[$"F{i}"].Value = item.DUNSNumber;
                            worksheet.Cells[$"G{i}"].Value = item.Country;
                            worksheet.Cells[$"H{i}"].Value = item.SourceSystem;
                            worksheet.Cells[$"I{i}"].Value = UniqueRoleNameB;
                            worksheet.Cells[$"J{i}"].Value = item.IsClientSide ? "Client Side" : "Non-Client Side";
                            worksheet.Cells[$"K{i}"].Value = item.AdditionalComments;
                            worksheet.Cells[$"L{i}"].Value = item.GIS;
                            worksheet.Cells[$"M{i}"].Value = item.Mercury;
                            worksheet.Cells[$"N{i}"].Value = item.CRR;
                            worksheet.Cells[$"O{i}"].Value = item.SPL;
                            worksheet.Cells[$"P{i}"].Value = item.Finscan;
                            item.Type = string.IsNullOrEmpty(item.Type) ? GetSubjectType(item) : item.Type;
                            worksheet.Cells[$"Q{i}"].Value = item.Type;
                            worksheet.Cells[$"R{i}"].Value = (item.Rework ? "Yes" : "No");
                            char[] columnsToApplyBorder = Enumerable.Range('B', 'R' - 'B' + 1).Select(c => (char)c).ToArray();
                            foreach (char column in columnsToApplyBorder)
                            {
                                string cellReference = $"{column}{i}";
                                worksheet.Cells[cellReference].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            }
                            Color cellColor = (item.GIS == "I") ? gisIColor : defaultColor;
                            var range = item.GIS == "I" ? $"B{i}:R{i}" : $"L{i}:P{i}";
                            SetCellBackgroundColor(worksheet.Cells[range], cellColor);
                        }
                    }
                }
                else
                {
                    queue.researchSummary = queue.researchSummary.CAUSort();
                    foreach (var item in queue.researchSummary)
                    {
                        ++i;

                        item.BotUnitRowNo = AUUnitRowindex = i;
                        if (!(item.Role.StartsWith("GUP") || item.Role.Contains("CER GUP") || item.Role.Contains("GIS GUP")))
                        {
                            ++row;
                        }
                        string formattedIndex = string.Empty;
                        formattedIndex = row.ToString("D2");

                        if (item.Role.StartsWith("GUP"))
                            formattedIndex += ".P01";

                        if (item.Role.Contains("CER GUP") || item.Role.Contains("GIS GUP"))
                            formattedIndex = item.WorksheetNo;
                        if (!Rework || (Rework && item.Rework)) //Do not modify rows
                        {
                            string orgRoleName = item.Role.Replace("GIS", "").Replace("CER", "").Replace("GUP of GUP of ", "GUP of ").Replace("GUP of  GUP of ", "GUP of ");
                            List<string> uniqueRoleValues = orgRoleName.Trim().Split(',').Distinct().ToList();
                            string UniqueRoleName = string.Empty;

                            if (uniqueRoleValues.Count > 1)
                                UniqueRoleName = string.Join(", ", uniqueRoleValues);
                            else
                                UniqueRoleName = uniqueRoleValues[0];

                            WriteHyperlink(worksheet, $"B{i}", formattedIndex, $"'{formattedIndex}'!A1");
                            worksheet.Cells[$"B{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            worksheet.Cells[$"C{i}"].Value = item.EntityName;
                            worksheet.Cells[$"D{i}"].Value = item.GISID;
                            worksheet.Cells[$"E{i}"].Value = item.MDMID;
                            worksheet.Cells[$"F{i}"].Value = item.DUNSNumber;
                            worksheet.Cells[$"G{i}"].Value = item.Country;
                            worksheet.Cells[$"H{i}"].Value = item.SourceSystem;
                            worksheet.Cells[$"I{i}"].Value = UniqueRoleName;
                            worksheet.Cells[$"J{i}"].Value = item.IsClientSide ? "Client Side" : "Non-Client Side";
                            worksheet.Cells[$"K{i}"].Value = item.AdditionalComments;
                            worksheet.Cells[$"L{i}"].Value = item.GIS;
                            worksheet.Cells[$"M{i}"].Value = item.Mercury;
                            worksheet.Cells[$"N{i}"].Value = item.CRR;
                            worksheet.Cells[$"O{i}"].Value = item.SPL;
                            worksheet.Cells[$"P{i}"].Value = item.Finscan;
                            item.Type = string.IsNullOrEmpty(item.Type) ? GetSubjectType(item) : item.Type;
                            worksheet.Cells[$"Q{i}"].Value = item.Type;
                            worksheet.Cells[$"R{i}"].Value = (item.Rework ? "Yes" : "No");
                            char[] columnsToApplyBorder = Enumerable.Range('B', 'R' - 'B' + 1).Select(c => (char)c).ToArray();
                            foreach (char column in columnsToApplyBorder)
                            {
                                string cellReference = $"{column}{i}";
                                worksheet.Cells[cellReference].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            }
                            Color cellColor = (item.GIS == "I") ? gisIColor : defaultColor;
                            var range = item.GIS == "I" ? $"B{i}:R{i}" : $"L{i}:P{i}";
                            SetCellBackgroundColor(worksheet.Cells[range], cellColor);

                            // item.WorksheetNo = formattedIndex;  // Carlos: Replaced this line with the following statement, due to Bug #1022053
                            item.WorksheetNo ??= formattedIndex;

                            item.TurnFlagsOffWhenNoResearch();
                        }
                    }
                }

                if (generateLegends)
                {
                    AddLegendsAUGrid(worksheet, AUUnitRowindex);
                }
                package.Save();
            }

            // DebugShowResearchSummaryEntries("output", queue.researchSummary);
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
    }
    /// <summary>
    /// For Clearing Bot Unit Grid. Once Duplication is removed from ResearchSummary, data should be removed from Unit Grid
    /// </summary>
    /// <param name="queue"></param>
    /// <param name="AdditionalResearch"></param>
    /// <param name="existingFilePath"></param>
    public static void ClearBotUnitTab(CheckerQueue queue, List<ResearchSummary> AdditionalResearch, string existingFilePath)
    {
        try
        {
            using (var package = new ExcelPackage(new FileInfo(existingFilePath)))
            {
                var worksheet = package.Workbook.Worksheets.Where(x => x.Name == "AU Unit Grid").FirstOrDefault();
                if (worksheet == null)
                    return;
                int startRow = 5;  // Adjust the starting row as needed
                int endRow = startRow + queue.researchSummary.Count;  // Calculate the ending row based on the number of items
                // Define the entire range
                var cellRange = worksheet.Cells[startRow, 2, endRow, 17];
                // Set all borders for the entire range
                var border = cellRange.Style.Border;
                border.Top.Style = border.Left.Style = border.Bottom.Style = border.Right.Style = ExcelBorderStyle.Thin;
                Color defaultColor = Color.White;
                int rowIndex = queue.researchSummary.Count + AdditionalResearch.Count + 10;
                for (int i = 6; i < rowIndex; i++)
                {
                    for (char column = 'B'; column <= 'R'; column++)
                    {
                        worksheet.Cells[$"{column}{i}"].Value = "";
                    }
                    char[] columnsToApplyBorder = Enumerable.Range('B', 'R' - 'B' + 1).Select(c => (char)c).ToArray();
                    foreach (char column in columnsToApplyBorder)
                    {
                        string cellReference = $"{column}{i}";
                        worksheet.Cells[cellReference].Style.Border.BorderAround(ExcelBorderStyle.None);
                    }
                    var range = $"B{i}:R{i}";
                    SetCellBackgroundColor(worksheet.Cells[range], defaultColor);
                }
                package.Save();
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
    }
    public static void SaveDataTableToExcel(DataSet dataSet, string filePath)
    {
        try
        {
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                if (dataSet.Tables.Count > 0 && dataSet.Tables[0] != null)
                {
                    var worksheet1 = excelPackage.Workbook.Worksheets.Add("Sheet1");
                    worksheet1.Cells["A1"].LoadFromDataTable(dataSet.Tables[0], true);
                }

                if (dataSet.Tables.Count > 1 && dataSet.Tables[1] != null)
                {
                    var worksheet2 = excelPackage.Workbook.Worksheets.Add("Sheet2");
                    worksheet2.Cells["A1"].LoadFromDataTable(dataSet.Tables[1], true);
                }

                if (dataSet.Tables.Count > 2 && dataSet.Tables[2] != null)
                {
                    var worksheet3 = excelPackage.Workbook.Worksheets.Add("Sheet3");
                    worksheet3.Cells["A1"].LoadFromDataTable(dataSet.Tables[2], true);
                }
                if (dataSet.Tables.Count > 3 && dataSet.Tables[3] != null)
                {
                    var worksheet4 = excelPackage.Workbook.Worksheets.Add("Sheet4");
                    worksheet4.Cells["A1"].LoadFromDataTable(dataSet.Tables[3], true);
                }
                FileInfo file = new FileInfo(filePath);
                excelPackage.SaveAs(file);
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
    }
#pragma warning disable IDE0051 // Remove unused private members
    private static void DebugShowResearchSummaryEntries(string label, List<ResearchSummary> listResearchSummary)
    {
        string input = string.Join("\n", listResearchSummary.Select(rs =>
            $"{listResearchSummary.IndexOf(rs) + 1:00}. tab '"
            + (rs.WorksheetNo ?? "NULL") + "' - EntityName:" + (rs.EntityName ?? "NULL")
            + "' - Role:" + (rs.Role ?? "NULL")));
        Console.WriteLine($"{label}, {listResearchSummary.Count} element(s): \n{input}\n");
    }
#pragma warning restore IDE0051 // Remove unused private members

    private static string ConcatWithComma(string original, string toAppend)
    {
        if (string.IsNullOrWhiteSpace(original))
        {
            return toAppend;
        }
        if (string.IsNullOrWhiteSpace(toAppend))
        {
            return original;
        }
        return $"{original}, {toAppend}";
    }
    public static void AddLegendsAUGrid(ExcelWorksheet worksheet, int i)
    {
        if (worksheet == null)
        {
            return;
        }

        i += 4;
        try
        {
            worksheet.Cells[$"B{i}"].Value = "Naming Convention Legend:";
            worksheet.Cells[$"B{i}"].Style.Font.Bold = true;
            worksheet.Cells[$"B{i}"].Style.Font.Color.SetColor(Color.Black);
            worksheet.Cells[$"L{i}:P{i}"].Merge = true;
            worksheet.Cells[$"L{i}"].Value = "Database Legend:";
            worksheet.Cells[$"L{i}"].Style.Font.Color.SetColor(Color.White);
            SetCellBackgroundColor(worksheet.Cells[$"L{i}"], Color.FromArgb(0, 32, 96));
            worksheet.Cells[$"L{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[$"L{i}"].Style.Font.Bold = true;
            i++;

            worksheet.Cells[$"B{i}"].Value = "G = GIS";
            worksheet.Cells[$"B{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"B{i}"].Style.Font.Color.SetColor(Color.Black);
            worksheet.Cells[$"L{i}"].Value = "X = Not Researched - Use this legend also for giving any research input to the AU";
            worksheet.Cells[$"L{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"L{i}"].Style.WrapText = false;
            i++;

            worksheet.Cells[$"B{i}"].Value = "C = CER";
            worksheet.Cells[$"B{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"B{i}"].Style.Font.Color.SetColor(Color.Black);
            worksheet.Cells[$"L{i}"].Value = "NA = Not Applicable for research";
            worksheet.Cells[$"L{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"L{i}"].Style.WrapText = false;
            i++;

            worksheet.Cells[$"B{i}"].Value = "P = PACE APG";
            worksheet.Cells[$"B{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"B{i}"].Style.Font.Color.SetColor(Color.Black);
            worksheet.Cells[$"L{i}"].Value = "C = Completed > Only AU process completed, Manual review is required";
            worksheet.Cells[$"L{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"L{i}"].Style.WrapText = false;
            i++;

            worksheet.Cells[$"B{i}"].Value = "A = Any Additional Research Unit as per GIS";
            worksheet.Cells[$"B{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"B{i}"].Style.Font.Color.SetColor(Color.Black);
            worksheet.Cells[$"L{i}"].Value = "F = Fail > Unable to open database, unable to fetch information, network issues";
            worksheet.Cells[$"L{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"L{i}"].Style.WrapText = false;
            i++;

            worksheet.Cells[$"B{i}"].Value = "D = Alternate Name (where DUNS name is different than PACE APG entity name)";
            worksheet.Cells[$"B{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"B{i}"].Style.Font.Color.SetColor(Color.Black);
            //worksheet.Cells[$"L{i}"].Value = "AP = Already processed in another tab in the same check";
            //worksheet.Cells[$"L{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            //worksheet.Cells[$"L{i}"].Style.WrapText = false;
            //i++;
            worksheet.Cells[$"L{i}"].Value = "R = Re-used research for a unit > AU will re-use the research done within 24 hours";
            worksheet.Cells[$"L{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"L{i}"].Style.WrapText = false;
            i++;
            worksheet.Cells[$"L{i}"].Value = "I = Inherited entities in Secondary Checks (carried forward from original check)";
            worksheet.Cells[$"L{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"L{i}"].Style.WrapText = false;

            i += 3;
            worksheet.Cells[$"B{i}"].Value = "Type of Unit*";
            worksheet.Cells[$"B{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"B{i}"].Style.Font.Color.SetColor(Color.Black);
            i++;
            worksheet.Cells[$"C{i}"].Value = "Entity / Individual / Unable to Decide";
            worksheet.Cells[$"C{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"C{i}"].Style.WrapText = false;
            i++;
            worksheet.Cells[$"C{i}"].Value = "For 'Unable to Decide', AU research them as 'Entities', if these happen to be an individual, please correct the research manually";
            worksheet.Cells[$"C{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"C{i}"].Style.WrapText = false;
            i++;
            worksheet.Cells[$"B{i}"].Value = "Whether Client Side or Non-Client Side ^";
            worksheet.Cells[$"B{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            SetCellBackgroundColor(worksheet.Cells[$"B{i}:C{i}"], Color.FromArgb(218, 150, 148));
            worksheet.Cells[$"B{i}"].Style.Font.Color.SetColor(Color.Black);
            i++;
            worksheet.Cells[$"C{i}"].Value = "For Client Side Entities, AU research on Linkage Type as Control – PE Investment” AND/OR \"Consolidated\"";
            worksheet.Cells[$"C{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"C{i}"].Style.WrapText = false;
            i++;
            worksheet.Cells[$"C{i}"].Value = "For Non-Client Side Entities, AU research on Linkage Type as \"Control – PE Investment\" AND/OR \"Consolidated\" AND/OR \"Significant Influence\"";
            worksheet.Cells[$"C{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"C{i}"].Style.WrapText = false;
            i++;
            SetCellBackgroundColor(worksheet.Cells[$"B{i}"], Color.FromArgb(196, 189, 151));
            i++;
            worksheet.Cells[$"C{i}"].Value = "All inherited entities in a Secondary Check is highlighted with this colour and AU does not perform research on such entities";
            worksheet.Cells[$"C{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"C{i}"].Style.WrapText = false;
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
    }


    public static SPLEntity ReadSPLData(string filePath)
    {
        SPLEntity splData = new SPLEntity();
        try
        {
            splData.splLists = ReadSheet<SPLList>(filePath, 0, 7);
            splData.commercialSensitivitiesLists = ReadSheet<CommercialSensitivitiesList>(filePath, 1, 2);
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var J1CellValue = worksheet.Cells["J1"]?.Value?.ToString();
                splData.vesionDetails = J1CellValue;
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }

        return splData;
    }
    public static RegionalEntity ReadRegionalData(string filePath)
    {
        RegionalEntity regionalData = new RegionalEntity();
        List<RegionalDeviations> regionalDeviations = new List<RegionalDeviations>();
        List<SupplementaryGuide> supplementaryGuides = new List<SupplementaryGuide>();
        try
        {
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[1];
                int startRow = 2;
                int endRow = worksheet.Dimension.Rows;
                for (int row = startRow; row <= endRow; row++)
                {
                    if (worksheet.Cells[row, 2]?.Value == null)
                    {
                        continue;
                    }
                    var regional = new RegionalDeviations
                    {
                        Area = worksheet.Cells[row, 1]?.Value?.ToString(),
                        Region = worksheet.Cells[row, 2]?.Value?.ToString(),
                        WorkAllocationByGDS = worksheet.Cells[row, 3]?.Value?.ToString(),
                        AdditionalLocalDB = worksheet.Cells[row, 4]?.Value?.ToString(),
                        GCSPConsultaion = worksheet.Cells[row, 5]?.Value?.ToString(),
                        SignoffGDS = worksheet.Cells[row, 6]?.Value?.ToString(),
                        OtherNuances = worksheet.Cells[row, 7]?.Value?.ToString()
                    };
                    regionalDeviations.Add(regional);
                }
                regionalData.regionalDeviations = regionalDeviations;
                var worksheetcom = package.Workbook.Worksheets[2];
                int startRowcCom = 7;
                int endRowCom = worksheetcom.Dimension.Rows;
                for (int row = startRowcCom; row <= endRowCom; row++)
                {
                    if (worksheetcom.Cells[row, 1]?.Value == null)
                    {
                        continue;
                    }
                    var supplementary = new SupplementaryGuide
                    {
                        SNo = int.Parse(worksheetcom.Cells[row, 1]?.Value?.ToString()),
                        Area = worksheetcom.Cells[row, 2]?.Value?.ToString(),
                        Region = worksheetcom.Cells[row, 3]?.Value?.ToString(),
                        ServiceLine = worksheetcom.Cells[row, 4]?.Value?.ToString(),
                        WorkAllocation = worksheetcom.Cells[row, 5]?.Value?.ToString(),
                        PreScreening = worksheetcom.Cells[row, 6]?.Value?.ToString(),
                        InfoRequestET = worksheetcom.Cells[row, 7]?.Value?.ToString(),
                        Research = worksheetcom.Cells[row, 8]?.Value?.ToString(),
                        Counterparty = worksheetcom.Cells[row, 9]?.Value?.ToString(),
                        Conclusion = worksheetcom.Cells[row, 10]?.Value?.ToString(),
                        Signoff = worksheetcom.Cells[row, 11]?.Value?.ToString(),
                        SupportingDocuments = worksheetcom.Cells[row, 12]?.Value?.ToString(),
                        ContactNames = worksheetcom.Cells[row, 13]?.Value?.ToString()
                    };
                    supplementaryGuides.Add(supplementary);
                }
                regionalData.supplementaryGuides = supplementaryGuides;
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
        return regionalData;
    }
    public static void ReadOnshoreData(string filePath, CheckerQueue summary)
    {
        List<OnshoreEntity> onshoreData = new List<OnshoreEntity>();
        try
        {
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets["Conflicts Network"];
                int startRow = 5;
                int endRow = worksheet.Dimension.Rows;
                for (int row = startRow; row <= endRow; row++)
                {
                    if (worksheet.Cells[row, 2]?.Value == null)
                    {
                        continue;
                    }
                    var entity = new OnshoreEntity
                    {
                        Area = worksheet.Cells[row, 2]?.Value?.ToString(),
                        Country = worksheet.Cells[row, 3]?.Value?.ToString(),
                        ServiceLine = worksheet.Cells[row, 4]?.Value?.ToString(),
                        PrimaryContact = worksheet.Cells[row, 5]?.Value?.ToString()
                    };
                    onshoreData.Add(entity);
                }
            }
            if (onshoreData.Any())
            {
                summary.onShore = onshoreData.GetPrimaryContact(summary.CountryName,
                                    summary.SubServiceLine, summary.SubServiceLineCode);
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
    }

    private static List<T> ReadSheet<T>(string filePath, int sheetIndex, int startRow) where T : new()
    {
        List<T> dataList = new List<T>();
        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets[sheetIndex];
            int endRow = worksheet.Dimension.Rows;
            for (int row = startRow; row <= endRow; row++)
            {
                var dataItem = new T();
                MapRowToEntity(worksheet, row, dataItem);
                dataList.Add(dataItem);
            }
        }
        return dataList;
    }

    private static void MapRowToEntity<T>(ExcelWorksheet worksheet, int row, T entity) where T : new()
    {
        for (int col = 1; col <= 16; col++)
        {
            try
            {
                var propertyName = typeof(T).GetProperties()[col - 1].Name;
                var cellValue = worksheet.Cells[row, col]?.Value?.ToString();
                SetProperty(entity, propertyName, cellValue);
            }
            catch
            {
                continue;
            }
        }
    }

    private static void SetProperty<T>(T entity, string propertyName, string value) where T : new()
    {
        var property = typeof(T).GetProperty(propertyName);
        if (property != null)
        {
            if (property.PropertyType == typeof(int))
            {
                if (int.TryParse(value, out int intValue))
                {
                    property.SetValue(entity, intValue);
                }
            }
            else if (property.PropertyType == typeof(DateTime))
            {
                if (DateTime.TryParse(value, out DateTime dateTimeValue))
                {
                    property.SetValue(entity, dateTimeValue);
                }
            }
            else
            {
                property.SetValue(entity, value);
            }
        }
    }

    public static void SaveUnitGridTab(ConflictCheck conflictCheck, CheckerQueue queue, SPLEntity sPLEntity, ResearchSummary rs,
        List<ResearchSummary> crs, string sFilePath, string destinationPath, List<string> keywordsList,
        AppConfigure configure, ProcessedChecks ProcessedLog, List<ResearchSummary> GISExtraResearch,
        List<ResearchSummary> MercuryExtraResearch, bool IsUKI, bool IsCRRGUP,
        List<string> keywordsListForFinScanSearch)
    {
        string additionalInfoOnError = $" - Computer: {System.Environment.MachineName}" +
                                       $" - Environment: {configure.Environment}" +
                                       $" - ConflictCheckID: {conflictCheck.ConflictCheckID}";

        try
        {
            List<MercuryEntity> mercuryEntities = new List<MercuryEntity>();
            List<FinalGISResults> lsUnitGrid_GISEntity = new List<FinalGISResults>();
            ConflictService conflictService = new ConflictService(configure);
            int iPresentRow;
            List<UnitGrid_FinscanEntity> lsUnitGrid_FinscanEntity = null;
            List<FileEmbedding> listFileEmbeddings = new List<FileEmbedding>();

            /* For now, this must be kept commented out, as per discussion with Vishnu about bug 1020861.
            string firstCEREntityAuditRelationship = string.Empty;
            string firstCEREntityServiceLines = string.Empty;
            */
            using (var package = new ExcelPackage(new FileInfo(sFilePath)))
            {
            GIS_REWORK:

                bool isValidEntitySearch = false;
                bool isValidDUNSSearch = false;
                bool isValidMDMIDSearch = false;
                bool isValidGISIDSearch = false;
                bool isDUNSSearchHappened = false;
                bool isMDMIDSearchHappened = false;
                bool isGISIDSearchHappened = false;

                var _WorksheetNo = rs.WorksheetNo;

                var existingWorksheet = package.Workbook.Worksheets.SingleOrDefault(x => x.Name == _WorksheetNo);
                if (existingWorksheet != null)
                {
                    package.Workbook.Worksheets.Delete(existingWorksheet);
                    package.Save();
                }
                var worksheet = package.Workbook.CopyWorksheet(
                                    sourceWorksheetName: CAUConstants.MASTER_WORKBOOK_RESEARCH_UNIT_TEMPLATE_TAB,
                                    targetWorksheetName: _WorksheetNo);  //GIS Tab
                if (worksheet == null)
                    return;
                iPresentRow = 1;
                worksheet.Cells.Style.Font.Name = "EYInterstate"; // Set the font name
                // worksheet.Cells.Style.Font.Size = 8;  // Keep this commented out. Do not interfere with Research Template default sizes.
                worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Header: 1st in processing order; 1st section vertically
                Console.WriteLine("  1. Header...");

                SetCellBackgroundColor(worksheet.Cells[$"A1, C1, E1"], darkGray);
                SetCellBackgroundColor(worksheet.Cells[$"B1, D1, F1"], brightTeal);
                FinalGISResults mainUnitGrid_GISEntity = new FinalGISResults();
                bool noRecordPerGIS = false;
                bool noRecordPerCER = false;
                worksheet.Cells["A1"].Value = "Entity / Individual Name";
                worksheet.Cells["B1"].Value = rs.EntityName;
                worksheet.Cells["C1"].Value = "Role (PACE)";
                worksheet.Cells["D1"].Value = rs.Role.Replace("GIS", "").Replace("CER", "").Replace("GUP of GUP of ", "GUP of ").Replace("GUP of  GUP of ", "GUP of ");
                worksheet.Cells["E1"].Value = "Comments (PACE)";
                worksheet.Cells["F1"].Value = rs.AdditionalComments;
                worksheet.Cells[$"A1:K1"].AutoFitColumns(35, 40);
                worksheet.Cells["A2"].Value = "Type of Unit";
                worksheet.Cells["B2"].Value = rs.Type;
                worksheet.Cells["C2"].Value = "Country (PACE)";
                worksheet.Cells["D2"].Value = rs.Country;
                worksheet.Cells[$"A2:K2"].AutoFitColumns(21, 35);
                worksheet.AutoFitRowHeight(1, 'A', 'F', allMergedCols: null, forceWrapText: true, 15.0 * 3);
                SetCellBackgroundColor(worksheet.Cells[$"A2, C2"], darkGray);
                SetCellBackgroundColor(worksheet.Cells[$"B2, D2"], brightTeal);
                worksheet.Cells["A3"].Value = "Client Relationship Summary:";
                worksheet.Cells["B3"].Style.WrapText = true;
                rs.ClientRelationshipSummary = new ClientRelationshipSummary();
                worksheet.Cells["B3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["B3:D3"].Merge = true;
                worksheet.Cells["A1:F1"].SetBorders(ExcelBorderStyle.Thin, ExcelBorderStyle.Thin);
                worksheet.Cells["A2:D3"].SetBorders(ExcelBorderStyle.Thin, ExcelBorderStyle.Thin);
                worksheet.Row(3).Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                SetCellBackgroundColor(worksheet.Cells[$"A3"], darkGray);

                _resultDetailsSectionsStartingRow = RESULT_DETAILS_SECTIONS_DEFAULT_STARTING_ROW;
                WriteDefaultHyperlinkBar(worksheet);

                worksheet.Cells["A5:R500"].ResetEverything("EYInterstate", 8,
                    ExcelHorizontalAlignment.Center, ExcelVerticalAlignment.Center);
                iPresentRow = 4;

                #region GIS Grid                
                // GIS Grid: 2nd in processing order; 2nd section vertically
                Console.WriteLine("  2. GIS Grid...");
                noRecordPerGIS = false;
                noRecordPerCER = false;
                GisSummaryGrid gisSummaryGrid = new(worksheet, ++iPresentRow);
                iPresentRow = gisSummaryGrid.WriteTitleAndHeaders();
                if (!rs.IsGISResearch)
                {
                    iPresentRow = gisSummaryGrid.WriteBodyWithNoData();
                    goto SKIP_GIS_GRID;
                }

                try
                {
                    List<SearchEntitiesResponseItemViewModel> gisRecords = new List<SearchEntitiesResponseItemViewModel>();
                    List<SearchEntitiesResponseItemViewModel> AddParentalRelationEntities = new List<SearchEntitiesResponseItemViewModel>();
                    List<SearchEntitiesResponseItemViewModel> GISEntityGrid = new List<SearchEntitiesResponseItemViewModel>();
                    FinalGISResults worksheetmainUnitGrid_GISEntity = new FinalGISResults();

                    string Notes = string.Empty;
                    string sKeyword = string.Empty;

                    foreach (var keyword in keywordsList)
                    {
                        if (ProcessedLog.Keywords != null && ProcessedLog.Keywords.Length > 0)
                            ProcessedLog.Keywords += " | " + keyword.ToString();
                        else
                            ProcessedLog.Keywords = keyword.ToString();

                        GISSearch searchList = new GISSearch();
                        searchList.Enity = keyword;
                        sKeyword = keyword;

                        if (rs.Type == "Individual")
                            searchList.IsIndividual = true;
                        else
                            searchList.IsIndividual = false;

                        var GIS = conflictService.GetGISEntitiesFromGISWebAPIAsync(searchList);
                        gisRecords.AddRange(GIS.Records);

                        //If you don't find anything then look with similar matches and include inactive
                        //no need to include inactive entities.
                        //if (GIS.TotalRecords == 0)
                        //{
                        //    var GISFuzzy = conflictService.GetGISEntitiesFromGISWebAPIAsync(searchList, true);
                        //    gisRecords.AddRange(GISFuzzy.Records);
                        //}
                    }

                    var GISEntityFiles = ConflictService.GetGISInfoForEmbeddedFiles(conflictCheck, rs, gisRecords.DistinctBy(i => i.GisId).ToList(), GISExtraResearch, rs.EntityName, destinationPath, configure, $"GE_{sKeyword}");

                    if (!string.IsNullOrEmpty(rs.GISID))
                        GISEntityGrid = gisRecords.DistinctBy(i => i.GisId).Where(gis => gis.GisId.ToString() == rs.GISID.ToString()).ToList();

                    if (GISEntityGrid.Count == 0 && string.IsNullOrEmpty(rs.GISID)) //Populate all of the GIS data table columns where there is no UID match and only one entity name with a 100% match. if UID is not available
                    {
                        if (GISEntityFiles.EntityMatchCount == 1)
                        {
                            SearchEntitiesResponseItemViewModel_SpreadSheet _svms = GISEntityFiles.EntityMatchRecords.FirstOrDefault();
                            GISEntityGrid = gisRecords.DistinctBy(i => i.GisId).Where(gis => gis.GisId.ToString() == _svms.GISID.ToString()).ToList();
                        }
                    }

                    if (GISEntityGrid.Count == 0)
                    {
                        if (GISEntityFiles.EntityMatchCount > 1)
                        {
                            rs.ClientRelationshipSummary.GISNotes = CAUConstants.GIS_MULTIPLE_CLOSE_MATCHES;
                            //  rs.ClientRelationshipSummary.GISDesc = "Multiple close matches identified in GIS with business name, please consider further analysis";
                        }
                    }
                    else
                        isValidEntitySearch = true;

                    IEnumerable<SearchEntitiesResponseItemViewModel> GISDUNSGrid = new List<SearchEntitiesResponseItemViewModel>();
                    IEnumerable<SearchEntitiesResponseItemViewModel> GISMDMGrid = new List<SearchEntitiesResponseItemViewModel>();
                    IEnumerable<SearchEntitiesResponseItemViewModel> GISGISGrid = new List<SearchEntitiesResponseItemViewModel>();

                    // GIS Extra Loop
                    if (GISEntityGrid.ToList().Count == 0)
                    {
                        GISSearch searchList = new GISSearch();
                        List<SearchEntitiesResponseItemViewModel> gisExtraRecords = new List<SearchEntitiesResponseItemViewModel>();

                        if (rs.DUNSNumber?.Length > 0)
                        {
                            isDUNSSearchHappened = true;
                            searchList.DUNSNumber = rs.DUNSNumber.ToString();

                            if (rs.Type == "Individual")
                                searchList.IsIndividual = true;

                            var GISDUNS = conflictService.GetGISEntitiesFromGISWebAPIAsync(searchList);
                            gisExtraRecords.AddRange(GISDUNS.Records);

                            var GISDUNSFiles = ConflictService.GetGISInfoForEmbeddedFiles(conflictCheck, rs, gisExtraRecords.DistinctBy(i => i.GisId).ToList(),
                                GISExtraResearch, rs.EntityName, destinationPath, configure, $"GD_{searchList.DUNSNumber}");

                            if (!string.IsNullOrEmpty(rs.DUNSNumber?.ToString()))
                                GISEntityGrid = gisExtraRecords.DistinctBy(i => i.GisId).Where(gis => gis.DUNSNumber.ToString() == rs.DUNSNumber.ToString()).ToList();

                            if (GISEntityGrid.ToList().Count == 0)
                            {
                                if (GISDUNSFiles.EntityMatchCount > 1)
                                {
                                    rs.ClientRelationshipSummary.GISNotes = CAUConstants.GIS_MULTIPLE_CLOSE_MATCHES;
                                    //  rs.ClientRelationshipSummary.GISDesc = "Multiple close matches identified in GIS with business name, please consider further analysis";
                                }
                            }
                            else
                                isValidDUNSSearch = true;
                        }

                        if (gisExtraRecords.ToList().Count == 0 || GISEntityGrid.ToList().Count == 0)
                        {
                            if (rs.MDMID?.Length > 0)
                            {
                                isMDMIDSearchHappened = true;

                                searchList.MDMID = rs.MDMID.ToString();
                                if (rs.Type == "Individual")
                                    searchList.IsIndividual = true;

                                var GISMDM = conflictService.GetGISEntitiesFromGISWebAPIAsync(searchList);
                                gisExtraRecords.AddRange(GISMDM.Records);

                                var GISMDMFiles = ConflictService.GetGISInfoForEmbeddedFiles(conflictCheck, rs, gisExtraRecords.DistinctBy(i => i.GisId).ToList(),
                                    GISExtraResearch, rs.EntityName, destinationPath, configure, $"GM_{searchList.MDMID}");

                                if (!string.IsNullOrEmpty(rs.MDMID?.ToString()))
                                    GISEntityGrid = gisExtraRecords.DistinctBy(i => i.GisId).Where(gis => gis.MDMID.ToString() == rs.MDMID.ToString()).ToList();

                                if (GISEntityGrid.ToList().Count == 0)
                                {
                                    if (GISMDMFiles.EntityMatchCount > 1)
                                    {
                                        rs.ClientRelationshipSummary.GISNotes = CAUConstants.GIS_MULTIPLE_CLOSE_MATCHES;
                                        //rs.ClientRelationshipSummary.GISDesc = "Multiple close matches identified in GIS with business name, please consider further analysis";
                                    }
                                }
                                else
                                    isValidMDMIDSearch = true;
                            }
                            if (gisExtraRecords.ToList().Count == 0 || GISEntityGrid.ToList().Count == 0)
                            {
                                if (rs.GISID?.Length > 0)
                                {
                                    isGISIDSearchHappened = true;
                                    searchList.GISID = rs.GISID.ToString();
                                    if (rs.Type == "Individual")
                                        searchList.IsIndividual = true;

                                    var gisGIS = conflictService.GetGISEntitiesFromGISWebAPIAsync(searchList);
                                    gisExtraRecords.AddRange(gisGIS.Records);

                                    var GISGISIDFiles = ConflictService.GetGISInfoForEmbeddedFiles(conflictCheck, rs, gisExtraRecords.DistinctBy(i => i.GisId).ToList(),
                                        GISExtraResearch, rs.EntityName, destinationPath, configure, $"GG_{searchList.GISID}");

                                    if (!string.IsNullOrEmpty(rs.GISID?.ToString()))
                                        GISEntityGrid = gisExtraRecords.DistinctBy(i => i.GisId).Where(gis => gis.GisId.ToString() == rs.GISID.ToString()).ToList();

                                    if (GISEntityGrid.ToList().Count == 0)
                                    {
                                        if (GISGISIDFiles?.EntityMatchCount > 1)
                                        {
                                            rs.ClientRelationshipSummary.GISNotes = CAUConstants.GIS_MULTIPLE_CLOSE_MATCHES;
                                            // rs.ClientRelationshipSummary.GISDesc = "Multiple close matches identified in GIS with business name, please consider further analysis";
                                        }
                                    }
                                    else
                                        isValidGISIDSearch = true;
                                }
                            }
                        }
                        if ((isValidDUNSSearch || isValidMDMIDSearch || isValidGISIDSearch) && rs.ClientRelationshipSummary.GISNotes == CAUConstants.GIS_MULTIPLE_CLOSE_MATCHES)
                            rs.ClientRelationshipSummary.GISNotes = rs.ClientRelationshipSummary.GISNotes.Replace(CAUConstants.GIS_MULTIPLE_CLOSE_MATCHES, string.Empty);
                    }

                    //ParentalRelationship -- Entity
                    string sGISIDs = string.Join(",", GISEntityGrid.DistinctBy(i => i.GisId).ToList().Select(x => x.GisId));
                    bool unitgrid_ParentRelationship = false;
                    if (sGISIDs != "")
                    {
                        var parentalRelations = conflictService.GetParentalRelationship(sGISIDs);
                        if (rs.GISID?.Length > 0)
                            unitgrid_ParentRelationship = parentalRelations.Where(gis => gis.BaseGISID.ToString().Contains(rs.GISID.ToString())).Count() > 0;

                        //Rule#3, 4 to add additional GIS research
                        //Collect all GISIDs from GISRecords where Parental Relationship is Control – PE Investment OR Consolidated OR startswith Significant Influence
                        if (unitgrid_ParentRelationship)
                        {
                            List<string> ParentalRelationshipAdditional = new List<string>();
                            if (!rs.IsClientSide)
                            {
                                ParentalRelationshipAdditional = parentalRelations.Where(i => (i.RelationshipName.StartsWith("Control") && i.SecondaryRelationshipAttribute.StartsWith("PE Investment"))
                                        || i.RelationshipName.Contains("Consolidated") || i.RelationshipName.StartsWith("Significant Influence"))
                                    .Select(i => i.Entity.ToString()).ToList();
                            }
                            //no significant 
                            else
                            {
                                ParentalRelationshipAdditional = parentalRelations.Where(i => (i.RelationshipName.StartsWith("Control") && i.SecondaryRelationshipAttribute.StartsWith("PE Investment"))
                                        || i.RelationshipName.Contains("Consolidated"))
                                    .Select(i => i.Entity.ToString()).ToList();
                            }

                            AddParentalRelationEntities = GISEntityGrid.DistinctBy(i => i.GisId).Where(i => ParentalRelationshipAdditional.Contains(i.GisId.ToString())).ToList();
                            var AddParent = parentalRelations.Where(i => ParentalRelationshipAdditional.Contains(i.Entity.ToString())).ToList();

                            if (AddParent.ToList().Where(x => !string.IsNullOrEmpty(x.ParentDUNSNumber)).Count() > 0)
                            {
                                AddParent.RemoveAll(x => rs.EntityWithoutLegalExt.ToLower() == x.EntityWithoutLegalExt.ToLower()
                                                && rs.DUNSNumber == x.ParentDUNSNumber);
                            }
                            else if (AddParent.ToList().Where(x => !string.IsNullOrEmpty(x.ParentGISID.ToString())).Count() > 0)
                            {
                                AddParent.RemoveAll(x => rs.EntityWithoutLegalExt.ToLower() == x.EntityWithoutLegalExt.ToLower()
                                                && rs.GISID == x.ParentGISID.ToString());
                            }
                            else if (AddParent.ToList().Where(x => !string.IsNullOrEmpty(x.ParentMDMID)).Count() > 0)
                            {
                                AddParent.RemoveAll(x => rs.EntityWithoutLegalExt.ToLower() == x.EntityWithoutLegalExt.ToLower()
                                                && rs.MDMID == x.ParentMDMID);
                            }
                            else if (AddParent.ToList().Where(x => !string.IsNullOrEmpty(x.Country)).Count() > 0)
                            {
                                AddParent.RemoveAll(x => rs.EntityWithoutLegalExt.ToLower() == x.EntityWithoutLegalExt.ToLower()
                                                && rs.Country == x.Country);
                            }

                            if (AddParent.Count > 0)
                            {
                                GISExtraResearch.AddRange(AddParent.Select(x => new ResearchSummary()
                                {

                                    EntityName = x.Entity,
                                    DUNSNumber = x.ParentDUNSNumber,
                                    GISID = x.ParentGISID.ToString(),
                                    MDMID = x.ParentMDMID.ToString(),
                                    Country = x.Country,
                                    //   Role = "GIS " + rs.Role,
                                    Role = rs.Role,
                                    ParentalRelationship = !string.IsNullOrEmpty(x.SecondaryRelationshipAttribute) ? x.RelationshipName + " - " + x.SecondaryRelationshipAttribute : x.RelationshipName,
                                    //   WorksheetNo = rs.Role.Contains("Additional Research Unit") ? rs.WorksheetNo : rs.WorksheetNo.Split('A')[0],
                                    WorksheetNo = rs.WorksheetNo,
                                    Type = x.Type,
                                    EntityWithoutLegalExt = x.EntityWithoutLegalExt,
                                    IsClientSide = rs.IsClientSide,
                                    IsFinscanResearch = rs.IsFinscanResearch,
                                    IsCERResearch = rs.IsCERResearch,
                                    IsCRRResearch = rs.IsCRRResearch,
                                    IsSPLResearch = rs.IsSPLResearch,
                                    IsGISResearch = rs.IsGISResearch,
                                    Rework = rs.Rework,
                                    PerformResearch = rs.PerformResearch
                                }));

                            }

                            if (GISExtraResearch.Count > 0)
                            {
                                foreach (var i in GISExtraResearch)
                                {
                                    if (i.ParentalRelationship != null && (i.ParentalRelationship.StartsWith("Consolidated") || i.ParentalRelationship.Contains("Control")) && i.IsClientSide)
                                    {
                                        i.IsCERResearch = false;
                                        i.IsCRRResearch = false;
                                    }
                                    if (i.ParentalRelationship != null && i.ParentalRelationship.Contains("Significant"))
                                    {
                                        i.IsCERResearch = false;
                                        i.IsCRRResearch = false;
                                        i.IsGISResearch = false;
                                        i.IsFinscanResearch = false;
                                    }
                                }
                            }

                        }
                    }

                    //Entity - Keywords
                    if (!isValidEntitySearch || !isValidDUNSSearch || !isValidMDMIDSearch || !isValidGISIDSearch)
                    {
                        if (!isValidEntitySearch)
                        {
                            iPresentRow++;
                            worksheet.Cells[$"A{iPresentRow}"].Value = string.Join(", ", keywordsList);
                            SetCellBackgroundColor(worksheet.Cells[$"A{iPresentRow}:R{iPresentRow}"], brightTeal);
                        }
                        if (!isValidDUNSSearch && !isValidEntitySearch && !string.IsNullOrEmpty(rs.DUNSNumber))
                        {
                            iPresentRow++;
                            worksheet.Cells[$"A{iPresentRow}"].Value = "DUNSNumber: " + rs.DUNSNumber?.ToString();
                            SetCellBackgroundColor(worksheet.Cells[$"A{iPresentRow}:R{iPresentRow}"], brightTeal);
                        }
                        if (!isValidMDMIDSearch && (!isValidEntitySearch && !isValidDUNSSearch) && !string.IsNullOrEmpty(rs.MDMID))
                        {
                            iPresentRow++;
                            worksheet.Cells[$"A{iPresentRow}"].Value = "MDMID: " + rs.MDMID.ToString();
                            SetCellBackgroundColor(worksheet.Cells[$"A{iPresentRow}:R{iPresentRow}"], brightTeal);
                        }
                        if (!isValidGISIDSearch && (!isValidEntitySearch && !isValidMDMIDSearch && !isValidDUNSSearch) && !string.IsNullOrEmpty(rs.GISID))
                        {
                            iPresentRow++;
                            worksheet.Cells[$"A{iPresentRow}"].Value = "GISID: " + rs.GISID.ToString();
                            SetCellBackgroundColor(worksheet.Cells[$"A{iPresentRow}:R{iPresentRow}"], brightTeal);
                        }
                        if (GISEntityGrid.ToList().Count == 0)
                        {
                            //if multiple close matches then don't fill up datatable
                            if (rs.ClientRelationshipSummary.GISNotes == CAUConstants.GIS_MULTIPLE_CLOSE_MATCHES)
                            {
                                worksheet.Cells[$"B{iPresentRow}:M{iPresentRow}"].Merge = true;
                                noRecordPerGIS = true;
                            }
                            else if (string.IsNullOrEmpty(rs.ClientRelationshipSummary.GISNotes))
                            {
                                worksheet.Cells[$"B{iPresentRow}:M{iPresentRow}"].Merge = true;
                                worksheet.Cells[$"B{iPresentRow}"].Value = "No Results identified in GIS with the keyword search string after filtering by the country and UID and applying fuzzy match threshold. Please perform web and MDM searches.";
                                noRecordPerGIS = true;
                            }
                        }
                    }
                    //re-work :(
                    if (GISEntityGrid.ToList().Count == 1 && (string.IsNullOrEmpty(rs.DUNSNumber) || string.IsNullOrEmpty(rs.MDMID)
                        || string.IsNullOrEmpty(rs.GISID)))
                    {
                        //GISRework(rs, GISEntityGrid);
                        bool isDUNSchanged = false;
                        bool isGISIDchanged = false;
                        bool isMDMIDchanged = false;
                        bool isCountrychanged = false;
                        List<ResearchSummary> GISGUPTemp = new List<ResearchSummary>();

                        foreach (SearchEntitiesResponseItemViewModel unitGrid_GISEntity in GISEntityGrid)
                        {
                            //As per the story, if we are missing any IDs in AU unit grid then back fill
                            //if (unitGrid_GISEntity.CountryName == rs.Country)
                            //{
                            if (string.IsNullOrEmpty(rs.DUNSNumber) && !string.IsNullOrEmpty(unitGrid_GISEntity.DUNSNumber))
                            {
                                rs.DUNSNumber = unitGrid_GISEntity.DUNSNumber;
                                GISGUPTemp.AddRange(conflictService.GetGISGUPInfo(rs.DUNSNumber, "DUNS", rs));
                                conflictService.RemoveDupes(GISExtraResearch, GISGUPTemp);
                                conflictService.RemoveDupes(crs, GISGUPTemp);
                                GISExtraResearch.AddRange(GISGUPTemp);


                                isDUNSchanged = true;
                            }
                            if (string.IsNullOrEmpty(rs.MDMID) && !string.IsNullOrEmpty(unitGrid_GISEntity.MDMID.ToString())
                                && unitGrid_GISEntity.MDMID.ToString() != "0")
                            {
                                rs.MDMID = unitGrid_GISEntity.MDMID.ToString();
                                GISGUPTemp.AddRange(conflictService.GetGISGUPInfo(rs.MDMID, "MDM", rs));
                                conflictService.RemoveDupes(GISExtraResearch, GISGUPTemp);
                                conflictService.RemoveDupes(crs, GISGUPTemp);
                                GISExtraResearch.AddRange(GISGUPTemp);


                                isMDMIDchanged = true;
                            }
                            if (string.IsNullOrEmpty(rs.GISID) && !string.IsNullOrEmpty(unitGrid_GISEntity.GisId.ToString()))
                            {
                                rs.GISID = unitGrid_GISEntity.GisId.ToString();
                                GISGUPTemp.AddRange(conflictService.GetGISGUPInfo(rs.GISID, "GISID", rs));
                                conflictService.RemoveDupes(GISExtraResearch, GISGUPTemp);
                                conflictService.RemoveDupes(crs, GISGUPTemp);
                                GISExtraResearch.AddRange(GISGUPTemp);

                                isGISIDchanged = true;
                            }
                            if (string.IsNullOrEmpty(rs.Country) && !string.IsNullOrEmpty(unitGrid_GISEntity.CountryName.ToString()))
                            {
                                rs.Country = unitGrid_GISEntity.CountryName.ToString();
                                isCountrychanged = true;
                            }
                            //}

                            if (isGISIDchanged || isDUNSchanged || isMDMIDchanged || isCountrychanged)
                            {
                                rs.Type = GetSubjectType(rs);
                                goto GIS_REWORK;
                            }
                        }
                    }
                    if (GISEntityGrid.ToList().Count > 0)
                    {
                        foreach (SearchEntitiesResponseItemViewModel unitGrid_GISEntity in GISEntityGrid)
                        {

                            string Restrictions = string.Empty;
                            List<Restrictions> RestrictionsList = new List<Restrictions>();
                            if (!string.IsNullOrEmpty(unitGrid_GISEntity.GisId.ToString()))
                            {
                                RestrictionsList = ConflictService.GetRestrictions(unitGrid_GISEntity.GisId.ToString(), configure.GISConnectionString).ToList();
                                if (RestrictionsList.Count > 0)
                                    Restrictions = RestrictionsList.FirstOrDefault().RestrictionName;
                            }

                            List<UnitGrid_GISEntity> GISNewFields = new List<UnitGrid_GISEntity>();
                            if (!string.IsNullOrEmpty(unitGrid_GISEntity.GisId.ToString()))
                                GISNewFields = conflictService.GetNewGISFields(unitGrid_GISEntity.GisId);

                            bool Subsidiaries = false;
                            bool PIE = false;
                            bool PIEAffiliate = false;
                            bool G360 = false;

                            if (GISNewFields.Count > 0)
                            {
                                Subsidiaries = Convert.ToBoolean(GISNewFields.FirstOrDefault().Subsidiaries);
                                PIE = Convert.ToBoolean(GISNewFields.FirstOrDefault().PIE);
                                PIEAffiliate = Convert.ToBoolean(GISNewFields.FirstOrDefault().PIEAffiliate);
                                G360 = Convert.ToBoolean(GISNewFields.FirstOrDefault().G360); //TODO Tejal take a look, GIS team will supply
                            }

                            string searchType = string.Join(", ", keywordsList);
                            if (isValidDUNSSearch)
                                searchType = "DUNSNumber: " + rs.DUNSNumber?.ToString();
                            if (isValidMDMIDSearch)
                                searchType = "MDMID: " + rs.MDMID.ToString();
                            if (isValidGISIDSearch)
                                searchType = "GISID: " + rs.GISID.ToString();
                            unitGrid_GISEntity.LAP ??= new Lap();
                            unitGrid_GISEntity.LAP.FirstName = string.IsNullOrEmpty(unitGrid_GISEntity.LAP.FirstName.ToString()) ? "-" : unitGrid_GISEntity.LAP.FirstName + " " + unitGrid_GISEntity.LAP.LastName;
                            unitGrid_GISEntity.LAP.LastName = "";
                            if (unitGrid_GISEntity.AdditionalLaps.Any())
                            {
                                foreach (var lap in unitGrid_GISEntity.AdditionalLaps)
                                {
                                    var fullName = lap.FirstName + " " + lap.LastName;
                                    unitGrid_GISEntity.LAP.FirstName = ConcatWithComma(unitGrid_GISEntity.LAP.FirstName, fullName);
                                }
                            }

                            iPresentRow++;
                            worksheet.Cells[$"A{iPresentRow}"].Value = searchType;
                            worksheet.Cells[$"A{iPresentRow}"].Style.WrapText = true;
                            worksheet.Cells[$"B{iPresentRow}"].Value = unitGrid_GISEntity.EntityName;
                            worksheet.Cells[$"B{iPresentRow}"].Style.WrapText = true;
                            worksheet.Cells[$"C{iPresentRow}"].Value = string.IsNullOrEmpty(string.Join(", ", unitGrid_GISEntity.Aliases)) ? "-" : string.Join(", ", unitGrid_GISEntity.Aliases);
                            worksheet.Cells[$"C{iPresentRow}"].Style.WrapText = true;
                            worksheet.Cells[$"D{iPresentRow}"].Value = string.IsNullOrEmpty(unitGrid_GISEntity.CountryName) ? "-" : unitGrid_GISEntity.CountryName;
                            worksheet.Cells[$"E{iPresentRow}"].Value = string.IsNullOrEmpty(unitGrid_GISEntity.DUNSNumber) ? "-" : unitGrid_GISEntity.DUNSNumber;
                            worksheet.Cells[$"F{iPresentRow}"].Value = GISNewFields.Select(x => string.IsNullOrEmpty(x.MDMGFISID) ? "-" : x.MDMGFISID);
                            worksheet.Cells[$"G{iPresentRow}"].Value = string.IsNullOrEmpty(Restrictions) ? "-" : Restrictions;
                            worksheet.Cells[$"G{iPresentRow}"].Style.WrapText = true;
                            worksheet.Cells[$"H{iPresentRow}"].Value = string.IsNullOrEmpty(unitGrid_GISEntity.GCSP.FirstName.ToString()) ? "-" : unitGrid_GISEntity.GCSP.FirstName + " " + unitGrid_GISEntity.GCSP.LastName;
                            worksheet.Cells[$"I{iPresentRow}"].Value = unitGrid_GISEntity.IsAuditClient ? "Yes" : "No";
                            worksheet.Cells[$"J{iPresentRow}"].Value = G360 ? "Yes" : "No";
                            worksheet.Cells[$"K{iPresentRow}"].Value = string.IsNullOrEmpty(unitGrid_GISEntity.GisId.ToString()) ? "-" : unitGrid_GISEntity.GisId;
                            worksheet.Cells[$"L{iPresentRow}"].Value = string.IsNullOrEmpty(unitGrid_GISEntity.GUPName) ? "-" : unitGrid_GISEntity.GUPName;
                            worksheet.Cells[$"M{iPresentRow}"].Value = string.IsNullOrEmpty(unitGrid_GISEntity.GupDUNSNumber) ? "-" : unitGrid_GISEntity.GupDUNSNumber;
                            worksheet.Cells[$"N{iPresentRow}"].Value = unitgrid_ParentRelationship ? "Yes" : "No";
                            worksheet.Cells[$"O{iPresentRow}"].Value = Subsidiaries ? "Yes" : "No";
                            worksheet.Cells[$"P{iPresentRow}"].Value = unitGrid_GISEntity.LAP.FirstName;
                            worksheet.Cells[$"P{iPresentRow}"].Style.WrapText = true;
                            worksheet.Cells[$"Q{iPresentRow}"].Value = PIE ? "Yes" : "No";
                            worksheet.Cells[$"R{iPresentRow}"].Value = PIEAffiliate ? "Yes" : "No";
                            worksheet.Cells[$"A{iPresentRow}:R{iPresentRow}"].AutoFitColumns(21, 35);
                            SetCellBackgroundColor(worksheet.Cells[$"A{iPresentRow}:R{iPresentRow}"], brightTeal);
                            worksheet.Cells[$"A5:R{iPresentRow}"].SetBorders(ExcelBorderStyle.Thin, ExcelBorderStyle.Thin);
                            worksheetmainUnitGrid_GISEntity.DUNSNumber = unitGrid_GISEntity.DUNSNumber;
                            worksheetmainUnitGrid_GISEntity.EntityUnderAudit = unitGrid_GISEntity.IsAuditClient;
                            worksheetmainUnitGrid_GISEntity.GCSPName = unitGrid_GISEntity.GCSP == null ? "NA" : unitGrid_GISEntity.GCSP.FirstName + " " + unitGrid_GISEntity.GCSP.LastName;
                            worksheetmainUnitGrid_GISEntity.LAPName = unitGrid_GISEntity.LAP == null ? "NA" : unitGrid_GISEntity.LAP.FirstName + " " + unitGrid_GISEntity.LAP.LastName;
                            worksheetmainUnitGrid_GISEntity.Restrictions = string.IsNullOrEmpty(Restrictions) ? "-" : Restrictions;
                            worksheetmainUnitGrid_GISEntity.PIEString = PIE ? "Yes" : "No";
                            worksheetmainUnitGrid_GISEntity.PIEAffiliateString = PIEAffiliate ? "Yes" : "No";
                            worksheetmainUnitGrid_GISEntity.Notes = Notes;
                        }
                    }


                    mainUnitGrid_GISEntity = worksheetmainUnitGrid_GISEntity;

                    string clientSummaryRestrictions = mainUnitGrid_GISEntity?.Restrictions;
                    if (clientSummaryRestrictions != null && clientSummaryRestrictions.StartsWith("-"))
                        clientSummaryRestrictions = "NA";
                    string researchDUNS = mainUnitGrid_GISEntity?.DUNSNumber;
                    if (string.IsNullOrEmpty(researchDUNS) && !string.IsNullOrEmpty(rs.DUNSNumber))
                        researchDUNS = rs.DUNSNumber;

                    rs.ClientRelationshipSummary.Duns = string.IsNullOrEmpty(researchDUNS) ? "NA" : researchDUNS;
                    string auditStatus = mainUnitGrid_GISEntity.EntityUnderAudit ? "Yes (As per GIS)" : "No (As per GIS)";
                    string recordStatus = noRecordPerGIS ? " / No record found with UID (As per GIS)" : string.Empty;
                    if (!string.IsNullOrEmpty(rs.ClientRelationshipSummary.GISNotes))
                        rs.ClientRelationshipSummary.GISDesc = CAUConstants.GIS_ENTITY_UNDER_AUDIT_UNDEFINED__NO_RECORD_WITH_UID;
                    if (string.IsNullOrEmpty(rs.ClientRelationshipSummary.GISNotes) && !noRecordPerGIS)
                        rs.ClientRelationshipSummary.GISDesc = $"Entity Under Audit: {auditStatus}{recordStatus}";
                    rs.ClientRelationshipSummary.GCSP = string.IsNullOrWhiteSpace(mainUnitGrid_GISEntity?.GCSPName) ? "NA" : mainUnitGrid_GISEntity.GCSPName;
                    rs.ClientRelationshipSummary.LAP = string.IsNullOrWhiteSpace(mainUnitGrid_GISEntity?.LAPName) ? "NA" : mainUnitGrid_GISEntity.LAPName;
                    rs.ClientRelationshipSummary.Restrictions = clientSummaryRestrictions;
                    rs.ClientRelationshipSummary.PIE = mainUnitGrid_GISEntity?.PIEString ?? "NA";
                    rs.ClientRelationshipSummary.PIEAffiliate = mainUnitGrid_GISEntity?.PIEAffiliateString ?? "NA";
                    rs.ClientRelationshipSummary.CERNotes = mainUnitGrid_GISEntity.Notes;
                    if (GISEntityGrid.Count == 0)
                    {
                        if (rs.ClientRelationshipSummary.GISDesc == null)
                            rs.ClientRelationshipSummary.GISDesc = CAUConstants.GIS_NO_RECORD_WITH_UID;
                    }
                    else if (GISEntityGrid.Count > 1)
                    {
                        // rs.ClientRelationshipSummary.GISDesc = "Multiple close matches identified in GIS with business name, please consider further analysis";
                        rs.ClientRelationshipSummary.GCSP = rs.ClientRelationshipSummary.LAP = rs.ClientRelationshipSummary.Restrictions = rs.ClientRelationshipSummary.PIE = rs.ClientRelationshipSummary.PIEAffiliate = "NA";
                        rs.NoEYRelationshipWithCounterparties = true;
                    }
                    rs.GIS = "C";
                }
                catch (Exception ex)
                {
                    rs.GIS = "F";
                    rs.ClientRelationshipSummary.SearchDesc = "Please perform GIS search manually";
                    LoggerInfo.LogException(ex);
                }
            #endregion GIS Grid
            SKIP_GIS_GRID:
                _resultDetailsSectionsStartingRow = iPresentRow + 2;

                #region CER Grid                
                // CER(Mercury) Grid: 3rd in processing order; 3rd section vertically
                Console.WriteLine("  3. CER Grid...");

                CerSummaryGrid cerSummaryGrid = new(worksheet, ++iPresentRow);
                iPresentRow = cerSummaryGrid.WriteTitleAndHeaders();

                if (!rs.IsCERResearch)
                {
                    iPresentRow = cerSummaryGrid.WriteBodyWithNoData();
                    goto SKIP_CER_GRID;
                }

                try
                {
                    List<MercuryEntity_SpreadSheet> removeemptydunsName = new List<MercuryEntity_SpreadSheet>();
                    var mdata = conflictService.GetMercuryEntitiesData(conflictCheck, rs, keywordsList, rs.EntityName, rs.DUNSNumber, destinationPath, IsUKI);

                    //DUNSNumber match - 1st step
                    var cerexactMatchWithDUNS = mdata.EntityMatchRecords.Where(s => s.DunsNumber == rs.DUNSNumber && !string.IsNullOrEmpty(rs.DUNSNumber)).Distinct().ToList();

                    mdata.EntityMatchCount = cerexactMatchWithDUNS.Select(i => i.DunsNumber).Distinct().ToList().Count;

                    //Fill out master grid based on DUNS                

                    if (mdata.EntityMatchCount == 1)
                    {
                        var dunsName = cerexactMatchWithDUNS.ToList().Where(i => !(i.DUNSNameWithoutLegalExt.Equals(i.EntityWithoutLegalExt)));
                        removeemptydunsName = dunsName.Where(s => !string.IsNullOrWhiteSpace(s.DunsName)).Distinct().ToList();
                    }

                    if (mdata.EntityMatchCount == 0)
                    {
                        var entityCERAdditional = mdata.EntityMatchRecords.Where(i => i.AN_Fuzzy.Equals(100) || i.BN_Fuzzy.Equals(100)).Distinct().ToList();

                        mdata.EntityMatchCount = entityCERAdditional.Select(i => i.DunsNumber).Distinct().ToList().Count;

                        //Issue 1022636 - CER missing hit 06/27/24 - we do not need to check DUNSLocation
                        //if (entityCERAdditional.Count > 1)
                        //    if (entityCERAdditional.Where(i => i.DunsLocation.Equals(rs.Country)).ToList().Count == 1)
                        //        entityCERAdditional = entityCERAdditional.Where(i => i.DunsLocation.Equals(rs.Country)).ToList();
                        List<MercuryEntity_SpreadSheet> findcloseMatches = new List<MercuryEntity_SpreadSheet>();
                        if (entityCERAdditional.Count > 1)
                        {
                            findcloseMatches = entityCERAdditional.DistinctBy(i => i.DunsNumber).Where(i => !string.IsNullOrEmpty(i.DunsNumber)).ToList();
                            if (findcloseMatches.Count == 1)  //fill up only one found (Issue 1022733: CER data table incorrectly filled)
                                cerexactMatchWithDUNS = findcloseMatches;
                        }

                        //No need to fill up data grid Bug 1021957: UAT Bug 971631: Client Relationship Summary missteps, Pt#1 start
                        // :) Manisha said fill up if its a same entity - meaning same DUNSNumber. so let's say we have 3 lines and same DUNS then fill up.
                        // If no DUNS from AU Unitgrid only then fill up datatable and look for dunsname analysis also
                        if (entityCERAdditional.Count == 1 && string.IsNullOrEmpty(rs.DUNSNumber))
                            cerexactMatchWithDUNS = entityCERAdditional;

                        //close match with org entity -- 2nd step
                        //Based on new rules you don't care if there are close matches and DUNSName keywords are not same as org keywords then add
                        //var exactMatchWithOrgEntity = mdata.EntityMatchRecords.Where(s => ConflictService.DropLegalExtension(s.Client, rs.Type).ToLower() == rs.EntityWithoutLegalExt.ToLower()).ToList();

                        //var dunsName = exactMatchWithOrgEntity.ToList().Where(i => !(ConflictService.DropLegalExtension(i.DunsName, rs.Type).ToLower())
                        //                 .Equals(ConflictService.DropLegalExtension(i.Client, rs.Type).ToLower()));

                        if (entityCERAdditional.Count == 1)
                            removeemptydunsName = cerexactMatchWithDUNS.Where(s => !string.IsNullOrWhiteSpace(s.DunsName)).Distinct().ToList();
                    }
                    List<string> dNamekeywordsList = new List<string>();
                    List<string> exceptKeywords = new List<string>();

                    var MissingList = new List<MercuryEntity_SpreadSheet>();
                    foreach (var d in removeemptydunsName.Distinct())
                    {
                        KeyGen.KeyGen keywordGenerator = ConflictService.GetKeywordGenerator(d.DunsName, rs.DUNSNumber, rs.GISID, rs.Country);


                        dNamekeywordsList = keywordGenerator.GenerateKey(d.DunsName)
                                            .Select(name => ConflictService.RemoveSpecialCharacters(name))
                                            .Distinct(StringComparer.OrdinalIgnoreCase)
                                            .ToList();

                        exceptKeywords = dNamekeywordsList.Where(t2 => !keywordsList.Any(t1 => t2.Contains(t1))).ToList();

                        if (exceptKeywords.Count > 0)
                            if (exceptKeywords != null)
                            {
                                MissingList.Add(d);
                            }
                    }

                    if (MissingList.Count > 0)
                    {
                        MercuryExtraResearch.AddRange(MissingList.DistinctBy(i => i.DunsName).Select(x => new ResearchSummary()
                        {
                            EntityName = x.DunsName,
                            DUNSNumber = x.DunsNumber,
                            GISID = "",
                            Country = x.DunsLocation,
                            Role = "CER" + rs.Role,
                            //   WorksheetNo = rs.WorksheetNo.Split('.')[0],
                            WorksheetNo = rs.Role.Contains("Additional Research Unit") ? rs.WorksheetNo : rs.WorksheetNo.Split('.')[0],
                            EntityWithoutLegalExt = ConflictService.DropLegalExtension(x.DunsName, rs.Type),
                            Rework = rs.Rework,
                            IsClientSide = rs.IsClientSide,
                            IsFinscanResearch = rs.IsFinscanResearch,
                            IsCERResearch = rs.IsCERResearch,
                            IsCRRResearch = rs.IsCRRResearch,
                            IsSPLResearch = rs.IsSPLResearch,
                            IsGISResearch = rs.IsGISResearch,
                            PerformResearch = rs.PerformResearch
                        }));
                    }

                    string MercurySearch = string.Empty;
                    if (!string.IsNullOrEmpty(rs.DUNSNumber))
                        MercurySearch = string.Join(", ", keywordsList) + ", " + rs.DUNSNumber;
                    else
                        MercurySearch = string.Join(", ", keywordsList);

                    var uniquecerexactMatchWithDUNS = cerexactMatchWithDUNS.DistinctBy(d => new { d.DunsName, d.DunsNumber, d.Client, d.UltimateDunsNumber });

                    //Need this as we have DUNSLocation coming up as Washington, United States and United States
                    var CERRemoveDupesResult = uniquecerexactMatchWithDUNS
                        .GroupBy(s => new { s.DunsNumber, s.UltimateDunsNumber })
                        .Select(group => group.OrderBy(x => x.EngagementPartner).First())
                        .Select(order => new MercuryEntity_SpreadSheet()
                        {
                            DunsNumber = order.DunsNumber,
                            Client = order.Client,
                            DunsName = order.DunsName,
                            DunsLocation = order.DunsLocation,
                            GCSP = order.GCSP,
                            EngagementPartner = order.EngagementPartner,
                            Account = order.Account,
                            UltimateDunsNumber = order.UltimateDunsNumber,
                            AccountChannel = order.AccountChannel

                        })
                        .ToList();

                    bool multipleGUPStop = false;

                    var multipleGUP = CERRemoveDupesResult.Select(s => s.UltimateDunsNumber).Distinct();
                    List<ResearchSummary> matchedGUPWithRS = new List<ResearchSummary>();
                    matchedGUPWithRS.AddRange(from x in crs
                                              where !!multipleGUP.ToList().Any(y => y.Contains(x.DUNSNumber)
                                                      && !string.IsNullOrEmpty(x.DUNSNumber))
                                              select x);

                    if (multipleGUP.ToList().Count > 1 && matchedGUPWithRS.Count > 0)
                        multipleGUPStop = true;

                    if (multipleGUPStop)
                        crs.RemoveAll(i => i.WorksheetNo.Contains(rs.WorksheetNo + ".C")); //does not make sense but rules say so
                    else
                    {
                        List<string> researchedGUP = CERRemoveDupesResult.ToList().DistinctBy(i => i.UltimateDunsNumber).ToList().
                                    Select(i => i.UltimateDunsNumber).ToList();
                        if (researchedGUP.Count > 0)
                        {
                            foreach (var rGUP in researchedGUP)
                            {

                                if (crs.Where(i => !i.DUNSNumber.Contains(rGUP)).ToList().Count > 0)
                                {
                                    var tempCERGUP = conflictService.GetCERGUPInfo(rGUP, rs);
                                    conflictService.RemoveDupes(crs, tempCERGUP);
                                    MercuryExtraResearch.AddRange(tempCERGUP);
                                }

                            }
                        }
                    }

                    if (CERRemoveDupesResult.ToList().Count == 1 && (string.IsNullOrEmpty(rs.DUNSNumber) || string.IsNullOrEmpty(rs.MDMID)))
                    {
                        bool isDUNSchanged = false;
                        bool isGISIDchanged = false;
                        bool isMDMIDchanged = false;
                        foreach (MercuryEntity_SpreadSheet unitGrid_GISEntity in CERRemoveDupesResult)
                        {
                            List<ResearchSummary> CERGUPTemp = new List<ResearchSummary>();
                            //As per the story, if we are missing any IDs in AU unit grid then back fill, in case of CER we do not have country
                            //if (unitGrid_GISEntity. == rs.Country)
                            //{
                            if (string.IsNullOrEmpty(rs.DUNSNumber) && !string.IsNullOrEmpty(unitGrid_GISEntity.DunsNumber))
                            {
                                rs.DUNSNumber = unitGrid_GISEntity.DunsNumber;
                                CERGUPTemp.AddRange(conflictService.GetCERGUPInfo(rs.DUNSNumber, rs));

                                conflictService.RemoveDupes(crs, CERGUPTemp);
                                MercuryExtraResearch.AddRange(CERGUPTemp);

                                isDUNSchanged = true;
                            }
                            if (string.IsNullOrEmpty(rs.MDMID) && !string.IsNullOrEmpty(unitGrid_GISEntity.ClientID))
                            {
                                if (unitGrid_GISEntity.ClientID.ToString() != "0")
                                {
                                    if (unitGrid_GISEntity.ClientID.ToString().StartsWith('1'))
                                    {
                                        rs.MDMID = unitGrid_GISEntity.ClientID.ToString();
                                        isMDMIDchanged = true;
                                    }
                                }
                            }
                            //}

                            if (isGISIDchanged || isDUNSchanged || isMDMIDchanged)
                            {
                                rs.Type = GetSubjectType(rs);
                                goto GIS_REWORK;
                            }
                        }
                    }

                    if (cerexactMatchWithDUNS.Count == 0)
                    {
                        iPresentRow++;

                        string EngagementOpenDateFrom = DateTime.Now.Date.AddYears(-5).ToString("dd MMMM yyyy");
                        if (IsUKI)
                            EngagementOpenDateFrom = DateTime.Now.Date.AddYears(-7).ToString("dd MMMM yyyy");

                        worksheet.Cells[$"A{iPresentRow}"].Value = MercurySearch;
                        worksheet.Cells[$"B{iPresentRow}"].Value = DateTime.UtcNow.ToString("dd MMMM yyyy");
                        worksheet.Cells[$"C{iPresentRow}"].Value = EngagementOpenDateFrom;
                        worksheet.Cells[$"D{iPresentRow}"].Value = DateTime.UtcNow.ToString("dd MMMM yyyy");
                        if (mdata.EntityMatchCount < 1 || mdata.EntityMatchCount > 1)
                        {
                            worksheet.Cells[$"E{iPresentRow}:O{iPresentRow}"].Merge = true;
                            worksheet.Cells[$"E{iPresentRow}"].Value = "No Results identified in ​CER with the keyword search string after filtering by the country and UID and applying fuzzy match threshold. Please perform web searches.";
                        }

                        var range = worksheet.Cells[$"A{iPresentRow}:O{iPresentRow}"];
                        range.SetBackgroundColor(brightTeal);
                        range.SetBorders(ExcelBorderStyle.Thin, ExcelBorderStyle.Thin);

                        noRecordPerCER = true;
                        if (mdata.EntityMatchCount > 1)
                            noRecordPerCER = false;

                        //if no file then delete CER GUP
                        crs.RemoveAll(i => i.WorksheetNo.Contains(rs.WorksheetNo + ".C"));
                    }
                    else
                    {
                        foreach (MercuryEntity_SpreadSheet unitGrid_CEREntity in CERRemoveDupesResult)
                        {
                            string auditRelationship = GetAuditRelationship(cerexactMatchWithDUNS, IsUKI,
                                            out List<MercuryEntity_SpreadSheet> cerExactMatchWithDUNSLast3years);
                            string serviceLines = string.Join(", ",
                                                    GetServiceLines(cerExactMatchWithDUNSLast3years));

                            /* For now, this must be kept commented out, as per discussion with Vishnu about bug 1020861.
                            firstCEREntityAuditRelationship = string.IsNullOrEmpty(firstCEREntityAuditRelationship) 
                                                                ? auditRelationship : firstCEREntityAuditRelationship;
                            firstCEREntityServiceLines = string.IsNullOrEmpty(firstCEREntityServiceLines) 
                                                                ? serviceLines : firstCEREntityServiceLines;
                            */
                            string EngagementOpenDateFrom = DateTime.Now.Date.AddYears(-5).ToString("dd MMMM yyyy");
                            if (IsUKI)
                                EngagementOpenDateFrom = DateTime.Now.Date.AddYears(-7).ToString("dd MMMM yyyy");

                            iPresentRow++;
                            worksheet.Cells[$"A{iPresentRow}"].Value = MercurySearch;
                            worksheet.Cells[$"A{iPresentRow}"].Style.WrapText = true;
                            worksheet.Cells[$"B{iPresentRow}"].Value = DateTime.UtcNow.ToString("dd MMMM yyyy");
                            worksheet.Cells[$"C{iPresentRow}"].Value = EngagementOpenDateFrom;
                            worksheet.Cells[$"D{iPresentRow}"].Value = DateTime.UtcNow.ToString("dd MMMM yyyy");  //TEJAL : TODO may be -1 day as data will be stale
                            worksheet.Cells[$"E{iPresentRow}"].Value = string.IsNullOrEmpty(unitGrid_CEREntity.Client) ? "-" : unitGrid_CEREntity.Client;
                            worksheet.Cells[$"E{iPresentRow}"].Style.WrapText = true;
                            worksheet.Cells[$"F{iPresentRow}"].Value = string.IsNullOrEmpty(unitGrid_CEREntity.DunsName) ? "-" : unitGrid_CEREntity.DunsName;
                            worksheet.Cells[$"F{iPresentRow}"].Style.WrapText = true;
                            worksheet.Cells[$"G{iPresentRow}"].Value = string.IsNullOrEmpty(unitGrid_CEREntity.DunsLocation) ? "-" : unitGrid_CEREntity.DunsLocation;
                            worksheet.Cells[$"G{iPresentRow}"].Style.WrapText = true;
                            worksheet.Cells[$"H{iPresentRow}"].Value = string.IsNullOrEmpty(unitGrid_CEREntity.DunsNumber) ? "-" : unitGrid_CEREntity.DunsNumber;
                            worksheet.Cells[$"I{iPresentRow}"].Value = string.IsNullOrEmpty(unitGrid_CEREntity.GCSP) ? "-" : unitGrid_CEREntity.GCSP;
                            worksheet.Cells[$"I{iPresentRow}"].Style.WrapText = true;
                            worksheet.Cells[$"J{iPresentRow}"].Value = string.IsNullOrEmpty(unitGrid_CEREntity.EngagementPartner) ? "-" : unitGrid_CEREntity.EngagementPartner;
                            worksheet.Cells[$"J{iPresentRow}"].Style.WrapText = true;
                            worksheet.Cells[$"K{iPresentRow}"].Value = string.IsNullOrEmpty(unitGrid_CEREntity.Account) ? "-" : unitGrid_CEREntity.Account;
                            worksheet.Cells[$"K{iPresentRow}"].Style.WrapText = true;
                            worksheet.Cells[$"L{iPresentRow}"].Value = string.IsNullOrEmpty(unitGrid_CEREntity.UltimateDunsNumber) ? "-" : unitGrid_CEREntity.UltimateDunsNumber;
                            worksheet.Cells[$"M{iPresentRow}"].Value = string.IsNullOrEmpty(unitGrid_CEREntity.AccountChannel) ? "-" : unitGrid_CEREntity.AccountChannel;
                            worksheet.Cells[$"N{iPresentRow}"].Value = string.IsNullOrEmpty(auditRelationship) ? "-" : auditRelationship;
                            worksheet.Cells[$"N{iPresentRow}"].Style.WrapText = true;
                            worksheet.Cells[$"O{iPresentRow}"].Value = string.IsNullOrEmpty(serviceLines) ? "-" : serviceLines;
                            worksheet.Cells[$"O{iPresentRow}"].Style.WrapText = true;
                            var range = worksheet.Cells[$"A{iPresentRow}:O{iPresentRow}"];
                            range.SetBackgroundColor(brightTeal);
                            range.SetBorders(ExcelBorderStyle.Thin, ExcelBorderStyle.Thin);
                        }
                    }
                    if (CERRemoveDupesResult.ToList().Count > 0)
                    {
                        if (noRecordPerGIS)
                        {
                            rs.ClientRelationshipSummary.Duns = string.IsNullOrEmpty(rs.DUNSNumber) ? "NA" : rs.DUNSNumber;
                            rs.ClientRelationshipSummary.GCSP = string.IsNullOrEmpty(cerexactMatchWithDUNS.FirstOrDefault()?.GCSP) ? "NA" : cerexactMatchWithDUNS.FirstOrDefault()?.GCSP;
                            rs.ClientRelationshipSummary.LAP = "NA";
                            rs.ClientRelationshipSummary.Restrictions = "NA";
                            rs.ClientRelationshipSummary.PIE = "NA";
                            rs.ClientRelationshipSummary.PIEAffiliate = "NA";
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(rs.ClientRelationshipSummary.Duns) || rs.ClientRelationshipSummary.Duns == "NA")
                                rs.ClientRelationshipSummary.Duns = string.IsNullOrEmpty(rs.DUNSNumber) ? "NA" : rs.DUNSNumber;
                            if (string.IsNullOrEmpty(rs.ClientRelationshipSummary.GCSP) || rs.ClientRelationshipSummary.GCSP == "NA")
                                rs.ClientRelationshipSummary.GCSP = string.IsNullOrEmpty(CERRemoveDupesResult.FirstOrDefault()?.GCSP) ? "NA" : CERRemoveDupesResult.FirstOrDefault()?.GCSP;
                        }
                    }
                    if (mdata.EntityMatchCount == 1 && CERRemoveDupesResult.Count == 0)
                    {
                        string recordStatus = noRecordPerCER ? $" {CAUConstants.CER_NO_RECORD_WITH_UID_1}" : "";
                        rs.ClientRelationshipSummary.CERDesc = $"{worksheet.Cells[$"N{iPresentRow}"].Value} {recordStatus}(<{worksheet.Cells[$"O{iPresentRow}"].Value}>)";
                    }
                    else if (mdata.EntityMatchCount > 1)
                    {
                        rs.ClientRelationshipSummary.CERNotes = CAUConstants.CER_MULTIPLE_CLOSE_MATCHES;
                        rs.ClientRelationshipSummary.CERDesc = $"Audit Client / Non-Audit Client / {CAUConstants.CER_NO_RECORD_WITH_UID_1}";
                        rs.NoEYRelationshipWithCounterparties = true;
                    }
                    else if (CERRemoveDupesResult.Count == 1)
                    {
                        string recordStatus = noRecordPerCER ? $" {CAUConstants.CER_NO_RECORD_WITH_UID_1}" : "";
                        rs.ClientRelationshipSummary.CERDesc = $"{worksheet.Cells[$"N{iPresentRow}"].Value} (As per CER){recordStatus}(<{worksheet.Cells[$"O{iPresentRow}"].Value}>)";
                    }
                    if (multipleGUPStop)
                    {
                        string recordStatus = noRecordPerCER ? $" {CAUConstants.CER_NO_RECORD_WITH_UID_1}" : "";
                        rs.ClientRelationshipSummary.CERDesc = $"{worksheet.Cells[$"N{iPresentRow}"].Value} (As per CER){recordStatus}(<{worksheet.Cells[$"O{iPresentRow}"].Value}>)";
                        rs.ClientRelationshipSummary.CERNotes = "MULTIPLE GUPS HAVE BEEN IDENTIFIED IN CER FOR WHICH AU HAS TAKEN NO ACTION. PLEASE ACTION ACCORDINGLY.";
                    }
                    if (noRecordPerCER)
                    {
                        rs.ClientRelationshipSummary.CERDesc = $" {CAUConstants.CER_NO_RECORD_WITH_UID_1}";
                    }

                    rs.Mercury = "C";
                }
                catch (Exception ex)
                {
                    rs.Mercury = "F";
                    rs.ClientRelationshipSummary.SearchDesc = "Please perform CER search manually";
                    LoggerInfo.LogException(ex);
                }
            #endregion CER Grid
            SKIP_CER_GRID:
                _resultDetailsSectionsStartingRow = iPresentRow + 2;

                string summaryMessage = "";

                #region SPL Grid
                // SPL Grid: 4th in processing order; 4th section vertically
                Console.WriteLine("  4. SPL Grid...");

                SplSummaryGrid splSummaryGrid = new(worksheet, ++iPresentRow);
                iPresentRow = splSummaryGrid.WriteTitleAndHeaders();

                try
                {
                    if (rs.IsSPLResearch)
                    {
                        summaryMessage = GetSPLSummaryMessage(sPLEntity, keywordsList);
                        rs.SPL = "C";
                    }
                    iPresentRow = WriteSPLSummaryGrid(worksheet, ++iPresentRow, summaryMessage);
                }
                catch (Exception ex)
                {
                    rs.SPL = "F";
                    LoggerInfo.LogException(ex);
                }
            #endregion SPL Grid
            SKIP_SPL_GRID:
                _resultDetailsSectionsStartingRow = iPresentRow + 2;

                #region FinScan Grid/Result Details                
                // FinScan: 5th in processing order
                //   FinScan Grid: 5th section vertically
                //   FinScan Result Details: 10th section vertically
                Console.WriteLine("  5. FinScan Grid/Result Details...");

                FinScanOperations.WriteFinScanSummaryGridTitle(worksheet, ++iPresentRow, CAUConstants.MSG_SECTION_FINSCAN);

                if (!rs.IsFinscanResearch)  // User Story #1012887: Skip FinScan search for Main Clients
                {
                    iPresentRow = FinScanOperations.WriteFinScanSummaryGridBodyWithSingleMessage(
                        worksheet, ++iPresentRow, FinScanConstants.MSG_EY_CLIENTS_DISMISS_FINSCAN_SEARCH);
                    _resultDetailsSectionsStartingRow = iPresentRow + 2;

                    int finScanResultDetailsSectionRow = ResultDetailsSectionRow(sectionNumber: RESULT_DETAILS_SECTION_NUM_FINSCAN);
                    WriteTargetSectionHeader(worksheet, finScanResultDetailsSectionRow, CAUConstants.MSG_SECTION_FINSCAN);
                    WriteHyperlink(worksheet, "E4", CAUConstants.MSG_SECTION_FINSCAN, $"A{finScanResultDetailsSectionRow}");

                    rs.Sanctions = false;
                    rs.FinScanListProfileReportFilePath = string.Empty;
                    rs.FinScanMatchReportFilePath = string.Empty;

                    goto SKIP_FINSCAN_SEARCH;
                }

                try
                {
                    lsUnitGrid_FinscanEntity = FinScanOperations.ProcessFinScanCheckForMultipleKeywords
                        (rs, keywordsListForFinScanSearch,
                         FinScanSearchMatchFilter.ByRankScore, Program.FinScanMatchesThreshold,
                         sFilePath, additionalInfoOnError);

                    iPresentRow = FinScanOperations.WriteFinScanSummaryGridBodyWithData(worksheet, ++iPresentRow, lsUnitGrid_FinscanEntity);
                    _resultDetailsSectionsStartingRow = iPresentRow + 2;

                    int finScanResultDetailsSectionRow = ResultDetailsSectionRow(sectionNumber: RESULT_DETAILS_SECTION_NUM_FINSCAN);
                    WriteTargetSectionHeader(worksheet, finScanResultDetailsSectionRow, CAUConstants.MSG_SECTION_FINSCAN);
                    WriteHyperlink(worksheet, "E4", CAUConstants.MSG_SECTION_FINSCAN, $"A{finScanResultDetailsSectionRow}");
                    FinScanOperations.WriteFinScanResultsSection(worksheet, finScanResultDetailsSectionRow, lsUnitGrid_FinscanEntity);

                    if (lsUnitGrid_FinscanEntity.IsError())
                    {
                        throw new Exception(FinScanConstants.MSG_SEARCH_API_ERROR);
                    }

                    rs.Sanctions = FinScanOperations.IsSanctioned(lsUnitGrid_FinscanEntity);
                    FinScanOperations.SetFinScanAttachmentsFilePaths(rs, lsUnitGrid_FinscanEntity);

                    var finScanFileEmbeddings = FinScanOperations.ConvertToListFileEmbedding(
                            lsUnitGrid_FinscanEntity, rs.WorksheetNo,
                            finScanResultDetailsSectionRow);
                    if (!finScanFileEmbeddings.IsNullOrEmpty())
                    {
                        listFileEmbeddings.AddRange(finScanFileEmbeddings);
                    }

                    iPresentRow++;
                    rs.Finscan = "C";
                }
                catch (Exception ex)
                {
                    rs.Finscan = "F";
                    LoggerInfo.LogException(ex);
                }
            #endregion FinScan Grid/Result Details
            SKIP_FINSCAN_SEARCH:
                // For FinScan, _resultDetailsSectionsStartingRow is 
                // already being properly incremented inside the above section.

                #region SPL Result Details                
                // SPL Result Details: 6th in processing order; 9th section vertically
                Console.WriteLine("  6. SPL Result Details...");

                iPresentRow = ResultDetailsSectionRow(sectionNumber: RESULT_DETAILS_SECTION_NUM_SPL);
                WriteTargetSectionHeader(worksheet, iPresentRow, CAUConstants.MSG_SECTION_SPL);
                WriteHyperlink(worksheet, "D4", CAUConstants.MSG_SECTION_SPL, $"A{iPresentRow}");

                if (rs.IsSPLResearch)
                {
                    try
                    {
                        iPresentRow += 2;
                        worksheet.Cells[$"A{iPresentRow}"].Style.Font.Size = 8;
                        foreach (var keyword in keywordsList)
                        {
                            DateTime localTime = DateTime.Now;
                            TimeZoneInfo istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                            DateTime istTime = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, istTimeZone);

                            worksheet.Cells[$"A{iPresentRow}"].Value = sPLEntity.vesionDetails;
                            worksheet.Cells[$"A{iPresentRow}"].Style.WrapText = true;
                            worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            iPresentRow++;
                            worksheet.Cells[$"A{iPresentRow}"].Value = istTime.ToString("yyyy-MM-dd HH:mm:ss");
                            worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            iPresentRow++;
                            worksheet.Cells[$"A{iPresentRow}"].Value = "Search String:" + keyword;
                            worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            iPresentRow++;
                            worksheet.Cells[$"A{iPresentRow}"].Value = "Search Type: Keyword";
                            worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            iPresentRow++;

                            if (sPLEntity.splLists != null && sPLEntity.commercialSensitivitiesLists != null)
                            {
                                var matchingSPLList = sPLEntity.splLists.Where(item => item.Entity.Contains(keyword, StringComparison.OrdinalIgnoreCase) || item.GUP.Contains(keyword, StringComparison.OrdinalIgnoreCase) || item.SPLInstructions.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
                                var matchingCSLList = sPLEntity.commercialSensitivitiesLists.Where(item => item.Entity.Contains(keyword, StringComparison.OrdinalIgnoreCase) || item.SPLInstructions.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();

                                if (matchingSPLList.Any() || matchingCSLList.Any())
                                {
                                    worksheet.Cells[$"A{iPresentRow}"].Value = "Search Result:";
                                    iPresentRow++;
                                    SetCellBackgroundColor(worksheet.Cells[$"A{iPresentRow}:P{iPresentRow}"], darkGray);
                                    worksheet.Cells[$"A{iPresentRow}"].Value = "S.NO";
                                    worksheet.Cells[$"B{iPresentRow}"].Value = "Area";
                                    worksheet.Cells[$"C{iPresentRow}"].Value = "Region";
                                    worksheet.Cells[$"D{iPresentRow}"].Value = "Category";
                                    worksheet.Cells[$"E{iPresentRow}"].Value = "Entity";
                                    worksheet.Cells[$"F{iPresentRow}"].Value = "SPL Instructions";
                                    worksheet.Cells[$"G{iPresentRow}"].Value = "GUP";
                                    worksheet.Cells[$"H{iPresentRow}"].Value = "Attachment";
                                    worksheet.Cells[$"I{iPresentRow}"].Value = "Contact Person";
                                    worksheet.Cells[$"J{iPresentRow}"].Value = "Onshore";
                                    worksheet.Cells[$"K{iPresentRow}"].Value = "Date Of Entry";
                                    worksheet.Cells[$"L{iPresentRow}"].Value = "Tentative Date Of Expiry";
                                    worksheet.Cells[$"M{iPresentRow}"].Value = "Validity Status";
                                    worksheet.Cells[$"N{iPresentRow}"].Value = "Last Follow Up Date";
                                    worksheet.Cells[$"O{iPresentRow}"].Value = "Follow UP Comments";
                                    worksheet.Cells[$"P{iPresentRow}"].Value = "Requestor";
                                    worksheet.Cells[$"A{iPresentRow}:P{iPresentRow}"].AutoFitColumns(21, 35);
                                    iPresentRow++;
                                    foreach (var item in matchingSPLList)
                                    {
                                        worksheet.Cells[$"A{iPresentRow}"].Value = item.SNo;
                                        worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                        worksheet.Cells[$"B{iPresentRow}"].Value = item.Area;
                                        worksheet.Cells[$"C{iPresentRow}"].Value = item.Region;
                                        worksheet.Cells[$"D{iPresentRow}"].Value = item.Category;
                                        worksheet.Cells[$"E{iPresentRow}"].Value = item.Entity;
                                        worksheet.Cells[$"F{iPresentRow}"].Value = item.SPLInstructions;
                                        worksheet.Cells[$"F{iPresentRow}"].Style.WrapText = true;
                                        worksheet.Cells[$"G{iPresentRow}"].Value = item.GUP;
                                        worksheet.Cells[$"H{iPresentRow}"].Value = item.Attachment;
                                        worksheet.Cells[$"I{iPresentRow}"].Value = item.ContactPerson;
                                        worksheet.Cells[$"J{iPresentRow}"].Value = item.Onshore;
                                        if (DateTime.TryParse(item.DateOfEntry, out DateTime dateValue))
                                        {
                                            worksheet.Cells[$"K{iPresentRow}"].Value = dateValue.ToShortDateString();
                                        }
                                        else
                                        {
                                            worksheet.Cells[$"K{iPresentRow}"].Value = item.DateOfEntry;
                                        }
                                        if (DateTime.TryParse(item.TentativeDateOfExpiry, out DateTime dateValuetenative))
                                        {
                                            worksheet.Cells[$"L{iPresentRow}"].Value = dateValuetenative.ToShortDateString();
                                        }
                                        else
                                        {
                                            worksheet.Cells[$"L{iPresentRow}"].Value = item.TentativeDateOfExpiry;
                                        }
                                        worksheet.Cells[$"M{iPresentRow}"].Value = item.ValidityStatus;
                                        if (DateTime.TryParse(item.LastFollowUpDate, out DateTime dateValuelast))
                                        {
                                            worksheet.Cells[$"N{iPresentRow}"].Value = dateValuelast.ToShortDateString();
                                        }
                                        else
                                        {
                                            worksheet.Cells[$"N{iPresentRow}"].Value = item.LastFollowUpDate;
                                        }
                                        worksheet.Cells[$"O{iPresentRow}"].Value = item.FollowUpComments;
                                        worksheet.Cells[$"P{iPresentRow}"].Value = item.Requestor;
                                        worksheet.Cells[$"A{iPresentRow}:P{iPresentRow}"].AutoFitColumns(21, 35);
                                        SetCellBackgroundColor(worksheet.Cells[$"A{iPresentRow}:P{iPresentRow}"], brightTeal);
                                        iPresentRow++;
                                    }
                                    foreach (var item in matchingCSLList)
                                    {
                                        worksheet.Cells[$"A{iPresentRow}"].Value = item.SNo;
                                        worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                        worksheet.Cells[$"B{iPresentRow}"].Value = item.Area;
                                        worksheet.Cells[$"C{iPresentRow}"].Value = item.Region;
                                        worksheet.Cells[$"D{iPresentRow}"].Value = item.Category;
                                        worksheet.Cells[$"E{iPresentRow}"].Value = item.Entity;
                                        worksheet.Cells[$"F{iPresentRow}"].Value = item.SPLInstructions;
                                        worksheet.Cells[$"F{iPresentRow}"].Style.WrapText = true;
                                        worksheet.Cells[$"G{iPresentRow}"].Value = item.Attachment;
                                        worksheet.Cells[$"H{iPresentRow}"].Value = item.ContactPerson;
                                        worksheet.Cells[$"I{iPresentRow}"].Value = item.Onshore;
                                        if (DateTime.TryParse(item.DateOfEntry, out DateTime dateValue))
                                        {
                                            worksheet.Cells[$"J{iPresentRow}"].Value = dateValue.ToShortDateString();
                                        }
                                        else
                                        {
                                            worksheet.Cells[$"J{iPresentRow}"].Value = item.DateOfEntry;
                                        }
                                        if (DateTime.TryParse(item.TentativeDateOfExpiry, out DateTime dateValuetenative))
                                        {
                                            worksheet.Cells[$"K{iPresentRow}"].Value = dateValuetenative.ToShortDateString();
                                        }
                                        else
                                        {
                                            worksheet.Cells[$"K{iPresentRow}"].Value = item.TentativeDateOfExpiry;
                                        }
                                        worksheet.Cells[$"L{iPresentRow}"].Value = item.ValidityStatus;
                                        if (DateTime.TryParse(item.LastFollowUpDate, out DateTime dateValuelast))
                                        {
                                            worksheet.Cells[$"M{iPresentRow}"].Value = dateValuelast.ToShortDateString();
                                        }
                                        else
                                        {
                                            worksheet.Cells[$"M{iPresentRow}"].Value = item.LastFollowUpDate;
                                        }
                                        worksheet.Cells[$"N{iPresentRow}"].Value = item.FollowUpComments;
                                        worksheet.Cells[$"O{iPresentRow}"].Value = item.Requestor;
                                        worksheet.Cells[$"A{iPresentRow}:O{iPresentRow}"].AutoFitColumns(21, 35);
                                        SetCellBackgroundColor(worksheet.Cells[$"A{iPresentRow}:O{iPresentRow}"], brightTeal);
                                        iPresentRow++;
                                    }
                                }
                                else
                                {
                                    worksheet.Cells[$"A{iPresentRow}"].Value = $"No Results identified with the {keyword} search string.";
                                    worksheet.Cells[$"A{iPresentRow}"].Style.WrapText = true;
                                    worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                    iPresentRow++;
                                }
                                rs.SPL = "C";
                            }
                            else
                            {
                                worksheet.Cells[$"A{iPresentRow}"].Value = "Search Result:";
                                worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                iPresentRow++;
                                worksheet.Cells[$"A{iPresentRow}"].Value = $"Unable to search SPL to generate results. Please investigate further.";
                                worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                                rs.SPL = "F";
                            }
                            iPresentRow++;
                        }

                    }
                    catch (Exception ex)
                    {
                        rs.SPL = "F";
                        LoggerInfo.LogException(ex);
                    }
                }
                #endregion SPL Result Details

                #region CRR Result Details
                // CRR Result Details: 7th in processing order; 8th section vertically
                Console.WriteLine("  7. CRR Result Details...");

                iPresentRow = ResultDetailsSectionRow(sectionNumber: RESULT_DETAILS_SECTION_NUM_CRR);
                WriteTargetSectionHeader(worksheet, iPresentRow, CAUConstants.MSG_SECTION_CRR);
                WriteHyperlink(worksheet, "C4", CAUConstants.MSG_SECTION_CRR, $"A{iPresentRow}");

                if (rs.IsCRRResearch)
                {
                    try
                    {
                        bool IsGUPLaw = false;
                        if (IsCRRGUP && (rs.Role.Contains("GUP of") || rs.Role.Contains("GIS GUP") || rs.Role.Contains("CER GUP")))
                        {
                            IsGUPLaw = true;
                        }
                        iPresentRow++;
                        worksheet.Cells[$"A{iPresentRow}"].Style.Font.Size = 8;
                        DateTime localTime = DateTime.Now;
                        TimeZoneInfo istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                        DateTime istTime = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, istTimeZone);
                        var entityName = rs.EntityName.Trim();
                        var legalExtensions = ConflictAULookUp.GetLeagalExtensions(configure);
#pragma warning disable CA1861 // Avoid constant arrays as arguments
                        string[] entityNameWithoutLegal = entityName.Split(new char[] { ' ', '-', ';', ':', '!' }, StringSplitOptions.RemoveEmptyEntries);
#pragma warning restore CA1861 // Avoid constant arrays as arguments
                        string entityNameSuffix = entityNameWithoutLegal.LastOrDefault();
                        bool isLastWordLegalExtension = legalExtensions.Any(i => string.Equals(i.Extensions, entityNameSuffix, StringComparison.OrdinalIgnoreCase));
                        List<string> strPermutations = new List<string> { entityName };
                        if (isLastWordLegalExtension)
                        {
                            var strWithSpecialChar = entityName.StrLeftBack(entityNameSuffix).Trim();
                            strPermutations.Add(strWithSpecialChar);
                            var sanitizedBaseName = new string(entityName.StrLeftBack(entityNameSuffix)
                                                   .Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                                                   .ToArray())
                                                   .Trim();
                            strPermutations.Add(sanitizedBaseName);
                        }
                        strPermutations = strPermutations.Distinct().ToList();
                        int entityCount = 0;
                        List<long> conflictCheckIDs = new List<long>();
                        var allKeyword = string.Join("|", keywordsList);
                        var crrResult = new CRRResults();
                        bool isNoResult = false;
                        var strCRR = "";
                        var pathCRR = Path.Combine(destinationPath, rs.WorksheetNo, "CRR");
                        var pathCRRGup = Path.Combine(destinationPath, rs.WorksheetNo, "CRRGup");
                        if (keywordsList.Count > 10)
                        {
                            if (IsGUPLaw)
                            {
                                keywordsList.Add(rs.DUNSNumber);
                            }
                            const int batchSize = 10;
                            isNoResult = true;
                            int batches = (int)Math.Ceiling((double)keywordsList.Count / batchSize);
                            for (int i = 0; i < batches; i++)
                            {
                                conflictCheckIDs.Clear();
                                entityCount = 0;
                                crrResult = new CRRResults();
                                var batchKeywords = keywordsList.Skip(i * batchSize).Take(batchSize).ToList();
                                var batchAllKeyword = string.Join("|", batchKeywords);
                                worksheet.Cells[$"A{iPresentRow}"].Value = istTime.ToString("yyyy-MM-dd HH:mm:ss");
                                worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                iPresentRow++;
                                worksheet.Cells[$"A{iPresentRow}"].Value = "Search String:" + batchAllKeyword;
                                worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                iPresentRow++;
                                worksheet.Cells[$"A{iPresentRow}"].Value = "Search Type: Keyword";
                                worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                iPresentRow++;
                                worksheet.Cells[$"A{iPresentRow}"].Value = "Search Result:";
                                worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                crrResult = conflictService.GetCRRData(batchAllKeyword, pathCRR, false);
                                iPresentRow += 2;
                                if (!string.IsNullOrEmpty(crrResult.fileName) && crrResult.fileName != "0")
                                {
                                    isNoResult = false;
                                    var embedRow = $"A{iPresentRow}";
                                    var filetoEmbed = new FileEmbedding(crrResult.fileName, rs.WorksheetNo, embedRow);
                                    listFileEmbeddings.Add(filetoEmbed);
                                    iPresentRow += 6;
                                }
                                foreach (DataTable table in crrResult.crrDataset.Tables)
                                {
                                    if (table.Columns.Contains("Entity Name") && table.Columns.Contains("Check ID"))
                                    {
                                        List<long> matchingIDs = new List<long>();
                                        foreach (var entity in strPermutations)
                                        {
                                            matchingIDs = table.AsEnumerable()
                                                 .Where(row => row.Field<string>("Entity Name") != null && row.Field<string>("Entity Name").ToLower().Contains(entity.ToLower()))
                                                 .Select(row =>
                                                 {
                                                     if (long.TryParse(row["Check ID"].ToString(), out long conflictCheckID))
                                                         return conflictCheckID;
                                                     return -1;
                                                 }).ToList();
                                            conflictCheckIDs.AddRange(matchingIDs);
                                        }
                                        entityCount += conflictCheckIDs.Distinct().Count(id => id != -1);
                                    }
                                }

                                var strResult = (entityCount == 0)
                                    ? $"CRR > {batchAllKeyword}: No relevant hits found"
                                    : (entityCount == 1 && conflictCheckIDs.Contains(conflictCheck.ConflictCheckID))
                                        ? $"CRR > {batchAllKeyword}: No relevant hits found"
                                        : (entityCount == 1)
                                            ? $"CRR > {batchAllKeyword}: Hit Identified"
                                            : $"CRR > {batchAllKeyword}: Multiple Hits Identified";
                                if (entityCount > 1)
                                {
                                    strResult = conflictCheckIDs.Any(checkID => checkID != conflictCheck.ConflictCheckID) ?
                                        $"CRR > {batchAllKeyword}: Multiple Hits Identified" :
                                        $"CRR > {batchAllKeyword}: No relevant hits found";
                                }
                                else if (crrResult.isError)
                                {
                                    strResult = $"CRR > {batchAllKeyword}: Unable to generate CRR results due to possible error. Please investigate further.";
                                }
                                if (crrResult.isError || isNoResult)
                                {
                                    var crrSearchRow = $"A{iPresentRow}:M{iPresentRow}";
                                    worksheet.Cells[$"A{iPresentRow}"].Value = crrResult.isError
                                                                                ? "Unable to generate CRR results due to possible error. Please investigate further."
                                                                                : "No results identified using the given search string";
                                    worksheet.Cells[crrSearchRow].Merge = true;
                                    worksheet.Cells[crrSearchRow].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                    SetCellBackgroundColor(worksheet.Cells[crrSearchRow], brightTeal);
                                    iPresentRow += 2;
                                }
                                strCRR += strResult + Environment.NewLine;
                                worksheet.Cells[_summaryRowReference].Value = summaryMessage = summaryMessage + Environment.NewLine + strCRR + Environment.NewLine;
                                worksheet.Cells[_summaryRowReference].EntireRow.Height = 60;
                                if (IsGUPLaw)
                                {
                                    conflictCheckIDs.Clear();
                                    entityCount = 0;
                                    strCRR = "";
                                    worksheet.Cells[$"A{iPresentRow}"].Value = istTime.ToString("yyyy-MM-dd HH:mm:ss");
                                    worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                    iPresentRow++;
                                    worksheet.Cells[$"A{iPresentRow}"].Value = "Search String:" + batchAllKeyword;
                                    worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                    iPresentRow++;
                                    worksheet.Cells[$"A{iPresentRow}"].Value = "Search Type: GUP";
                                    worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                    iPresentRow++;
                                    worksheet.Cells[$"A{iPresentRow}"].Value = "Search Result:";
                                    worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                                    var gupResult = conflictService.GetCRRData(batchAllKeyword, pathCRRGup, true);
                                    if (!string.IsNullOrEmpty(gupResult.fileName) && gupResult.fileName != "0")
                                    {
                                        isNoResult = false;
                                        var embedRow = $"A{iPresentRow}";
                                        var filetoEmbed = new FileEmbedding(gupResult.fileName, rs.WorksheetNo, embedRow);
                                        listFileEmbeddings.Add(filetoEmbed);
                                        iPresentRow += 6;
                                    }
                                    foreach (DataTable table in gupResult.crrDataset.Tables)
                                    {
                                        if (table.Columns.Contains("Entity Name") && table.Columns.Contains("Check ID"))
                                        {
                                            List<long> matchingIDs = new List<long>();
                                            foreach (var entity in strPermutations)
                                            {
                                                matchingIDs = table.AsEnumerable()
                                                     .Where(row => row.Field<string>("Entity Name") != null && row.Field<string>("Entity Name").ToLower().Contains(entity.ToLower()))
                                                     .Select(row =>
                                                     {
                                                         if (long.TryParse(row["Check ID"].ToString(), out long conflictCheckID))
                                                             return conflictCheckID;
                                                         return -1;
                                                     }).ToList();
                                                conflictCheckIDs.AddRange(matchingIDs);
                                            }
                                            entityCount += conflictCheckIDs.Distinct().Count(id => id != -1);
                                        }
                                    }
                                    strResult = (entityCount == 0)
                                  ? $"CRR > {batchAllKeyword}: No relevant hits found"
                                  : (entityCount == 1 && conflictCheckIDs.Contains(conflictCheck.ConflictCheckID))
                                      ? $"CRR > {batchAllKeyword}: No relevant hits found"
                                      : (entityCount == 1)
                                          ? $"CRR > {batchAllKeyword}: Hit Identified"
                                          : $"CRR > {batchAllKeyword}: Multiple Hits Identified";
                                    if (entityCount > 1)
                                    {
                                        strResult = conflictCheckIDs.Any(checkID => checkID != conflictCheck.ConflictCheckID) ?
                                            $"CRR > {batchAllKeyword}: Multiple Hits Identified" :
                                            $"CRR > {batchAllKeyword}: No relevant hits found";
                                    }
                                    else if (crrResult.isError)
                                    {
                                        strResult = $"CRR > {batchAllKeyword}: Unable to generate CRR results due to possible error. Please investigate further.";
                                    }
                                    if (crrResult.isError || isNoResult)
                                    {
                                        var crrSearchRow = $"A{iPresentRow}:M{iPresentRow}";
                                        worksheet.Cells[$"A{iPresentRow}"].Value = crrResult.isError
                                                                                    ? "Unable to generate CRR results due to possible error. Please investigate further."
                                                                                    : "No results identified using the given search string";
                                        worksheet.Cells[crrSearchRow].Merge = true;
                                        worksheet.Cells[crrSearchRow].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                        SetCellBackgroundColor(worksheet.Cells[crrSearchRow], brightTeal);
                                        iPresentRow += 2;
                                    }
                                    strCRR += strResult + Environment.NewLine;
                                    worksheet.Cells[_summaryRowReference].Value = summaryMessage + strCRR;
                                    worksheet.Cells[_summaryRowReference].EntireRow.Height = 50;
                                }
                            }
                        }
                        else
                        {
                            worksheet.Cells[$"A{iPresentRow}"].Value = istTime.ToString("yyyy-MM-dd HH:mm:ss");
                            worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            iPresentRow++;
                            worksheet.Cells[$"A{iPresentRow}"].Value = "Search String:" + allKeyword;
                            worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            iPresentRow++;
                            worksheet.Cells[$"A{iPresentRow}"].Value = "Search Type: Keyword";
                            worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            iPresentRow++;
                            worksheet.Cells[$"A{iPresentRow}"].Value = "Search Result:";
                            worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            crrResult = conflictService.GetCRRData(allKeyword, pathCRR, false);
                            isNoResult = string.IsNullOrWhiteSpace(crrResult.fileName) || crrResult.fileName == "0";
                            foreach (DataTable table in crrResult.crrDataset.Tables)
                            {
                                if (table.Columns.Contains("Entity Name") && table.Columns.Contains("Check ID"))
                                {
                                    List<long> matchingIDs = new List<long>();
                                    foreach (var entity in strPermutations)
                                    {
                                        matchingIDs = table.AsEnumerable()
                                             .Where(row => row.Field<string>("Entity Name") != null && row.Field<string>("Entity Name").ToLower().Contains(entity.ToLower()))
                                             .Select(row =>
                                             {
                                                 if (long.TryParse(row["Check ID"].ToString(), out long conflictCheckID))
                                                     return conflictCheckID;
                                                 return -1;
                                             }).ToList();
                                        conflictCheckIDs.AddRange(matchingIDs);
                                    }
                                    entityCount += conflictCheckIDs.Distinct().Count(id => id != -1);
                                }
                            }

                            var allKeywordsSeparatedByComma = allKeyword.Replace("|", ", ");
                            strCRR = (entityCount == 0)
                                         ? $"CRR > {allKeywordsSeparatedByComma}: No relevant hits found"
                                         : (entityCount == 1 && conflictCheckIDs.Contains(conflictCheck.ConflictCheckID))
                                             ? $"CRR > {allKeywordsSeparatedByComma}: No relevant hits found"
                                             : (entityCount == 1)
                                                 ? $"CRR > {allKeywordsSeparatedByComma}: Hit Identified"
                                                 : $"CRR > {allKeywordsSeparatedByComma}: Multiple Hits Identified";
                            if (entityCount > 1)
                            {
                                strCRR = conflictCheckIDs.Any(checkID => checkID != conflictCheck.ConflictCheckID) ?
                                    $"CRR > {allKeywordsSeparatedByComma}: Multiple Hits Identified" :
                                    $"CRR > {allKeywordsSeparatedByComma}: No relevant hits found";
                            }
                            else if (crrResult.isError)
                            {
                                strCRR = $"CRR > {allKeywordsSeparatedByComma}: Unable to generate CRR results due to possible error. Please investigate further.";
                            }
                            worksheet.Cells[_summaryRowReference].Value = summaryMessage = summaryMessage + Environment.NewLine + strCRR + Environment.NewLine;
                            worksheet.Cells[_summaryRowReference].EntireRow.Height = 40;

                            if (crrResult.isError || isNoResult)
                            {
                                iPresentRow++;
                                var crrSearchRow = $"A{iPresentRow}:M{iPresentRow}";
                                worksheet.Cells[$"A{iPresentRow}"].Value = crrResult.isError
                                                                            ? "Unable to generate CRR results due to possible error. Please investigate further."
                                                                            : "No results identified using the given search string";
                                worksheet.Cells[crrSearchRow].Merge = true;
                                worksheet.Cells[crrSearchRow].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                SetCellBackgroundColor(worksheet.Cells[crrSearchRow], brightTeal);
                            }
                            iPresentRow += 2;
                            if (Directory.Exists(pathCRR))
                            {
                                string[] excelFiles = Directory.GetFiles(pathCRR, "*.xlsx", SearchOption.AllDirectories);
                                foreach (var filePath in excelFiles)
                                {
                                    var embedRow = $"A{iPresentRow}";
                                    var filetoEmbed = new FileEmbedding(filePath, rs.WorksheetNo, embedRow);
                                    listFileEmbeddings.Add(filetoEmbed);
                                    iPresentRow += 6;
                                }
                            }
                            if (IsGUPLaw)
                            {
                                allKeyword = allKeyword + "|" + rs.DUNSNumber;
                                worksheet.Cells[$"A{iPresentRow}"].Value = istTime.ToString("yyyy-MM-dd HH:mm:ss");
                                worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                iPresentRow++;
                                worksheet.Cells[$"A{iPresentRow}"].Value = "Search String:" + allKeyword;
                                worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                iPresentRow++;
                                worksheet.Cells[$"A{iPresentRow}"].Value = "Search Type: GUP";
                                worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                iPresentRow++;
                                worksheet.Cells[$"A{iPresentRow}"].Value = "Search Result:";
                                worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                conflictCheckIDs.Clear();
                                entityCount = 0;
                                var gupResult = conflictService.GetCRRData(allKeyword, pathCRRGup, true);
                                isNoResult = string.IsNullOrWhiteSpace(gupResult.fileName) || gupResult.fileName == "0";
                                foreach (DataTable table in gupResult.crrDataset.Tables)
                                {
                                    if (table.Columns.Contains("Entity Name") && table.Columns.Contains("Check ID"))
                                    {
                                        List<long> matchingIDs = new List<long>();
                                        foreach (var entity in strPermutations)
                                        {
                                            matchingIDs = table.AsEnumerable()
                                                 .Where(row => row.Field<string>("Entity Name") != null && row.Field<string>("Entity Name").ToLower().Contains(entity.ToLower()))
                                                 .Select(row =>
                                                 {
                                                     if (long.TryParse(row["Check ID"].ToString(), out long conflictCheckID))
                                                         return conflictCheckID;
                                                     return -1;
                                                 }).ToList();
                                            conflictCheckIDs.AddRange(matchingIDs);
                                        }
                                        entityCount += conflictCheckIDs.Distinct().Count(id => id != -1);
                                    }
                                }
                                allKeywordsSeparatedByComma = allKeyword.Replace("|", ", ");
                                strCRR = (entityCount == 0)
                                       ? $"CRR > {allKeywordsSeparatedByComma}: No relevant hits found"
                                       : (entityCount == 1 && conflictCheckIDs.Contains(conflictCheck.ConflictCheckID))
                                           ? $"CRR > {allKeywordsSeparatedByComma}: No relevant hits found"
                                           : (entityCount == 1)
                                               ? $"CRR > {allKeywordsSeparatedByComma}: Hit Identified"
                                               : $"CRR > {allKeywordsSeparatedByComma}: Multiple Hits Identified";
                                if (entityCount > 1)
                                {
                                    strCRR = conflictCheckIDs.Any(checkID => checkID != conflictCheck.ConflictCheckID) ?
                                        $"CRR > {allKeywordsSeparatedByComma}: Multiple Hits Identified" :
                                        $"CRR > {allKeywordsSeparatedByComma}: No relevant hits found";
                                }
                                else if (crrResult.isError)
                                {
                                    strCRR = $"CRR > {allKeywordsSeparatedByComma}: Unable to generate CRR results due to possible error. Please investigate further.";
                                }
                                worksheet.Cells[_summaryRowReference].Value = summaryMessage + strCRR + Environment.NewLine;
                                worksheet.Cells[_summaryRowReference].EntireRow.Height = 50;
                                if (crrResult.isError || isNoResult)
                                {
                                    iPresentRow++;
                                    var crrSearchRow = $"A{iPresentRow}:M{iPresentRow}";
                                    worksheet.Cells[$"A{iPresentRow}"].Value = crrResult.isError
                                                                                ? "Unable to generate CRR results due to possible error. Please investigate further."
                                                                                : "No results identified using the given search string";
                                    worksheet.Cells[crrSearchRow].Merge = true;
                                    worksheet.Cells[crrSearchRow].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                    SetCellBackgroundColor(worksheet.Cells[crrSearchRow], brightTeal);
                                }
                                iPresentRow += 2;
                                if (Directory.Exists(pathCRRGup))
                                {
                                    string[] excelFiles = Directory.GetFiles(pathCRRGup, "*.xlsx", SearchOption.AllDirectories);
                                    foreach (var filePath in excelFiles)
                                    {
                                        var embedRow = $"A{iPresentRow}";
                                        var filetoEmbed = new FileEmbedding(filePath, rs.WorksheetNo, embedRow);
                                        listFileEmbeddings.Add(filetoEmbed);
                                        iPresentRow += 6;
                                    }
                                }
                            }
                        }
                        rs.CRR = "C";
                    }
                    catch (Exception ex)
                    {
                        rs.CRR = "F";
                        LoggerInfo.LogException(ex, "IsCRRResearch CheckID: " + conflictCheck.ConflictCheckID + " Entity:" + rs.EntityName);
                    }
                }
                #endregion CRR Result Details

                #region GIS Result Details                
                // GIS Result Details: 8th in processing order; 6th section vertically
                Console.WriteLine("  8. GIS Result Details...");

                iPresentRow = ResultDetailsSectionRow(sectionNumber: RESULT_DETAILS_SECTION_NUM_GIS);
                WriteTargetSectionHeader(worksheet, iPresentRow, CAUConstants.MSG_SECTION_GIS);
                WriteHyperlink(worksheet, "A4", CAUConstants.MSG_SECTION_GIS, $"A{iPresentRow}");

                if (rs.IsGISResearch)
                {
                    try
                    {
                        string noResultsGIS = " No Results identified in GIS with the search string.";

                        iPresentRow++;
                        worksheet.Cells[$"A{iPresentRow}"].Style.Font.Size = 8;
                        DateTime localTime = DateTime.Now;
                        TimeZoneInfo istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                        DateTime istTime = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, istTimeZone);
                        var allKeyword = string.Join("|", keywordsList);
                        worksheet.Cells[$"A{iPresentRow}"].Value = istTime.ToString("yyyy-MM-dd HH:mm:ss");
                        worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        iPresentRow++;
                        worksheet.Cells[$"A{iPresentRow}"].Value = "Search String:" + allKeyword;
                        worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        iPresentRow++;
                        worksheet.Cells[$"A{iPresentRow}"].Value = "Search Type: Keyword";
                        worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        iPresentRow++;
                        worksheet.Cells[$"A{iPresentRow}"].Value = "Search Result:" + noResultsGIS;
                        worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        var pathGIS = Path.Combine(destinationPath, rs.WorksheetNo, "GIS");
                        if (Directory.Exists(pathGIS))
                        {
                            string[] excelFiles = Directory.GetFiles(pathGIS, "*.xlsx", SearchOption.AllDirectories);
                            foreach (var filePath in excelFiles)
                            {
                                if (filePath.Contains("GE_"))
                                {
                                    worksheet.Cells[$"A{iPresentRow}"].Value = "Search Result:";
                                    iPresentRow += 2;
                                    var embedRow = $"A{iPresentRow}";
                                    var filetoEmbed = new FileEmbedding(filePath, rs.WorksheetNo, embedRow);
                                    listFileEmbeddings.Add(filetoEmbed);
                                    iPresentRow += 6;
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(rs.DUNSNumber) && isDUNSSearchHappened)
                        {
                            iPresentRow += 2;
                            worksheet.Cells[$"A{iPresentRow}"].Style.Font.Size = 8;
                            DateTime localTimeDUNS = DateTime.Now;
                            TimeZoneInfo istTimeZoneDUNS = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                            DateTime istTimeDUNS = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, istTimeZoneDUNS);
                            var allKeywordDUNS = rs.DUNSNumber;
                            worksheet.Cells[$"A{iPresentRow}"].Value = istTimeDUNS.ToString("yyyy-MM-dd HH:mm:ss");
                            worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            iPresentRow++;
                            worksheet.Cells[$"A{iPresentRow}"].Value = "Search String:" + allKeywordDUNS;
                            worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            iPresentRow++;
                            worksheet.Cells[$"A{iPresentRow}"].Value = "Search Type: DUNSNumber";
                            worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            iPresentRow++;
                            worksheet.Cells[$"A{iPresentRow}"].Value = "Search Result:" + noResultsGIS;
                            worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            if (isValidDUNSSearch)
                            {
                                var pathGISDUNS = Path.Combine(destinationPath, rs.WorksheetNo, "GIS");
                                if (Directory.Exists(pathGISDUNS))
                                {
                                    string[] excelFiles = Directory.GetFiles(pathGISDUNS, "*.xlsx", SearchOption.AllDirectories);
                                    foreach (var filePath in excelFiles)
                                    {
                                        if (filePath.Contains("GD_"))
                                        {
                                            worksheet.Cells[$"A{iPresentRow}"].Value = "Search Result:";
                                            iPresentRow += 2;
                                            var embedRow = $"A{iPresentRow}";
                                            var filetoEmbed = new FileEmbedding(filePath, rs.WorksheetNo, embedRow);
                                            listFileEmbeddings.Add(filetoEmbed);
                                            iPresentRow += 6;
                                        }
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(rs.MDMID) && isMDMIDSearchHappened)
                        {
                            iPresentRow += 2;
                            worksheet.Cells[$"A{iPresentRow}"].Style.Font.Size = 8;
                            DateTime localTimeMDM = DateTime.Now;
                            TimeZoneInfo istTimeZoneMDM = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                            DateTime istTimeMDM = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, istTimeZoneMDM);
                            var allKeywordMDM = rs.MDMID;
                            worksheet.Cells[$"A{iPresentRow}"].Value = istTimeMDM.ToString("yyyy-MM-dd HH:mm:ss");
                            worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            iPresentRow++;
                            worksheet.Cells[$"A{iPresentRow}"].Value = "Search String:" + allKeywordMDM;
                            worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            iPresentRow++;
                            worksheet.Cells[$"A{iPresentRow}"].Value = "Search Type: MDMID";
                            worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            iPresentRow++;
                            worksheet.Cells[$"A{iPresentRow}"].Value = "Search Result:" + noResultsGIS;
                            worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            if (isValidMDMIDSearch)
                            {
                                var pathGISMDM = Path.Combine(destinationPath, rs.WorksheetNo, "GIS");
                                if (Directory.Exists(pathGISMDM))
                                {
                                    string[] excelFiles = Directory.GetFiles(pathGISMDM, "*.xlsx", SearchOption.AllDirectories);
                                    foreach (var filePath in excelFiles)
                                    {
                                        if (filePath.Contains("GM_"))
                                        {
                                            worksheet.Cells[$"A{iPresentRow}"].Value = "Search Result:";
                                            iPresentRow += 2;
                                            var embedRow = $"A{iPresentRow}";
                                            var filetoEmbed = new FileEmbedding(filePath, rs.WorksheetNo, embedRow);
                                            listFileEmbeddings.Add(filetoEmbed);
                                            iPresentRow += 6;
                                        }
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(rs.GISID) && isGISIDSearchHappened)
                        {
                            iPresentRow += 2;
                            worksheet.Cells[$"A{iPresentRow}"].Style.Font.Size = 8;
                            DateTime localTimeGIS = DateTime.Now;
                            TimeZoneInfo istTimeZoneGIS = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                            DateTime istTimeGIS = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, istTimeZoneGIS);
                            var allKeywordGIS = rs.GISID;
                            worksheet.Cells[$"A{iPresentRow}"].Value = istTimeGIS.ToString("yyyy-MM-dd HH:mm:ss");
                            worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            iPresentRow++;
                            worksheet.Cells[$"A{iPresentRow}"].Value = "Search String:" + allKeywordGIS;
                            worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            iPresentRow++;
                            worksheet.Cells[$"A{iPresentRow}"].Value = "Search Type: GISID";
                            worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            iPresentRow++;
                            worksheet.Cells[$"A{iPresentRow}"].Value = "Search Result:" + noResultsGIS;
                            worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                            if (isValidGISIDSearch)
                            {
                                var pathGISGISID = Path.Combine(destinationPath, rs.WorksheetNo, "GIS");
                                if (Directory.Exists(pathGISGISID))
                                {
                                    string[] excelFiles = Directory.GetFiles(pathGISGISID, "*.xlsx", SearchOption.AllDirectories);
                                    foreach (var filePath in excelFiles)
                                    {
                                        if (filePath.Contains("GG_"))
                                        {
                                            worksheet.Cells[$"A{iPresentRow}"].Value = "Search Result:";
                                            iPresentRow += 2;
                                            var embedRow = $"A{iPresentRow}";
                                            var filetoEmbed = new FileEmbedding(filePath, rs.WorksheetNo, embedRow);
                                            listFileEmbeddings.Add(filetoEmbed);
                                            iPresentRow += 6;
                                        }
                                    }
                                }
                            }
                        }



                        rs.GIS = "C";
                    }
                    catch (Exception ex)
                    {
                        rs.GIS = "F";
                        LoggerInfo.LogException(ex);
                    }
                }
                #endregion GIS Result Details

                #region CER Result Details                    
                // CER Result Details: 9th in processing order; 7th section vertically
                Console.WriteLine("  9. CER Result Details...");

                iPresentRow = ResultDetailsSectionRow(sectionNumber: RESULT_DETAILS_SECTION_NUM_CER);
                WriteTargetSectionHeader(worksheet, iPresentRow, CAUConstants.MSG_SECTION_CER);
                WriteHyperlink(worksheet, "B4", CAUConstants.MSG_SECTION_CER, $"A{iPresentRow}");

                if (rs.IsCERResearch)
                {
                    try
                    {
                        string noResultsCER = " No Results identified in CER with the keyword search string.";
                        var pathCER = Path.Combine(destinationPath, rs.WorksheetNo, "CER");

                        string searchType = "Search Type: Keyword";

                        var allKeyword = string.Join("|", keywordsList);
                        string MercurySearch = allKeyword;
                        if (!string.IsNullOrEmpty(rs.DUNSNumber))
                        {
                            MercurySearch = allKeyword + ", " + rs.DUNSNumber;
                            searchType = "Search Type: Keyword, DUNSNumber";
                        }

                        iPresentRow++;
                        worksheet.Cells[$"A{iPresentRow}"].Style.Font.Size = 8;
                        DateTime localTime = DateTime.Now;
                        TimeZoneInfo istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                        DateTime istTime = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, istTimeZone);

                        worksheet.Cells[$"A{iPresentRow}"].Value = istTime.ToString("yyyy-MM-dd HH:mm:ss");
                        worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        iPresentRow++;
                        worksheet.Cells[$"A{iPresentRow}"].Value = "Search String:" + MercurySearch;
                        worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        iPresentRow++;
                        worksheet.Cells[$"A{iPresentRow}"].Value = searchType;
                        worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        iPresentRow++;
                        if (Directory.Exists(pathCER))
                            worksheet.Cells[$"A{iPresentRow}"].Value = "Search Result:";
                        else
                            worksheet.Cells[$"A{iPresentRow}"].Value = "Search Result:" + noResultsCER;
                        worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        iPresentRow += 2;

                        if (Directory.Exists(pathCER))
                        {
                            string[] excelFiles = Directory.GetFiles(pathCER, "*.xlsx", SearchOption.AllDirectories);
                            foreach (var filePath in excelFiles)
                            {
                                var embedRow = $"A{iPresentRow}";
                                var filetoEmbed = new FileEmbedding(filePath, rs.WorksheetNo, embedRow);
                                listFileEmbeddings.Add(filetoEmbed);
                                iPresentRow += 6;
                            }
                        }
                        rs.Mercury = "C";
                    }
                    catch (Exception ex)
                    {
                        rs.Mercury = "F";
                        LoggerInfo.LogException(ex);
                    }
                }
                #endregion CER Result Details

                #region Local & External DBs Result Details
                // Local & External DBs Result Details: 10th in processing order; 11th section vertically
                Console.WriteLine(" 10. Local & External DBs Result Details...");
                Log.Information("Starting 10. Local & External DBs Result Details...");

                iPresentRow = ResultDetailsSectionRow(sectionNumber: RESULT_DETAILS_SECTION_NUM_LOCAL_AND_EXTERNAL_DBS);
                WriteTargetSectionHeader(worksheet, iPresentRow, CAUConstants.MSG_SECTION_LOCAL_AND_EXTERNAL_DBS);
                worksheet.Cells[$"A{iPresentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                WriteHyperlink(worksheet, "F4", CAUConstants.MSG_SECTION_LOCAL_AND_EXTERNAL_DBS, $"A{iPresentRow}");
                #endregion Local & External DBs Result Details

                //***** SaveUnitGridTab_END_OF_PROCESSING:
                if (rs.ClientRelationshipSummary.CERDesc == CAUConstants.CER_NO_RECORD_WITH_UID_1 &&
                   (rs.ClientRelationshipSummary.GISDesc != null && rs.ClientRelationshipSummary.GISDesc.Contains("Entity Under Audit:") || rs.ClientRelationshipSummary.GISDesc == CAUConstants.GIS_NO_RECORD_WITH_UID))
                {
                    rs.NoEYRelationshipWithCounterparties = true;
                }

                string clientRelationshipSummary = rs.MakeClientRelationshipSummaryText();
                worksheet.Cells["B3"].Value = clientRelationshipSummary;
                worksheet.Cells["B3"].Style.Font.Size = (clientRelationshipSummary.Length >= 949) ? 7.5f : 8.0f;
                for (int col = 2; col <= 4; col++)
                {
                    worksheet.Columns[col].Width = 30.0d;
                }
                worksheet.AutoFitRowHeight(3, 'B', 'D', [('B', 'D')]);
                package.Save();

                Log.Information("Completing 10. Local & External DBs Result Details...");
            }

            if (listFileEmbeddings.Any())
            {
                try
                {
                    if (File.Exists(sFilePath))
                    {
                        Log.Information("Running EmbedCFlesIntoMaster");
                        EmbedCFlesIntoMaster(sFilePath, listFileEmbeddings);
                    }
                    else
                    {
                        Log.Information("File not exist:" + sFilePath);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error in listFileEmbeddings");
                    //Do nothing. Skip embedding if it fails
                }
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }

        Log.Information("Complete SaveUnitGridTab");
    }


    public static string GetSubjectType(ResearchSummary rs) =>
        (rs is null) ? string.Empty :
            Program.KeywordGeneratorFactory.GetSubjectType(
                rs.EntityName, rs.DUNSNumber, rs.GISID, rs.Country).ToString();


    private static void WriteDefaultHyperlinkBar(ExcelWorksheet worksheet)
    {
        var range = worksheet.Cells["A4:F4"];
        range.Style.Font.Size = 11;
        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

        WriteHyperlink(worksheet, "A4", CAUConstants.MSG_SECTION_GIS, $"A{ResultDetailsSectionRow(RESULT_DETAILS_SECTION_NUM_GIS)}");
        WriteHyperlink(worksheet, "B4", CAUConstants.MSG_SECTION_CER, $"A{ResultDetailsSectionRow(RESULT_DETAILS_SECTION_NUM_CER)}");
        WriteHyperlink(worksheet, "C4", CAUConstants.MSG_SECTION_CRR, $"A{ResultDetailsSectionRow(RESULT_DETAILS_SECTION_NUM_CRR)}");
        WriteHyperlink(worksheet, "D4", CAUConstants.MSG_SECTION_SPL, $"A{ResultDetailsSectionRow(RESULT_DETAILS_SECTION_NUM_SPL)}");
        WriteHyperlink(worksheet, "E4", CAUConstants.MSG_SECTION_FINSCAN, $"A{ResultDetailsSectionRow(RESULT_DETAILS_SECTION_NUM_FINSCAN)}");
        WriteHyperlink(worksheet, "F4", CAUConstants.MSG_SECTION_LOCAL_AND_EXTERNAL_DBS,
            $"A{ResultDetailsSectionRow(RESULT_DETAILS_SECTION_NUM_LOCAL_AND_EXTERNAL_DBS)}");
    }


    private static string GetSPLSummaryMessage(SPLEntity sPLEntity, List<string> keywordsList)
    {
        List<string> results = [];
        foreach (var keyword in keywordsList)
        {
            string keywordResult = string.Empty;

            if (sPLEntity.splLists != null && sPLEntity.commercialSensitivitiesLists != null)
            {
                var matchingSPLList = sPLEntity.splLists.Where(item => item.Entity.Contains(keyword, StringComparison.OrdinalIgnoreCase) || item.GUP.Contains(keyword, StringComparison.OrdinalIgnoreCase) || item.SPLInstructions.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
                var matchingCSLList = sPLEntity.commercialSensitivitiesLists.Where(item => item.Entity.Contains(keyword, StringComparison.OrdinalIgnoreCase) || item.SPLInstructions.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
                keywordResult += (matchingSPLList.Count == 0)
                                    ? "No result found in SPL Sheet. "
                                    : "Result found in SPL Sheet. ";
                keywordResult += (matchingCSLList.Count == 0)
                                    ? "No result found in Commercial Sensitivities list Sheet."
                                    : "Result found in Commercial Sensitivities list Sheet. ";
            }
            else
            {
                keywordResult += "Unable to search SPL to generate results. Please investigate further.";
            }

            results.Add(keywordResult);
        }

        string result = $"SPL > {string.Join(", ", keywordsList)}: ";
        if (results.Distinct().Count() > 1)
        {
            result += "Check the distinct results in the SPL details section. ";
        }
        else
        {
            result += results.FirstOrDefault();
        }
        return result;
    }


    private static int WriteSPLSummaryGrid(ExcelWorksheet worksheet, int iPresentRow, string splSummaryMessage)
    {
        worksheet.Cells[$"A{iPresentRow}"].Value = GetContentsOrNaIfEmpty(splSummaryMessage);
        _summaryRowReference = $"A{iPresentRow}:H{iPresentRow}";
        worksheet.Cells[_summaryRowReference].Style.Font.Size = 8;
        SetCellBackgroundColor(worksheet.Cells[_summaryRowReference], brightTeal);
        worksheet.Cells[_summaryRowReference].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        worksheet.Cells[_summaryRowReference].Style.WrapText = true;
        worksheet.Cells[_summaryRowReference].AutoFitColumns(21, 35);
        worksheet.Cells[_summaryRowReference].Merge = true;
        worksheet.Cells[$"A{iPresentRow - 1}:H{iPresentRow}"].SetBorders(ExcelBorderStyle.Thin, ExcelBorderStyle.Thin);

        return iPresentRow;
    }


    // User Story 1019884 - CER Search and Extract Cont'd ----------
    private static string GetAuditRelationship(List<MercuryEntity_SpreadSheet> lsMercuryEntities, bool IsUKI,
                                               out List<MercuryEntity_SpreadSheet> lsMercuryEntitiesMoreRecentThan3yearsAgo)
    {
        if ((lsMercuryEntities is null) || (lsMercuryEntities.Count == 0))
        {
            lsMercuryEntitiesMoreRecentThan3yearsAgo = null;
            return string.Empty;
        }

        if (IsUKI)
        {
            //UKI -- 6 years
            lsMercuryEntitiesMoreRecentThan3yearsAgo = lsMercuryEntities.Where(mercuryEntity => mercuryEntity.IsEngagementMoreRecentThan(deltaYears: -6)).ToList();

            var lsMercuryEntitiesMoreRecentThan3yearsAgoAuditUKI = lsMercuryEntitiesMoreRecentThan3yearsAgo
           .Where(mercuryEntity => mercuryEntity.IsFinancialStatementAudit()).ToList();
            if (lsMercuryEntitiesMoreRecentThan3yearsAgoAuditUKI.Count > 0)
            {
                // a. The results contain at least one Service Code 35 or 10067
                //    (both codes are 'Financial Statement Audit').                
                return "Audit Client";
            }
            else
            {
                //Go back to 3 years and look for service codes
                lsMercuryEntitiesMoreRecentThan3yearsAgo = lsMercuryEntities.Where(mercuryEntity => mercuryEntity.IsEngagementMoreRecentThan(deltaYears: -3)).ToList();

                //if any service code exists
                var lsMercuryEntitiesUKI = lsMercuryEntitiesMoreRecentThan3yearsAgo
                            .Where(mercuryEntity => mercuryEntity.EngagementGlobalService != "").ToList();
                if (lsMercuryEntitiesUKI.Count > 0)
                {
                    return "Non-Audit Client";
                }
                else
                {
                    return "N/A";
                }
            }
        }
        else
        {

            lsMercuryEntitiesMoreRecentThan3yearsAgo = lsMercuryEntities
                .Where(mercuryEntity => mercuryEntity.IsEngagementMoreRecentThan(deltaYears: -3)).ToList();
            // deltaYears must be negative to result in past dates.
            // After filtering out the engagements older than 3 years...

            if (lsMercuryEntitiesMoreRecentThan3yearsAgo.Count == 0)
            {
                // c. The results are empty.
                return "N/A";
            }

            var lsMercuryEntitiesMoreRecentThan3yearsAgoAudit = lsMercuryEntitiesMoreRecentThan3yearsAgo
                .Where(mercuryEntity => mercuryEntity.IsFinancialStatementAudit()).ToList();
            if (lsMercuryEntitiesMoreRecentThan3yearsAgoAudit.Count > 0)
            {
                // a. The results contain at least one Service Code 35 or 10067
                //    (both codes are 'Financial Statement Audit').
                return "Audit Client";
            }

            // b. The results are not empty, but do not contain Service Code 35 or 10067.
            return "Non-Audit Client";
        }
    }


    private static List<string> GetServiceLines(List<MercuryEntity_SpreadSheet> lsMercuryEntities)
    {
        if ((lsMercuryEntities is null) || (lsMercuryEntities.Count == 0))
        {
            return [];
        }

        return lsMercuryEntities
                .SelectMany(mercuryEntity => mercuryEntity.EngagementServiceLine.Split(","))
                .Select(mercuryEntity => mercuryEntity.FullTrim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(mercuryEntity => mercuryEntity)
                .ToList();
    }
    // User Story 1019884 - CER Search and Extract Cont'd ----------


    private static void WriteTargetSectionHeader(ExcelWorksheet worksheet, int row, string title)
    {
        worksheet.Cells[$"A{row}"].Value = title;
        worksheet.Cells[$"A{row}"].Style.Font.Size = 11;
        worksheet.Cells[$"A{row}"].Style.Font.Bold = true;
        SetCellBackgroundColor(worksheet.Cells[$"A{row}:M{row}"], darkGray);
        worksheet.Cells[$"A{row}:M{row}"].AutoFitColumns(21, 35);
    }


    private static void WriteHyperlink(ExcelWorksheet worksheet, string sourceCell, string title, string targetCell)
    {
        var hyperlinkCell = worksheet.Cells[sourceCell];
        hyperlinkCell.Formula = $"HYPERLINK(\"#{targetCell}\", \"{title.Replace("\"", "\"\"")}\")";
        hyperlinkCell.Style.Font.Color.SetColor(Color.Blue);
        hyperlinkCell.Style.Font.UnderLine = true;
    }


    private static void EmbedCFlesIntoMaster(string MasterWorkbookFullPath, List<FileEmbedding> fileEmbeddings)
    {
        try
        {
            using ExcelFileEmbedder excelFileEmbedder = new(MasterWorkbookFullPath);
            excelFileEmbedder.EmbedDetailFiles(fileEmbeddings);
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
    }


    private static void SetCellBackgroundColor(ExcelRangeBase cell, Color color)
    {
        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
        cell.Style.Fill.BackgroundColor.SetColor(color);
    }
    private static void SetRowCellBackgroundColor(ExcelRangeBase cells, Color color)
    {
        cells.Style.Fill.PatternType = ExcelFillStyle.Solid;
        cells.Style.Fill.BackgroundColor.SetColor(color);
    }
    public static bool GenerateUnitGridTabEmbeddedFiles(string sPath, DataSet ds, ResearchSummary rs, int rowStartIndex, TabCategory tabCategory)
    {
        FileInfo newFile = new FileInfo(sPath);

        try
        {
            string sDirectoryPath = Path.GetFullPath(Path.GetDirectoryName(sPath));
            if (!Directory.Exists(sDirectoryPath))
            {
                Directory.CreateDirectory(sDirectoryPath);
            }

            try
            {
                if (File.Exists(sPath))
                {
                    File.Delete(sPath);
                }
            }
            catch
            {
                //ignored
            }

            using (OfficeOpenXml.ExcelPackage package = new OfficeOpenXml.ExcelPackage(newFile))
            {
                OfficeOpenXml.Style.XmlAccess.ExcelNamedStyleXml headerStyle;
                OfficeOpenXml.Style.XmlAccess.ExcelNamedStyleXml headerFilterStyleGreen;
                OfficeOpenXml.Style.XmlAccess.ExcelNamedStyleXml headerFilterStyleTurquoise;
                OfficeOpenXml.Style.XmlAccess.ExcelNamedStyleXml headerFilterStylePink;
                OfficeOpenXml.Style.XmlAccess.ExcelNamedStyleXml headerFilterStyleYellow;
                OfficeOpenXml.Style.XmlAccess.ExcelNamedStyleXml hyperLinkStyle;
                headerStyle = null;
                hyperLinkStyle = null;
                headerFilterStyleGreen = null;
                headerFilterStyleTurquoise = null;
                headerFilterStylePink = null;
                headerFilterStyleYellow = null;
                Color bgGreen = Color.FromArgb(153, 204, 0); //Near Green
                Color bgTurquoise = Color.FromArgb(51, 204, 204); //Near Turquoise
                Color bgLtYellow = Color.FromArgb(255, 255, 204); //Light Yellow
                Color bgLtPink = Color.FromArgb(255, 182, 193); //Light Pink

                foreach (DataTable dt in ds.Tables)
                {
                    ApplyPrettyName(dt, tabCategory);
                    List<int> dateColumns = new List<int>();
                    int columnEngagementOpenDate = dt.Columns.IndexOf("Engagement Open Date");
                    int columnEngagementOpenDateFrom = dt.Columns.IndexOf("Engagement Open Date From");
                    int columnEngagementOpenDateTo = dt.Columns.IndexOf("Engagement Open Date To");
                    int columnEngagementStatusEffectiveDate = dt.Columns.IndexOf("Engagement Status Effective Date");
                    int columnEngagementLastTimeChargedDate = dt.Columns.IndexOf("Engagement Last Time Charged Date");
                    int columnLatestInvoiceIssuedDate = dt.Columns.IndexOf("Latest Invoice Issued Date");
                    dateColumns.Add(columnEngagementOpenDate);
                    dateColumns.Add(columnEngagementOpenDateFrom);
                    dateColumns.Add(columnEngagementOpenDateTo);
                    dateColumns.Add(columnEngagementStatusEffectiveDate);
                    dateColumns.Add(columnEngagementLastTimeChargedDate);
                    dateColumns.Add(columnLatestInvoiceIssuedDate);
                    //package.Load(new FileStream(sPath,FileMode.Open));
                    OfficeOpenXml.ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(dt.TableName.Replace("'", "`"));

                    if (!worksheet.Workbook.Styles.NamedStyles.Contains(headerStyle)) headerStyle = worksheet.Workbook.Styles.CreateNamedStyle("HeaderColor");
                    headerStyle.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headerStyle.Style.Fill.BackgroundColor.SetColor(Color.Black);
                    headerStyle.Style.Font.Color.SetColor(Color.White);
                    headerStyle.Style.Font.Bold = true;

                    //Apply row start index
                    if (rowStartIndex == 0)
                    {
                        worksheet.Cells["A1"].LoadFromDataTable(dt, true);

                        worksheet.Cells[1, 1, 1, dt.Columns.Count].AutoFilter = true;
                        worksheet.Cells[1, 1, 1, dt.Columns.Count].StyleName = "HeaderColor";

                        worksheet.Cells.AutoFitColumns(60, 100);

                        if (tabCategory == TabCategory.TabGISEmbedded || tabCategory == TabCategory.TabCEREmbedded)
                        {
                            worksheet.Cells.AutoFitColumns(15, 20);

                            for (int i = rowStartIndex + 1; i < dt.Rows.Count; i++)
                            {
                                worksheet.Row(i).Height = 20;
                            }
                        }
                    }
                    else
                    {
                        worksheet.Cells["A" + rowStartIndex].LoadFromDataTable(dt, true);
                        worksheet.Cells[rowStartIndex, 1, rowStartIndex, dt.Columns.Count].AutoFilter = true;
                        worksheet.Cells[rowStartIndex, 1, rowStartIndex, dt.Columns.Count].StyleName = "HeaderColor";

                        //Show legend in header
                        if (tabCategory == TabCategory.TabGISEmbedded)
                        {
                            if (!worksheet.Workbook.Styles.NamedStyles.Contains(headerFilterStyleGreen)) headerFilterStyleGreen = worksheet.Workbook.Styles.CreateNamedStyle("HeaderFilterColorGreen");
                            headerFilterStyleGreen.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            headerFilterStyleGreen.Style.Fill.BackgroundColor.SetColor(bgGreen); //Near Green

                            worksheet.Cells[1, 1, 1, 1].Value = "Legend:";
                            worksheet.Cells[1, 2, 1, 2].StyleName = "HeaderFilterColorGreen";
                            worksheet.Cells[1, 3, 1, 5].Value = "100 % matching with APG Name";
                            worksheet.Cells[1, 3, 1, 5].Merge = true;

                            if (!worksheet.Workbook.Styles.NamedStyles.Contains(headerFilterStyleTurquoise)) headerFilterStyleTurquoise = worksheet.Workbook.Styles.CreateNamedStyle("HeaderFilterColorTurquoise");
                            headerFilterStyleTurquoise.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            headerFilterStyleTurquoise.Style.Fill.BackgroundColor.SetColor(bgTurquoise); //Near Turquoise

                            worksheet.Cells[2, 2, 2, 2].StyleName = "HeaderFilterColorTurquoise";
                            worksheet.Cells[2, 3, 2, 3].Value = "GIS ID Search";

                        }
                        if (tabCategory == TabCategory.TabCEREmbedded)
                        {
                            if (!worksheet.Workbook.Styles.NamedStyles.Contains(headerFilterStyleYellow)) headerFilterStyleYellow = worksheet.Workbook.Styles.CreateNamedStyle("HeaderFilterColorLtYellow");
                            headerFilterStyleYellow.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            headerFilterStyleYellow.Style.Fill.BackgroundColor.SetColor(bgLtYellow); //Near Yellow

                            worksheet.Cells[1, 1, 1, 1].Value = "Legend:";
                            worksheet.Cells[1, 2, 1, 2].StyleName = "HeaderFilterColorLtYellow";
                            worksheet.Cells[1, 3, 1, 5].Value = "100 % matching with APG Name";
                            worksheet.Cells[1, 3, 1, 5].Merge = true;

                            if (!worksheet.Workbook.Styles.NamedStyles.Contains(headerFilterStylePink)) headerFilterStylePink = worksheet.Workbook.Styles.CreateNamedStyle("HeaderFilterColorLtPink");
                            headerFilterStylePink.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            headerFilterStylePink.Style.Fill.BackgroundColor.SetColor(bgLtPink); //Near Pink

                            worksheet.Cells[2, 2, 2, 2].StyleName = "HeaderFilterColorLtPink";
                            worksheet.Cells[2, 3, 2, 3].Value = "DUNS ID Search";

                        }
                        try
                        {
                            worksheet.Cells.AutoFitColumns(60, 100);
                        }
                        catch
                        {
                            //Ignored
                        }


                        if (tabCategory == TabCategory.TabGISEmbedded || tabCategory == TabCategory.TabCEREmbedded)
                        {
                            try
                            {
                                worksheet.Cells.AutoFitColumns(15, 20);
                            }
                            catch
                            {
                                //Ignored
                            }

                            for (int i = rowStartIndex + 1; i < dt.Rows.Count; i++)
                            {
                                worksheet.Row(i).Height = 16;
                                worksheet.Row(i).Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                            }
                        }

                    }

                    //Color code the rows
                    if (tabCategory == TabCategory.TabGISEmbedded)
                    {
                        for (int i = rowStartIndex + 1; i < (dt.Rows.Count + rowStartIndex + 1); i++) //+ rowStartIndex + 1
                        {
                            //A. If 100% fuzzy match, bg color to be green
                            if (worksheet.Cells[$"C{i}"].Value.Equals(100) || worksheet.Cells[$"D{i}"].Value.Equals(100)) //C = BN_Fuzzy, D = AN_Fuzzy
                            {
                                SetCellBackgroundColor(worksheet.Cells[$"A{i}:O{i}"], bgGreen);
                            }
                            if (!string.IsNullOrEmpty(rs.GISID))
                            {
                                if (worksheet.Cells[$"A{i}"].Value.ToString() == rs.GISID) //If GISID matches, bg color to be turquoise
                                {
                                    SetCellBackgroundColor(worksheet.Cells[$"A{i}:O{i}"], bgTurquoise);
                                }
                            }
                        }
                    }

                    if (tabCategory == TabCategory.TabCEREmbedded)
                    {
                        for (int i = rowStartIndex + 1; i < (dt.Rows.Count + rowStartIndex + 1); i++)
                        {
                            //A. If 100% fuzzy match, bg color to be green
                            if (worksheet.Cells[$"AK{i}"].Value.Equals(100) || worksheet.Cells[$"AL{i}"].Value.Equals(100)) //C = BN_Fuzzy, D = AN_Fuzzy
                            {
                                SetCellBackgroundColor(worksheet.Cells[$"A{i}:AN{i}"], bgLtYellow);
                            }
                            if (!string.IsNullOrEmpty(rs.DUNSNumber))
                            {
                                if (worksheet.Cells[$"C{i}"].Value.ToString() == rs.DUNSNumber) //If DUNS matches, bg color to be light pink
                                {
                                    SetCellBackgroundColor(worksheet.Cells[$"A{i}:AN{i}"], bgLtPink);
                                }
                            }
                        }
                    }

                    //Iterate through column index to set columnformat
                    foreach (var col in dateColumns)
                    {
                        if (col == -1)
                            continue;
                        int colindex = col;
                        worksheet.Column(++colindex).Style.Numberformat.Format = "dd mmm yyyy";
                    }

                    //Apply style only if there records in table
                    if (dt.Rows.Count > 0)
                    {
                        if (!worksheet.Workbook.Styles.NamedStyles.Contains(hyperLinkStyle)) hyperLinkStyle = worksheet.Workbook.Styles.CreateNamedStyle("HyperLink");
                        hyperLinkStyle.Style.Font.UnderLine = true;
                        hyperLinkStyle.Style.Font.Color.SetColor(Color.Blue);
                    }

                    package.Save();
                }
            }
        }
        catch (Exception Ex)
        {
            LoggerInfo.LogException(Ex);
            return false;
        }

        return true;
    }
    public static DataTable ApplyPrettyName(DataTable dtbl, TabCategory tabCategory)
    {
        foreach (DataColumn col in dtbl.Columns)
        {
            try
            {
                if (tabCategory == TabCategory.TabGISEmbedded)
                {
                    if (col.ColumnName == "GISID") col.ColumnName = "GIS ID";
                    if (col.ColumnName == "UltimateParent") col.ColumnName = "Ultimate Parent";
                    if (col.ColumnName == "IdentificationNumber") col.ColumnName = "Identification Number";
                    if (col.ColumnName == "BusinessName") col.ColumnName = "Business Name";
                    if (col.ColumnName == "GUPESDID") col.ColumnName = "GUP ESD ID";
                    if (col.ColumnName == "UltimateBusinessName") col.ColumnName = "Ultimate Business Name";
                    if (col.ColumnName == "StateProvinceName") col.ColumnName = "State";
                    if (col.ColumnName == "MarkupCodes") col.ColumnName = "Restriction Name";
                }
                if (tabCategory == TabCategory.TabCEREmbedded)
                {
                    if (col.ColumnName == "ClientID") col.ColumnName = "Client ID";
                    if (col.ColumnName == "Client") col.ColumnName = "Client";
                    if (col.ColumnName == "DUNSNumber") col.ColumnName = "Duns Number";
                    if (col.ColumnName == "DUNSName") col.ColumnName = "DUNSName";
                    if (col.ColumnName == "DUNSLocation") col.ColumnName = "DUNSLocation";
                    if (col.ColumnName == "AccountID") col.ColumnName = "Ultimate Duns Number";
                    if (col.ColumnName == "Account") col.ColumnName = "Account";
                    if (col.ColumnName == "EngagementmentID") col.ColumnName = "Engagement ID";
                    if (col.ColumnName == "Engagement") col.ColumnName = "Engagement";


                    if (col.ColumnName == "GTACStatusFolderName") col.ColumnName = "PACE ID";
                    if (col.ColumnName == "GTACStatus") col.ColumnName = "PACE Status";
                    if (col.ColumnName == "EngagementGlobalService") col.ColumnName = "Global Service";
                    if (col.ColumnName == "EngagementServiceLine") col.ColumnName = "Engagement Service Line";
                    if (col.ColumnName == "EngagementServiceLine") col.ColumnName = "Engagement Sub Service Line";
                    if (col.ColumnName == "EngagementCountry") col.ColumnName = "Engagement Country/Region";
                    if (col.ColumnName == "EngagementStatus") col.ColumnName = "Engagement Status";
                    if (col.ColumnName == "EngagementStatusEffectiveDate") col.ColumnName = "Engagement Status Effective Date";
                    if (col.ColumnName == "EngagementOpenDateShowcase") col.ColumnName = "Engagement Open Date";
                    if (col.ColumnName == "EngagementOpenDateFrom") col.ColumnName = "Engagement Open Date From";
                    if (col.ColumnName == "EngagementOpenDateTo") col.ColumnName = "Engagement Open Date To";

                    if (col.ColumnName == "EngagementLastTimeChargedDate") col.ColumnName = "Engagement Last Time Charged Date";
                    if (col.ColumnName == "LatestInvoiceIssuedDate") col.ColumnName = "Latest Invoice Issued Date";
                    if (col.ColumnName == "EngagementType") col.ColumnName = "Engagement Type";
                    if (col.ColumnName == "GCSP") col.ColumnName = "GCSP";
                    if (col.ColumnName == "GCSPEmail") col.ColumnName = "GCSP Email";
                    if (col.ColumnName == "EngagementPartner") col.ColumnName = "Engagement Partner";
                    if (col.ColumnName == "EngagementPartnerEmail") col.ColumnName = "Engagement Partner Email";
                    if (col.ColumnName == "EngagementManager") col.ColumnName = "Engagement Manager";
                    if (col.ColumnName == "TechnologyIndicatorCdJoin") col.ColumnName = "Engagement Indicator";

                    if (col.ColumnName == "NER") col.ColumnName = "NER/ANSR/Tech Revenue(k)";
                    if (col.ColumnName == "ChargedHours") col.ColumnName = "Charged Hours";
                    if (col.ColumnName == "BilledFees") col.ColumnName = "Billed Fees";
                    if (col.ColumnName == "CurrencyCode") col.ColumnName = "Currency Code";
                    if (col.ColumnName == "AccountChannel") col.ColumnName = "Account Channel";
                    if (col.ColumnName == "SECFlag") col.ColumnName = "Client SECRegistrant";
                    if (col.ColumnName == "EngagementReportingOrg") col.ColumnName = "Engagement Organization";
                    if (col.ColumnName == "BN_Fuzzy") col.ColumnName = "Fuzzy match on unit name with client name";
                    if (col.ColumnName == "AN_Fuzzy") col.ColumnName = "Fuzzy match on unit name with DUNS name";


                }
            }
            catch
            {
                //ignore
            }
        }

        return dtbl;
    }


    public static List<ResearchSummaryEntry> SaveResearchSummaryTab(string destinationFilePath)
    {
        List<ResearchSummaryEntry> researchSummaryEntries = [];

        try
        {
            using (ExcelPackage package = new ExcelPackage(destinationFilePath))
            {
                ExcelWorksheet summaryWorksheet = package.GetWorksheet(CAUConstants.MASTER_WORKBOOK_SUMMARY_TAB);
                ArgumentNullException.ThrowIfNull(summaryWorksheet);

                RSE.ResearchSummaryEngine researchSummaryEngine = new(summaryWorksheet);
                researchSummaryEntries = researchSummaryEngine.GetEntries();
                researchSummaryEngine.WriteEntries(RESEARCH_SUMMARY_STARTING_CELL, researchSummaryEntries);
                // Auto width
                int row = 5;
                foreach (ResearchSummaryEntry researchSummaryEntry in researchSummaryEntries)
                {
                    summaryWorksheet.AutoFitRowHeight(row, 'O', 'O', [('O', 'O')]);
                    row++;
                }
                package.Save();
            }
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred in method ExcelOperations.SaveResearchSummaryTab() - Message: {ex}");
            LoggerInfo.LogException(ex);
            return [];
        }

        return researchSummaryEntries;
    }

    public static void MarkRework(string destinationFilePath, CheckerQueue summary, ConflictService conflictService)
    {
        List<ResearchSummary> rs = summary.researchSummary;
        List<string> apgRolesExclusion = conflictService.GetAPGRolesExclusion();

        using (ExcelPackage package = new ExcelPackage(destinationFilePath))
        {
            ExcelWorksheet _worksheet = package.GetWorksheet(CAUConstants.MASTER_WORKBOOK_AU_UNIT_GRID_TAB);

            int startRow = 5;  // Adjust the starting row as needed
            int endRow = startRow + summary.researchSummary.Count;  // Calculate the ending row based on the number of items

            // Define the entire range
            //var cellRange = _worksheet.Cells[startRow, 2, endRow, 18];
            int i = startRow + 1;
            while ((_worksheet.Cells[$"B{i}"].Value != null) && !string.IsNullOrEmpty(_worksheet.Cells[$"B{i}"].Value.ToString()))
            {

                string ws_num = _worksheet.Cells[$"B{i}"].Value.ToString();
                var ws_Rework = Convert.ToString(_worksheet.Cells[$"R{i}"].Value);
                var ws_entityName = _worksheet.Cells[$"C{i}"].Value.ToString();
                var ws_gisid = _worksheet.Cells[$"D{i}"].Value == null ? "" : _worksheet.Cells[$"D{i}"].Value.ToString();
                var ws_mdmid = _worksheet.Cells[$"E{i}"].Value == null ? "" : _worksheet.Cells[$"E{i}"].Value.ToString();
                var ws_duns = _worksheet.Cells[$"F{i}"].Value == null ? "" : _worksheet.Cells[$"F{i}"].Value.ToString();
                var ws_country = _worksheet.Cells[$"G{i}"].Value == null ? "" : _worksheet.Cells[$"G{i}"].Value.ToString();
                var ws_sourceSystem = _worksheet.Cells[$"H{i}"].Value == null ? "" : _worksheet.Cells[$"H{i}"].Value.ToString();
                var ws_role = _worksheet.Cells[$"I{i}"].Value == null ? "" : _worksheet.Cells[$"I{i}"].Value.ToString();
                var ws_clientSide = _worksheet.Cells[$"J{i}"].Value == null ? "" : _worksheet.Cells[$"J{i}"].Value.ToString();
                var ws_unit_type = _worksheet.Cells[$"Q{i}"].Value == null ? "" : _worksheet.Cells[$"Q{i}"].Value.ToString();

                var lst_result = rs.Where(r => r.EntityName == ws_entityName.ToString()).ToList();

                if (lst_result.Count == 1)
                {
                    var Result = lst_result[0];
                    Result.WorksheetNo = ws_num;
                    if (ws_Rework == "Yes")
                    {
                        if (apgRolesExclusion.Contains(ws_role))
                        {
                            Result.PerformResearch = false;
                        }
                        else { Result.PerformResearch = true; }

                        Result.Rework = true;
                    }
                }
                else
                {
                    ResearchSummary _rs = new ResearchSummary();
                    int n;
                    if (int.TryParse(ws_num, out n))
                    {
                        _rs.WorksheetNo = n.ToString("D2");
                    }
                    else
                        _rs.WorksheetNo = ws_num;

                    _rs.EntityName = ws_entityName;
                    _rs.GISID = ws_gisid.ToString();
                    _rs.MDMID = ws_mdmid.ToString();
                    _rs.DUNSNumber = ws_duns.ToString();
                    _rs.Country = ws_country.ToString();
                    _rs.SourceSystem = ws_sourceSystem.ToString();
                    _rs.Role = ws_role.ToString();
                    _rs.IsClientSide = (ws_clientSide == "Client Side");
                    _rs.Type = ws_unit_type;
                    _rs.EntityWithoutLegalExt = ConflictService.DropLegalExtension(_rs.EntityName, _rs.Type);

                    if (apgRolesExclusion.Contains(_rs.Role))
                    {
                        _rs.PerformResearch = false;
                    }
                    else { _rs.PerformResearch = true; }
                    _rs.Rework = true;

                    summary.researchSummary.Add(_rs);
                }

                i++;
            }

        }
    }


    private static int ResultDetailsSectionRow(int sectionNumber) =>
        _resultDetailsSectionsStartingRow +
        ((sectionNumber - RESULT_DETAILS_SECTIONS_FIRST_SECTION_NUM) * resultDetailsSectionsHeightInRows);


    private static string GetContentsOrNaIfEmpty(string text) =>
        string.IsNullOrWhiteSpace(text) ? CAUConstants.MSG_NO_DATA : text;


    //public void GISRework(ResearchSummary rs, List<SearchEntitiesResponseItemViewModel> GISEntityGrid)
    //{
    //    bool isDUNSchanged = false;
    //    bool isGISIDchanged = false;
    //    bool isMDMIDchanged = false;
    //    foreach (SearchEntitiesResponseItemViewModel unitGrid_GISEntity in GISEntityGrid)
    //    {
    //        //As per the story, if we are missing any IDs in AU unit grid then back fill
    //        if (unitGrid_GISEntity.CountryName == rs.Country)
    //        {
    //            if (string.IsNullOrEmpty(rs.DUNSNumber) && !string.IsNullOrEmpty(unitGrid_GISEntity.DUNSNumber))
    //            {
    //                rs.DUNSNumber = unitGrid_GISEntity.DUNSNumber;
    //                isDUNSchanged = true;
    //            }
    //            if (string.IsNullOrEmpty(rs.MDMID) && !string.IsNullOrEmpty(unitGrid_GISEntity.MDMID.ToString()))
    //            {
    //                rs.MDMID = unitGrid_GISEntity.MDMID.ToString();
    //                isMDMIDchanged = true;
    //            }
    //            if (string.IsNullOrEmpty(rs.GISID) && !string.IsNullOrEmpty(unitGrid_GISEntity.GisId.ToString()))
    //            {
    //                rs.GISID = unitGrid_GISEntity.GisId.ToString();
    //                isGISIDchanged = true;
    //            }
    //        }

    //        if (isGISIDchanged || isDUNSchanged || isMDMIDchanged)
    //            rs.Type = GetSubjectType(rs);
    //            goto GIS_REWORK;
    //    }
    //}
}

#pragma warning restore CA1866 // Use char overload
#pragma warning restore CS0164 // This label has not been referenced
#pragma warning restore IDE0018 // Inline variable declaration
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
#pragma warning restore CA1827 // Do not use Count() or LongCount() when Any() can be used
#pragma warning restore IDE0305 // Simplify collection initialization
#pragma warning restore IDE0017 // Simplify object initialization
#pragma warning restore IDE0063 // Use simple 'using' statement
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore CA1860 // Avoid using 'Enumerable.Any()' extension method
#pragma warning restore IDE0028 // Simplify collection initialization
