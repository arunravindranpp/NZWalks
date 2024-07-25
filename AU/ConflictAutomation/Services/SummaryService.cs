using ConflictAutomation.Constants;
using ConflictAutomation.Extensions;
using ConflictAutomation.Models;
using ConflictAutomation.Models.PreScreening;
using ConflictAutomation.Models.PreScreening.SubClasses;
using ConflictAutomation.Services.PreScreening;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Office.Interop.Excel;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PACE;
using PACE.Domain.Services;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using _Excel = Microsoft.Office.Interop.Excel;

namespace ConflictAutomation.Services;

#pragma warning disable CA2211 // Non-constant fields should not be visible
#pragma warning disable IDE0063 // Use simple 'using' statement
#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0017 // Simplify object initialization
#pragma warning disable IDE0057 // Use range operator
#pragma warning disable CA1860 // Avoid using 'Enumerable.Any()' extension method
#pragma warning disable IDE0018 // Inline variable declaration
#pragma warning disable CA1806 // Do not ignore method results
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
#pragma warning disable CA1416 // Validate platform compatibility
public class SummaryService
{
    const int maxIterations = 10;
    const int startRow = 4;
    const int endRow = 80;
    public static List<string> excelFields;
    public static bool ProcessCompletedCheck(AppConfigure _configuration, long conflictCheckID, ConflictCheck conflictCheck, ServiceProvider serviceProvider, CheckerQueue summary, List<ApprovalRungQuestion> processQuestionnaire, string destinationFilePath, string destinationPath)
    {
        try
        {

            bool oldTemplate = false;
            Attachment latestAttachment = null;
            var attachments = serviceProvider.GetService<IConflictCheckServices>()
                                             .GetAttachments("Conflicts-AttachmentsForConsolidatedResearchResults", conflictCheckID.ToString(), "PACE.ConflictCheck");
            Match match = Regex.Match(summary.pursuitCheckID.ToString(), @"\d+");
            bool isNumber = int.TryParse(match.Value, out int result);
            if (isNumber)
            {
                latestAttachment = attachments
                                   .Where(i => i.FileName.Contains(match.Value) && i.FileName.Contains("Research Template"))
                                   .OrderByDescending(i => i.UploadedDate)
                                   .FirstOrDefault();

                if (latestAttachment == null)
                {
                    latestAttachment = attachments
                                       .Where(i => i.FileName.Contains(match.Value))
                                       .OrderByDescending(i => i.UploadedDate)
                                       .FirstOrDefault();

                    if (latestAttachment == null)
                    {
                        return false;
                    }
                    else
                    {
                        oldTemplate = true;
                    }
                }
            }
            else
            {
                return false;
            }
            var dt = EYSql.ExecuteDataset(Program.PACEConnectionString, CommandType.StoredProcedure,
            @"usp_Get_Attachment", new SqlParameter("AttachmentID", latestAttachment.AttachmentId)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                var file = dt.Rows[0]["FileName"].ToString();
                var Type = dt.Rows[0]["Type"].ToString();
                long EntityID;
                long.TryParse(dt.Rows[0]["EntityID"].ToString(), out EntityID);
                file = Regex.Replace(file, "[#',%^+&]", "_");
                var fileData = (byte[])dt.Rows[0]["Content"];
                string guidfolder = Guid.NewGuid().ToString();
                var filePath = Path.Combine(destinationPath, file);
                File.WriteAllBytes(filePath, fileData);
                var summaryPreviousCheck = new CheckerQueue();
                var preScreeningInfoPrev = new PreScreeningInfo();
                if (oldTemplate)
                {
                    GetSummaryFromOldExcel(summaryPreviousCheck, filePath);
                }
                else
                {
                    GetSummaryFromExcel(summaryPreviousCheck, filePath);
                }
                string prescreeingPath = "";
                if (oldTemplate)
                {
                    GetPrescreeningFromOldExcel(preScreeningInfoPrev, filePath);
                    SaveSummaryPursuitTab(summaryPreviousCheck, destinationFilePath, CAUConstants.MASTER_WORKBOOK_SUMMARY_PURSUIT_TAB);
                }
                else
                {
                    prescreeingPath = GetPrescreeningFromExcelToDatatable(preScreeningInfoPrev, filePath);
                    GetPrescreeningFromExcel(preScreeningInfoPrev, prescreeingPath);
                    SaveSummaryPursuitTab(summaryPreviousCheck, destinationFilePath, CAUConstants.MASTER_WORKBOOK_SUMMARY_PURSUIT_TAB);
                }
                PreScreeningOperations.WritePrescreeningPursuitTab(preScreeningInfoPrev, destinationFilePath);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                if (File.Exists(prescreeingPath))
                {
                    File.Delete(prescreeingPath);
                }
                ExcelOperations.SaveSummaryTab(conflictCheck, summary, destinationFilePath);
                if (!oldTemplate)
                {
                    CompareSummary(summary, summaryPreviousCheck, destinationFilePath);
                }
                if (oldTemplate)
                {
                    PreScreeningOperations.WritePrescreeningTab(_configuration, conflictCheckID, conflictCheck, summary.questionnaireSummary, summary.questionnaireAdditionalParties, processQuestionnaire, destinationFilePath);
                }
                else
                {
                    ExcelOperations.SavePrescreeningTab(conflictCheck, summary, preScreeningInfoPrev, processQuestionnaire, destinationFilePath);
                }
            }
            return isNumber;
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
            return false;
        }
    }

    public static void GetSummaryFromExcel(CheckerQueue queue, string existingFilePath)
    {
        try
        {
            using (var package = new ExcelPackage(new FileInfo(existingFilePath)))
            {
                var worksheet = package.Workbook.Worksheets.Where(x => x.Name == "Summary").FirstOrDefault();
                if (worksheet == null)
                    return;
                queue.ConflictCheckType = worksheet.Cells["C4"].Value?.ToString();
                queue.Region = worksheet.Cells["C5"].Value?.ToString();
                queue.ClientName = worksheet.Cells["C6"].Value?.ToString();
                queue.EngagementName = worksheet.Cells["C7"].Value?.ToString();
                queue.SubServiceLine = worksheet.Cells["C8"].Value?.ToString();
                queue.Services = worksheet.Cells["C9"].Value?.ToString();
                queue.checkPerformed = worksheet.Cells["C10"].Value?.ToString();
                queue.SubTypeofCheck = worksheet.Cells["C11"].Value?.ToString();
                queue.AttachmentsinPACE = worksheet.Cells["C12"].Value?.ToString();
                queue.ConfidentialPace = worksheet.Cells["C13"].Value?.ToString();
                queue.Contractuallylimit = worksheet.Cells["C14"].Value?.ToString();
                queue.DisputeLitigation = worksheet.Cells["C15"].Value?.ToString();
                queue.Hostile = worksheet.Cells["C16"].Value?.ToString();
                queue.Auction = worksheet.Cells["C17"].Value?.ToString();
                queue.AuditPartner = worksheet.Cells["C18"].Value?.ToString();
                queue.TimeStamp = worksheet.Cells["C19"].Value?.ToString();
                queue.GovtEntity = worksheet.Cells["C20"].Value?.ToString();
                queue.ConflictCheckID = Convert.ToInt64(worksheet.Cells["C21"]?.Value);
                queue.onShore = worksheet.Cells["C22"].Value?.ToString();
                queue.CheckerName = worksheet.Cells["C23"].Value?.ToString();
                queue.ReviewerName = worksheet.Cells["C24"].Value?.ToString();
                queue.EngagementDesc = worksheet.Cells["D5"].Value?.ToString();
                queue.Case = worksheet.Cells["C30"].Value?.ToString();
                queue.CaseDescription = worksheet.Cells["D30"].Value?.ToString();
                queue.Issue = worksheet.Cells["C31"].Value?.ToString();
                queue.IssueDescription = worksheet.Cells["D31"].Value?.ToString();
                queue.Conclusion = worksheet.Cells["C32"].Value?.ToString();
                queue.ConclusionDescription = worksheet.Cells["D32"].Value?.ToString();
                queue.Condition1 = worksheet.Cells["C33"].Value?.ToString();
                queue.Condition1Description = worksheet.Cells["D33"].Value?.ToString();
                queue.Condition2 = worksheet.Cells["C34"].Value?.ToString();
                queue.Condition2Description = worksheet.Cells["D34"].Value?.ToString();
                queue.Condition3 = worksheet.Cells["C35"].Value?.ToString();
                queue.Condition3Description = worksheet.Cells["D35"].Value?.ToString();
                queue.Condition4 = worksheet.Cells["C36"].Value?.ToString();
                queue.Condition4Description = worksheet.Cells["D36"].Value?.ToString();
                queue.researchSummary = new List<ResearchSummary>();
                for (int row = 5; row < 30; row++) // Start from row 5 to skip header
                {
                    ResearchSummary data = new ResearchSummary();
                    var cell = worksheet.Cells[row, 12];
                    string formula = cell.Formula;
                    if (string.IsNullOrEmpty(formula))
                    {
                        continue;
                    }
                    var hyperlinkText = formula.Substring(0, formula.Length - 2);
                    var partiesInvolved = hyperlinkText.Split(", \"").Last();
                    data.PartyInvolved = partiesInvolved;
                    data.Country = worksheet.Cells[row, 13].Value?.ToString();
                    data.Role = worksheet.Cells[row, 14].Value?.ToString();
                    data.Summary = worksheet.Cells[row, 15].Value?.ToString();
                    queue.researchSummary.Add(data);
                }
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
    }
    public static void GetSummaryFromOldExcel(CheckerQueue queue, string existingFilePath)
    {
        try
        {
            using (var package = new ExcelPackage(new FileInfo(existingFilePath)))
            {
                var worksheet = package.Workbook.Worksheets.Where(x => x.Name == "Summary").FirstOrDefault();
                if (worksheet == null)
                    return;
                queue.ConflictCheckType = worksheet.Cells["C4"].Value?.ToString();
                queue.Region = worksheet.Cells["C5"].Value?.ToString();
                queue.ClientName = worksheet.Cells["C6"].Value?.ToString();
                queue.EngagementName = worksheet.Cells["C7"].Value?.ToString();
                queue.SubServiceLine = worksheet.Cells["C8"].Value?.ToString();
                queue.Services = worksheet.Cells["C9"].Value?.ToString();
                queue.checkPerformed = worksheet.Cells["C10"].Value?.ToString();
                queue.SubTypeofCheck = worksheet.Cells["C11"].Value?.ToString();
                queue.AttachmentsinPACE = worksheet.Cells["C12"].Value?.ToString();
                queue.ConfidentialPace = worksheet.Cells["C13"].Value?.ToString();
                queue.Sanctioned = worksheet.Cells["C14"].Value?.ToString();
                queue.Contractuallylimit = worksheet.Cells["C15"].Value?.ToString();
                queue.DisputeLitigation = worksheet.Cells["C16"].Value?.ToString();
                queue.Hostile = worksheet.Cells["C17"].Value?.ToString();
                queue.Auction = worksheet.Cells["C18"].Value?.ToString();
                queue.AuditPartner = worksheet.Cells["C19"].Value?.ToString();
                queue.TimeStamp = worksheet.Cells["C20"].Value?.ToString();
                queue.GovtEntity = worksheet.Cells["C21"].Value?.ToString();
                queue.ConflictCheckID = Convert.ToInt64(worksheet.Cells["C22"]?.Value);
                queue.onShore = worksheet.Cells["C23"].Value?.ToString();
                queue.CheckerName = worksheet.Cells["C24"].Value?.ToString();
                queue.ReviewerName = worksheet.Cells["C25"].Value?.ToString();
                queue.EngagementDesc = worksheet.Cells["D5"].Value?.ToString();
                queue.Case = worksheet.Cells["C31"].Value?.ToString();
                queue.CaseDescription = worksheet.Cells["D31"].Value?.ToString();
                queue.Issue = worksheet.Cells["C32"].Value?.ToString();
                queue.IssueDescription = worksheet.Cells["D32"].Value?.ToString();
                queue.Conclusion = worksheet.Cells["C33"].Value?.ToString();
                queue.ConclusionDescription = worksheet.Cells["D33"].Value?.ToString();
                queue.Condition1 = worksheet.Cells["C34"].Value?.ToString();
                queue.Condition1Description = worksheet.Cells["D34"].Value?.ToString();
                queue.Condition2 = worksheet.Cells["C35"].Value?.ToString();
                queue.Condition2Description = worksheet.Cells["D35"].Value?.ToString();
                queue.Condition3 = worksheet.Cells["C36"].Value?.ToString();
                queue.Condition3Description = worksheet.Cells["D36"].Value?.ToString();
                queue.Condition4 = worksheet.Cells["C37"].Value?.ToString();
                queue.Condition4Description = worksheet.Cells["D37"].Value?.ToString();
                queue.researchSummary = new List<ResearchSummary>();
                for (int row = 5; row < 30; row++) // Start from row 5 to skip header
                {
                    ResearchSummary data = new ResearchSummary();
                    data.PartyInvolved = worksheet.Cells[row, 12].Value?.ToString();
                    if (string.IsNullOrEmpty(data.PartyInvolved))
                    {
                        continue;
                    }
                    data.Country = worksheet.Cells[row, 13].Value?.ToString();
                    data.Role = worksheet.Cells[row, 14].Value?.ToString();
                    data.Summary = worksheet.Cells[row, 15].Value?.ToString();
                    queue.researchSummary.Add(data);
                }
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
    }
    public static void SaveSummaryPursuitTab(CheckerQueue queue, string existingFilePath, string sheetName, bool isResubmitted = false)
    {
        try
        {
            using (var package = new ExcelPackage(new FileInfo(existingFilePath)))
            {
                var worksheet = package.Workbook.Worksheets.Where(x => x.Name == sheetName).FirstOrDefault();
                if (worksheet == null)
                    return;
                worksheet.Hidden = eWorkSheetHidden.Visible;
                if (isResubmitted)
                {
                    worksheet.Name = CAUConstants.MASTER_WORKBOOK_SUMMARY_PREVIOUS;
                }
                worksheet.Cells["C4"].Value = queue.ConflictCheckType;
                worksheet.Cells["C5"].Value = queue.Region;
                worksheet.Cells["C6"].Value = queue.ClientName;
                worksheet.Cells["C7"].Value = queue.EngagementName;
                worksheet.Cells["C8"].Value = queue.SubServiceLine;
                worksheet.Cells["C8"].Style.WrapText = true;
                worksheet.Cells["C9"].Value = queue.Services;
                worksheet.Cells["C9"].Style.WrapText = true;
                worksheet.Cells["C10"].Value = queue.checkPerformed;
                worksheet.Cells["C11"].Value = queue.SubTypeofCheck;
                worksheet.Cells["C12"].Value = queue.AttachmentsinPACE;
                worksheet.Cells["C13"].Value = queue.ConfidentialPace;
                worksheet.Cells["C14"].Value = queue.Contractuallylimit;
                worksheet.Cells["C15"].Value = queue.DisputeLitigation;
                worksheet.Cells["C16"].Value = queue.Hostile;
                worksheet.Cells["C17"].Value = queue.Auction;
                worksheet.Cells["C18"].Value = queue.AuditPartner;
                worksheet.Cells["C19"].Value = queue.TimeStamp;
                worksheet.Cells["C20"].Value = queue.GovtEntity;
                worksheet.Cells["C21"].Value = queue.ConflictCheckID;
                worksheet.Cells["C22"].Value = queue.onShore;
                worksheet.Cells["C23"].Value = queue.CheckerName;
                worksheet.Cells["C24"].Value = queue.ReviewerName;
                for (int row = 22; row <= 24; row++)
                {
                    worksheet.Cells[$"C{row}"].Style.WrapText = true;
                    worksheet.AutoFitRowHeight(row, 'C', 'C');
                }
                worksheet.Cells["D5"].Value = queue.EngagementDesc;
                worksheet.Cells["D5"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["D5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells["D5"].Style.WrapText = true;
                worksheet.Cells["C30"].Value = queue.Case;
                worksheet.Cells["D30"].Value = queue.CaseDescription;
                worksheet.Cells["C31"].Value = queue.Issue;
                worksheet.Cells["D31"].Value = queue.IssueDescription;
                worksheet.Cells["C32"].Value = queue.Conclusion;
                worksheet.Cells["D32"].Value = queue.ConclusionDescription;
                worksheet.Cells["C33"].Value = queue.Condition1;
                worksheet.Cells["D33"].Value = queue.Condition1Description;
                worksheet.Cells["C34"].Value = queue.Condition2;
                worksheet.Cells["D34"].Value = queue.Condition2Description;
                worksheet.Cells["C35"].Value = queue.Condition3;
                worksheet.Cells["D35"].Value = queue.Condition3Description;
                worksheet.Cells["C36"].Value = queue.Condition4;
                worksheet.Cells["D36"].Value = queue.Condition4Description;
                worksheet.Columns[15].Width = 76.0d;
                int i = 4;
                if (queue.researchSummary != null)
                {
                    foreach (var item in queue.researchSummary)
                    {
                        ++i;
                        var cellL = worksheet.Cells[$"L{i}"];
                        var cellM = worksheet.Cells[$"M{i}"];
                        var cellN = worksheet.Cells[$"N{i}"];
                        var cellO = worksheet.Cells[$"O{i}"];
                        cellL.Value = item.PartyInvolved;
                        cellM.Value = item.Country;
                        cellN.Value = item.Role;
                        cellO.Value = item.Summary;
                        cellL.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        cellM.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        cellN.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        cellO.Style.Border.BorderAround(ExcelBorderStyle.Thin);
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

    public static void GetPrescreeningFromExcel(PreScreeningInfo queue, string existingFilePath)
    {
        try
        {
            using (var package = new ExcelPackage(new FileInfo(existingFilePath)))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault(x => x.Name == "PreScreening Info");
                if (worksheet == null)
                    return;

                for (int row = startRow; row <= endRow; row++)
                {
                    var header = worksheet.Cells[row, 2].Value?.ToString();
                    if (string.IsNullOrEmpty(header))
                        continue;
                    switch (header.ToLower())
                    {
                        case "triggers for check":
                            ProcessTriggerForCheck(queue, worksheet, ref row);
                            break;
                        case "notes":
                            ProcessNotes(queue, worksheet, ref row);
                            break;
                        case "team members":
                            ProcessTeamMembers(queue, worksheet, ref row);
                            break;
                        case "questionnaire additional parties":
                            ProcessAdditionalParties(queue, worksheet, ref row);
                            break;
                        case "hostile question":
                            ProcessHostileQuestion(queue, worksheet, ref row);
                            break;
                        case "are there limitations to act for specific entities or within a market requested by the client?":
                            ProcessLimitationsToAct(queue, worksheet, ref row);
                            break;
                        case "has another conflict check been performed in connection with this engagement?":
                            ProcessAnotherConflictCheck(queue, worksheet, ref row);
                            break;
                        case "dispute/litigation involvement":
                            ProcessDisputeLitigationInvolvement(queue, worksheet, ref row);
                            break;
                        case "negative press coverage":
                            ProcessHighProfileEngagement(queue, worksheet, ref row);
                            break;
                        case "are there any reasons why the counterparty gcsp/lap should not be contacted?":
                            ProcessConsentToContactCounterparty(queue, worksheet, ref row);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
    }
    public static void GetPrescreeningFromOldExcel(PreScreeningInfo queue, string existingFilePath)
    {
        try
        {
            using (var package = new ExcelPackage(new FileInfo(existingFilePath)))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault(x => x.Name == "PreScreening Info");
                if (worksheet == null)
                    return;

                for (int row = startRow; row <= endRow; row++)
                {
                    var header = worksheet.Cells[row, 2].Value?.ToString();
                    if (string.IsNullOrEmpty(header))
                        continue;
                    switch (header.ToLower())
                    {
                        case "triggers for check":
                            ProcessTriggerForCheck(queue, worksheet, ref row);
                            break;
                        case "notes":
                            ProcessNotes(queue, worksheet, ref row);
                            break;
                        case "team members":
                            ProcessTeamMembers(queue, worksheet, ref row);
                            break;
                        case "questionnaire additional parties":
                            ProcessAdditionalParties(queue, worksheet, ref row);
                            break;
                        case "hostile question":
                            ProcessHostileQuestion(queue, worksheet, ref row);
                            break;
                        case "could this engagement contractually limit our ability to work for other clients?":
                            ProcessLimitationsToAct(queue, worksheet, ref row);
                            break;
                        case "pursuit check performed":
                            ProcessAnotherConflictCheck(queue, worksheet, ref row);
                            break;
                        case "dispute/litigation involvement":
                            ProcessDisputeLitigationInvolvement(queue, worksheet, ref row);
                            break;
                        case "high profile engagement":
                            ProcessHighProfileEngagement(queue, worksheet, ref row);
                            break;
                        case "whether to contact counterparty (g)csp/audit partner?":
                            ProcessConsentToContactCounterparty(queue, worksheet, ref row);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
    }
    public static string GetPrescreeningFromExcelToDatatable(PreScreeningInfo queue, string existingFilePath)
    {
        System.Data.DataTable dataTable = new System.Data.DataTable();
        string fullpath = "";
        try
        {
            string directoryPath = Path.GetDirectoryName(existingFilePath);
            string tempExcel = "PreScreening.xlsx";
            fullpath = Path.Combine(directoryPath, tempExcel);
            dataTable = ReadExcelToDataTableInterop(existingFilePath, "PreScreening Info");
            if (dataTable.Rows.Count > 0)
            {
                // Return the value of the first column in the first row
                var firstelement = dataTable.Rows[0][0].ToString();
                if (firstelement.StartsWith("All information"))
                {
                    DataColumn newColumn = new DataColumn("Column0", typeof(string));
                    dataTable.Columns.Add(newColumn);
                    newColumn.SetOrdinal(0);
                }
            }
            SaveDataTableToExcel(dataTable, fullpath);
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
        return fullpath;
    }
    static void SaveDataTableToExcel(System.Data.DataTable dataTable, string filePath)
    {
        try
        {
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("PreScreening Info");

                // Load the datatable into the sheet, starting from cell A1.
                // Print the column names on row 1
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);

                // Format the header for column names
                using (ExcelRange range = worksheet.Cells[1, 1, 1, dataTable.Columns.Count])
                {
                    range.Style.Font.Bold = true;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Save the file
                FileInfo fileInfo = new FileInfo(filePath);
                excelPackage.SaveAs(fileInfo);
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
    }
    static System.Data.DataTable ReadExcelToDataTable(string filePath, string sheetName)
    {
        System.Data.DataTable dt = new System.Data.DataTable();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets[sheetName];
            if (worksheet == null)
                throw new Exception($"Worksheet {sheetName} not found.");

            // Add columns to DataTable
            for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
            {
                dt.Columns.Add(worksheet.Cells[1, col].Text);
            }

            // Add rows to DataTable
            for (int row = worksheet.Dimension.Start.Row + 1; row <= worksheet.Dimension.End.Row; row++)
            {
                DataRow newRow = dt.NewRow();
                for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
                {
                    newRow[col - 1] = worksheet.Cells[row, col].Text;
                }
                dt.Rows.Add(newRow);
            }
        }

        return dt;
    }

    static System.Data.DataTable ReadExcelToDataTableInterop(string filePath, string sheetName)
    {
        System.Data.DataTable dt = new System.Data.DataTable();
        Application excelApp = new Application();
        Workbook workbook = null;
        Worksheet worksheet = null;
        _Excel.Range range = null;

        try
        {
            workbook = excelApp.Workbooks.Open(filePath);
            worksheet = workbook.Sheets[sheetName] as Worksheet;
            if (worksheet == null)
                return dt;

            range = worksheet.UsedRange;
            int rows = range.Rows.Count;
            int cols = range.Columns.Count;

            // Add columns to DataTable
            for (int c = 1; c <= cols; c++)
            {
                var cell = (range.Cells[1, c] as _Excel.Range).Value2;
                string colName = cell != null ? cell.ToString() : $"Column{c}";
                dt.Columns.Add(colName);
            }

            // Add rows to DataTable
            for (int r = 2; r <= rows; r++)
            {
                DataRow row = dt.NewRow();
                for (int c = 1; c <= cols; c++)
                {
                    var cell = (range.Cells[r, c] as _Excel.Range).Value2;
                    row[c - 1] = cell != null ? cell.ToString() : string.Empty;
                }
                dt.Rows.Add(row);
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
        finally
        {
            // Cleanup
            workbook?.Close(false);
            excelApp.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
        }

        return dt;
    }
    public static bool ProcessResubmittedCheck(AppConfigure _configuration, long conflictCheckID, ConflictCheck conflictCheck, ServiceProvider serviceProvider, CheckerQueue summary, string destinationFilePath, string destinationPath, List<ApprovalRungQuestion> processQuestionnaire = null)
    {
        try
        {
            bool oldTemplate = false;
            var conflictCheckService = serviceProvider.GetService<IConflictCheckServices>();
            var attachments = conflictCheckService.GetAttachments("Conflicts-AttachmentsForConsolidatedResearchResults", conflictCheckID.ToString(), "PACE.ConflictCheck");
            var latestAttachment = attachments
                .Where(i => i.FileName.StartsWith("Research Template"))
                .OrderByDescending(i => i.UploadedDate)
                .FirstOrDefault();
            if (latestAttachment == null)
            {
                latestAttachment = attachments
                    .OrderByDescending(i => i.UploadedDate)
                    .FirstOrDefault();

                if (latestAttachment == null)
                {
                    return false;
                }
                else
                {
                    oldTemplate = true;
                }
            }
            var dt = EYSql.ExecuteDataset(Program.PACEConnectionString, CommandType.StoredProcedure,
            @"usp_Get_Attachment", new SqlParameter("AttachmentID", latestAttachment.AttachmentId)).Tables[0];
            if (dt.Rows.Count > 0)
            {
                var file = dt.Rows[0]["FileName"].ToString();
                var Type = dt.Rows[0]["Type"].ToString();
                long EntityID;
                long.TryParse(dt.Rows[0]["EntityID"].ToString(), out EntityID);
                file = Regex.Replace(file, "[#',%^+&]", "_");
                var fileData = (byte[])dt.Rows[0]["Content"];
                string guidfolder = Guid.NewGuid().ToString();
                var filePath = Path.Combine(destinationPath, file);
                File.WriteAllBytes(filePath, fileData);
                var summaryPreviousCheck = new CheckerQueue();
                var preScreeningInfoPrev = new PreScreeningInfo();
                if (oldTemplate)
                {
                    GetSummaryFromOldExcel(summaryPreviousCheck, filePath);
                }
                else
                {
                    GetSummaryFromExcel(summaryPreviousCheck, filePath);
                }
                string prescreeingPath = "";
                if (oldTemplate)
                {
                    GetPrescreeningFromOldExcel(preScreeningInfoPrev, filePath);
                    SaveSummaryPursuitTab(summaryPreviousCheck, destinationFilePath, CAUConstants.MASTER_WORKBOOK_SUMMARY_PURSUIT_TAB, true);
                }
                else
                {
                    prescreeingPath = GetPrescreeningFromExcelToDatatable(preScreeningInfoPrev, filePath);
                    GetPrescreeningFromExcel(preScreeningInfoPrev, prescreeingPath);
                    SaveSummaryPursuitTab(summaryPreviousCheck, destinationFilePath, CAUConstants.MASTER_WORKBOOK_SUMMARY_PURSUIT_TAB, true);
                }
                PreScreeningOperations.WritePrescreeningResubmittedTab(preScreeningInfoPrev, destinationFilePath);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                if (File.Exists(prescreeingPath))
                {
                    File.Delete(prescreeingPath);
                }
                ExcelOperations.SaveSummaryTab(conflictCheck, summary, destinationFilePath);
                if (!oldTemplate)
                {
                    CompareSummary(summary, summaryPreviousCheck, destinationFilePath);
                }
                if (oldTemplate)
                {
                    PreScreeningOperations.WritePrescreeningTab(_configuration, conflictCheckID, conflictCheck, summary.questionnaireSummary, summary.questionnaireAdditionalParties, processQuestionnaire, destinationFilePath);
                }
                else
                {
                    ExcelOperations.SavePrescreeningTab(conflictCheck, summary, preScreeningInfoPrev, processQuestionnaire, destinationFilePath);
                }
                return true;
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
        return false;
    }
    static void ProcessTriggerForCheck(PreScreeningInfo queue, ExcelWorksheet worksheet, ref int row)
    {
        row += 2;
        queue.ListTriggersForCheck = new List<TriggerForCheck>();
        for (int i = 0; i < maxIterations; i++)
        {
            var triggerForCheck = new TriggerForCheck();
            var trigerType = worksheet.Cells[row, 3].Value?.ToString();
            var details = worksheet.Cells[row, 4].Value?.ToString();
            if (!string.IsNullOrEmpty(trigerType))
            {
                triggerForCheck.TriggerType = trigerType;
                triggerForCheck.Details = details;
                queue.ListTriggersForCheck.Add(triggerForCheck);
                row++;
            }
            else
            {
                break;
            }
        }
    }

    static void ProcessNotes(PreScreeningInfo queue, ExcelWorksheet worksheet, ref int row)
    {
        row += 2;
        queue.ListNotes = new List<Note>();
        for (int i = 0; i < maxIterations; i++)
        {
            var preNotes = new Note();
            var created = worksheet.Cells[row, 3].Value?.ToString();
            var createdBy = worksheet.Cells[row, 4].Value?.ToString();
            var category = worksheet.Cells[row, 5].Value?.ToString();
            var comments = worksheet.Cells[row, 6].Value?.ToString();
            if (!string.IsNullOrEmpty(created))
            {
                preNotes.Created = created;
                preNotes.CreatedBy = createdBy;
                preNotes.Category = category;
                preNotes.Comments = comments;
                queue.ListNotes.Add(preNotes);
                row++;
            }
            else
            {
                break;
            }
        }
    }


    static void ProcessTeamMembers(PreScreeningInfo queue, ExcelWorksheet worksheet, ref int row)
    {
        row += 2;
        queue.ListTeamMembers = new List<Models.PreScreening.SubClasses.TeamMember>();
        for (int i = 0; i < maxIterations; i++)
        {
            var teamMember = new Models.PreScreening.SubClasses.TeamMember();
            var role = worksheet.Cells[row, 3].Value?.ToString();
            var team = worksheet.Cells[row, 4].Value?.ToString();
            var preparer = worksheet.Cells[row, 5].Value?.ToString();
            var dateAdded = worksheet.Cells[row, 6].Value?.ToString();
            if (!string.IsNullOrEmpty(role))
            {
                if (role == "Role" && team == "Team Member")
                {
                    row++;
                    continue;
                }
                teamMember.Role = role;
                teamMember.Name = team;
                teamMember.Preparer = preparer;
                teamMember.DateAdded = dateAdded;
                queue.ListTeamMembers.Add(teamMember);
                row++;
            }
            else
            {
                break;
            }
        }
    }

    static void ProcessAdditionalParties(PreScreeningInfo queue, ExcelWorksheet worksheet, ref int row)
    {
        row += 2;
        queue.ListAdditionalParties = new List<Models.PreScreening.SubClasses.AdditionalParty>();
        for (int i = 0; i < maxIterations; i++)
        {
            var additionalParty = new Models.PreScreening.SubClasses.AdditionalParty();
            var name = worksheet.Cells[row, 3].Value?.ToString();
            var position = worksheet.Cells[row, 4].Value?.ToString();
            var otherInfo = worksheet.Cells[row, 5].Value?.ToString();
            if (!string.IsNullOrEmpty(name))
            {
                additionalParty.Name = name;
                additionalParty.Position = position;
                additionalParty.OtherInformation = otherInfo;
                queue.ListAdditionalParties.Add(additionalParty);
                row++;
            }
            else
            {
                break;
            }
        }
    }

    static void ProcessHostileQuestion(PreScreeningInfo queue, ExcelWorksheet worksheet, ref int row)
    {
        row++;
        queue.HostileQuestion = new HostileQuestion();
        for (int i = 0; i < 3; i++)
        {
            var title = worksheet.Cells[row, 3].Value?.ToString();
            var answer = worksheet.Cells[row, 4].Value?.ToString();
            if (title == "Q. No. 1.4 Answer")
            {
                queue.HostileQuestion.Question_1_4_Answer = answer;
                row++;
            }
            else if (title == "Q. No. 1.4 Comments")
            {
                queue.HostileQuestion.Question_1_4_Comments = answer;
                row++;
            }
        }
    }

    static void ProcessLimitationsToAct(PreScreeningInfo queue, ExcelWorksheet worksheet, ref int row)
    {
        row++;
        queue.LimitationsToAct = new LimitationsToAct();
        queue.LimitationsToAct.YesNo = worksheet.Cells[row, 4].Value?.ToString();
        row++;
    }

    static void ProcessAnotherConflictCheck(PreScreeningInfo queue, ExcelWorksheet worksheet, ref int row)
    {
        row++;
        queue.AnotherConflictCheck = new AnotherConflictCheck();
        queue.AnotherConflictCheck.Comments = worksheet.Cells[row, 4].Value?.ToString();
        row++;
    }

    static void ProcessDisputeLitigationInvolvement(PreScreeningInfo queue, ExcelWorksheet worksheet, ref int row)
    {
        row++;
        queue.DisputeLitigationInvolvement = new DisputeLitigationInvolvement();
        queue.DisputeLitigationInvolvement.Comments = worksheet.Cells[row, 4].Value?.ToString();
        row++;
    }

    static void ProcessHighProfileEngagement(PreScreeningInfo queue, ExcelWorksheet worksheet, ref int row)
    {
        row++;
        queue.HighProfileEngagement = new HighProfileEngagement();
        for (int i = 0; i < 2; i++)
        {
            var title = worksheet.Cells[row, 3].Value?.ToString();
            var answer = worksheet.Cells[row, 4].Value?.ToString();
            if (title == "Yes/No?")
            {
                queue.HighProfileEngagement.YesNo = answer;
            }
            else if (title == "Comments")
            {
                queue.HighProfileEngagement.Comments = answer;
            }
            row++;
        }
    }
    static void ProcessConsentToContactCounterparty(PreScreeningInfo queue, ExcelWorksheet worksheet, ref int row)
    {
        row++;
        queue.ConsentToContactCounterparty = new ConsentToContactCounterparty();
        queue.ConsentToContactCounterparty.Comments = worksheet.Cells[row, 4].Value?.ToString();
        row++;
    }
    public static void CompareSummary(CheckerQueue SummaryCurrent, CheckerQueue SummaryPrevious, string existingFilePath)
    {
        try
        {
            excelFields = new List<string>();
            var region = SummaryCurrent.Region + "/" + SummaryCurrent.CountryName;
            AddFieldIfNotMatch(region, SummaryPrevious.Region, "C5");
            AddFieldIfNotMatch(SummaryCurrent.ClientName, SummaryPrevious.ClientName, "C6");
            AddFieldIfNotMatch(SummaryCurrent.EngagementName, SummaryPrevious.EngagementName, "C7");
            AddFieldIfNotMatch(SummaryCurrent.SubServiceLine, SummaryPrevious.SubServiceLine, "C8");
            AddFieldIfNotMatch(SummaryCurrent.Services, SummaryPrevious.Services, "C9");
            AddFieldIfNotMatch(SummaryCurrent.SubTypeofCheck, SummaryPrevious.SubTypeofCheck, "C11");
            AddFieldIfNotMatch(SummaryCurrent.AttachmentsinPACE, SummaryPrevious.AttachmentsinPACE, "C12");
            AddFieldIfNotMatch(SummaryCurrent.ConfidentialPace, SummaryPrevious.ConfidentialPace, "C13");
            AddFieldIfNotMatch(SummaryCurrent.Contractuallylimit, SummaryPrevious.Contractuallylimit, "C14");
            AddFieldIfNotMatch(SummaryCurrent.DisputeLitigation, SummaryPrevious.DisputeLitigation, "C15");
            AddFieldIfNotMatch(SummaryCurrent.Hostile, SummaryPrevious.Hostile, "C16");
            AddFieldIfNotMatch(SummaryCurrent.Auction, SummaryPrevious.Auction, "C17");
            AddFieldIfNotMatch(SummaryCurrent.AuditPartner, SummaryPrevious.AuditPartner, "C18");
            AddFieldIfNotMatch(SummaryCurrent.GovtEntity, SummaryPrevious.GovtEntity, "C20");
            AddFieldIfNotMatch(SummaryCurrent.onShore, SummaryPrevious.onShore, "C22");
            AddFieldIfNotMatch(SummaryCurrent.EngagementDesc, SummaryPrevious.EngagementDesc, "D5");
            if (excelFields.Any())
            {
                using (var package = new ExcelPackage(new FileInfo(existingFilePath)))
                {
                    var worksheet = package.Workbook.Worksheets.Where(x => x.Name == Constants.CAUConstants.MASTER_WORKBOOK_SUMMARY_TAB).FirstOrDefault();
                    if (worksheet == null)
                        return;
                    var yellowFill = worksheet.Workbook.Styles.CreateNamedStyle("YellowFill");
                    yellowFill.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    yellowFill.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    yellowFill.Style.WrapText = true;
                    yellowFill.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    yellowFill.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    yellowFill.Style.Font.Name = "EYInterstate";
                    yellowFill.Style.Font.Size = 8;
                    yellowFill.Style.Border.Top.Style = yellowFill.Style.Border.Left.Style = yellowFill.Style.Border.Bottom.Style = yellowFill.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    foreach (var cellAddress in excelFields)
                    {
                        var cell = worksheet.Cells[cellAddress];
                        cell.StyleName = "YellowFill";
                    }
                    package.Save();
                }
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
    }
    static void AddFieldIfNotMatch(string current, string previous, string cellAddress)
    {
        if (!string.Equals(current?.Trim(), previous?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            excelFields.Add(cellAddress);
        }
    }
}

#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CA1860 // Avoid using 'Enumerable.Any()' extension method
#pragma warning restore IDE0057 // Use range operator
#pragma warning restore IDE0017 // Simplify object initialization
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0028 // Simplify collection initialization
#pragma warning restore IDE0063 // Use simple 'using' statement
#pragma warning restore CA2211 // Non-constant fields should not be visible
#pragma warning restore IDE0018 // Inline variable declaration
#pragma warning restore CA1806 // Do not ignore method results
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
