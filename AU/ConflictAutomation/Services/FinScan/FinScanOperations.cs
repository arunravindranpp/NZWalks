using ConflictAutomation.Extensions;
using ConflictAutomation.Models;
using ConflictAutomation.Models.FinScan;
using ConflictAutomation.Models.FinScan.SubClasses;
using ConflictAutomation.Models.FinScan.SubClasses.enums;
using ConflictAutomation.Utilities;
using ConflictAutomation.Utilities.ExcelFileEmbedder.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using Log = Serilog.Log;

namespace ConflictAutomation.Services.FinScan;

public static class FinScanOperations
{
    private const string MSG_NO_DATA = "-";  // "NA";

    private static readonly Color darkGray = Color.FromArgb(191, 191, 191);
    private static readonly Color brightTeal = Color.FromArgb(183, 222, 232);
    private static readonly Color black = ColorTranslator.FromHtml("#000000");

    private static FinScanListProfileReport _finScanListProfileReportConsolidated;
    private static FinScanMatchReport _finScanMatchReportConsolidated;


    public static List<UnitGrid_FinscanEntity> ProcessFinScanCheckForMultipleKeywords(
                    ResearchSummary rs, List<string> keywordsList,
                    Func<SearchMatch, bool> searchMatchFilter, int finScanMatchesThreshold,
                    string masterWorkbookFilePath, string additionalInfoOnError)
    {
        List<UnitGrid_FinscanEntity> lsUnitGrid_FinscanEntity = [];

        _finScanListProfileReportConsolidated = new(searchMatchFilter, finScanMatchesThreshold);
        _finScanMatchReportConsolidated = new();


        int keywordIndex = 0;
        string keyword;
        while (keywordIndex < keywordsList.Count)
        {
            keyword = keywordsList.ElementAt(keywordIndex);

            List<string> additionalKeywords = [];

            List<UnitGrid_FinscanEntity> resultsFromAFinScanCheck =
                ProcessFinScanCheckForSingleKeyword(
                    ConflictService.IndividualOrEntity, keyword, rs.Country,
                    searchMatchFilter, finScanMatchesThreshold,
                    out additionalKeywords, additionalInfoOnError);
            lsUnitGrid_FinscanEntity.AddRange(resultsFromAFinScanCheck);

            if (lsUnitGrid_FinscanEntity.IsError())
            {
                return lsUnitGrid_FinscanEntity;
            }

            if (!additionalKeywords.IsNullOrEmpty())
            {
                // The list of additional keywords may have been extended for Parent Companies or Employers.
                // If those keywords (with or without prefix "...#") are already present in the queue,
                // they must NOT be added again, to avoid reprocessing of keywords which could lead to infinite loops.
                // Each additional keyword is a string with the following structure:
                //   The parent relationship and the keyword,
                //   separated by "#::" (FinScanConstants.ParentRelationship_SEPARATOR),
                //   e.g. "Parent Company#::ABC Company" or "Employer#::XYZ Company".

                // In the dictionaryNewKeywords being created below from additionalKeywords,
                // each KeyValuePair<string, string> has the following structure:
                //   - Key:   The keyword alone (used as a key, to avoid reprocessing); 
                //   - Value: The parent relationship and the keyword,
                //            separated by "#::" (FinScanConstants.ParentRelationship_SEPARATOR),
                //            e.g. "Parent Company#::ABC Company" or "Employer#::XYZ Company".
                Dictionary<string, string> dictionaryNewKeywords = new(
                    additionalKeywords.Select(additionalKeyword =>
                        new KeyValuePair<string, string>(
                            additionalKeyword.StrRight(FinScanConstants.ParentRelationship_SEPARATOR),
                            additionalKeyword)));

                keywordsList.AppendValuesOfNewKeyValuePairsOnly(dictionaryNewKeywords);
            }

            keywordIndex++;
        }

        string listProfileReportConsolidatedFilePath = GenerateConsolidatedListProfileReport(
                                         masterWorkbookFilePath, tabName: rs.WorksheetNo);
        if (!listProfileReportConsolidatedFilePath.IsNullOrEmpty())
        {
            ReplaceFirstListProfileReportFilePath(lsUnitGrid_FinscanEntity, listProfileReportConsolidatedFilePath);
        }

        string matchReportFilePath = GenerateConsolidatedMatchReport(
                                        masterWorkbookFilePath, tabName: rs.WorksheetNo);
        if (!matchReportFilePath.IsNullOrEmpty())
        {
            ReplaceFirstMatchReportFilePath(lsUnitGrid_FinscanEntity, matchReportFilePath);
        }

        return lsUnitGrid_FinscanEntity;
    }


    public static bool IsError(this List<UnitGrid_FinscanEntity> lsUnitGrid_FinscanEntity) =>
        lsUnitGrid_FinscanEntity.Any(unitGrid_FinscanEntity => unitGrid_FinscanEntity.IsError());


    private static bool IsError(this UnitGrid_FinscanEntity unitGrid_FinscanEntity) => 
        unitGrid_FinscanEntity.Comments.Contains(FinScanConstants.MSG_SEARCH_API_ERROR, StringComparison.OrdinalIgnoreCase);


    private static void ReplaceFirstListProfileReportFilePath(List<UnitGrid_FinscanEntity> lsUnitGrid_FinscanEntity, string listProfileReportFilePath)
    {
        if (lsUnitGrid_FinscanEntity.IsNullOrEmpty())
            return;

        var firstUnitGrid_FinscanEntity = lsUnitGrid_FinscanEntity.First();
        if (firstUnitGrid_FinscanEntity is null)
        {
            return;
        }

        var remainingUnitGrid_FinscanEntity = lsUnitGrid_FinscanEntity.Skip(1).ToList();

        firstUnitGrid_FinscanEntity.ListProfileReportFilePath = listProfileReportFilePath;

        lsUnitGrid_FinscanEntity.Clear();
        lsUnitGrid_FinscanEntity.Add(firstUnitGrid_FinscanEntity);
        if (!remainingUnitGrid_FinscanEntity.IsNullOrEmpty())
        {
            lsUnitGrid_FinscanEntity.AddRange(remainingUnitGrid_FinscanEntity);
        }
    }


    private static void ReplaceFirstMatchReportFilePath(List<UnitGrid_FinscanEntity> lsUnitGrid_FinscanEntity, string matchReportReportFilePath)
    {
        if (lsUnitGrid_FinscanEntity.IsNullOrEmpty())
            return;

        var firstUnitGrid_FinscanEntity = lsUnitGrid_FinscanEntity.First();
        if (firstUnitGrid_FinscanEntity is null)
        {
            return;
        }

        var remainingUnitGrid_FinscanEntity = lsUnitGrid_FinscanEntity.Skip(1).ToList();

        firstUnitGrid_FinscanEntity.MatchReportFilePath = matchReportReportFilePath;

        lsUnitGrid_FinscanEntity.Clear();
        lsUnitGrid_FinscanEntity.Add(firstUnitGrid_FinscanEntity);
        if (!remainingUnitGrid_FinscanEntity.IsNullOrEmpty())
        {
            lsUnitGrid_FinscanEntity.AddRange(remainingUnitGrid_FinscanEntity);
        }
    }


    private static List<UnitGrid_FinscanEntity> ProcessFinScanCheckForSingleKeyword(
                        SearchTypeEnum individualOrEntity,
                        string keyword, string countryName,
                        Func<SearchMatch, bool> searchMatchFilter, int finScanMatchesThreshold,
                        out List<string> additionalKeywords, string additionalInfoOnError)
    {
        List<UnitGrid_FinscanEntity> results = [];

        var resultFromAFinScanCheck = ProcessFinScanCheckForSingleKeywordAndGivenSearchType(
                                        individualOrEntity, ClientSearchCodeEnum.FullName,
                                        keyword, countryName,
                                        searchMatchFilter, finScanMatchesThreshold,
                                        out additionalKeywords, additionalInfoOnError);
        if (resultFromAFinScanCheck is not null)
        {
            results.Add(resultFromAFinScanCheck);

        }

        // At this point, there once was a similar call to a FinScan Partial Search,
        // but it was removed according to the new requirements requested by business team.

        return results;
    }


    private static UnitGrid_FinscanEntity ProcessFinScanCheckForSingleKeywordAndGivenSearchType(
        SearchTypeEnum individualOrEntity, ClientSearchCodeEnum fullOrPartialName,
        string keyword, string countryName,
        Func<SearchMatch, bool> searchMatchFilter, int finScanMatchesThreshold,
        out List<string> additionalKeywords, string additionalInfoOnError)
    {
        try
        {
            string relationship;

            if (keyword.IsParentCompany() || keyword.IsEmployerCompany())
            {
                relationship = keyword.StrLeft(FinScanConstants.ParentRelationship_SEPARATOR);
                keyword = keyword.StrRight(FinScanConstants.ParentRelationship_SEPARATOR);
            }
            else
            {
                relationship = string.Empty;
            }

            string individualOrEntityStr = individualOrEntity == SearchTypeEnum.Individual ? FinScanConstants.MSG_INDIVIDUAL :
                                                                    FinScanConstants.MSG_ENTITY;
            string fullOrPartialNameStr = fullOrPartialName == ClientSearchCodeEnum.FullName ? FinScanConstants.MSG_FULL_NAME :
                                                                    FinScanConstants.MSG_PARTIAL_NAME;
            WriteLog($"Creating FinScan Request for {individualOrEntityStr}, keyword '{keyword}'");
            FinScanRequest finScanRequest = Program.FinScanRequestFactory.MakeRequest(
                                                individualOrEntity, fullOrPartialName,
                                                Program.FinScanSearch.GetNextClientId(),
                                                keyword, countryName);

            string timeStamp = DateTime.Now.TimestampWithTimezoneFromLocal();

            FinScanResponse finScanResponse;

            WriteLog("Calling FinScanSearchAPI");
            try
            {
                finScanResponse = Program.FinScanSearch.Execute(finScanRequest, $"{additionalInfoOnError} - Keyword: \"{keyword}\"");
            }
            catch (Exception ex)
            {
                WriteLog($"Call to FinScanSearchAPI failed {additionalInfoOnError}".FullTrim());  // Writes log entry to .txt Log file
                LoggerInfo.LogException(ex, additionalInfoOnError);  // Writes log entry to SQL table CAU_ExceptionLog

                additionalKeywords = null;

                string shortErrorMessage = $"{FinScanConstants.MSG_SEARCH_API_ERROR}";
                string fullErrorMessage = $"{shortErrorMessage} {new string(' ', 30)}" +
                                          $"(finScanRequest.clientId = {finScanRequest.clientId})";
                UnitGrid_FinscanEntity resultOnError = new()
                {
                    KeywordUsed = keyword,
                    SearchType = fullOrPartialNameStr,
                    ActualMatches = 0,
                    Relationship = string.Empty,
                    Sanction = string.Empty,
                    Source = string.Empty,
                    Comments = shortErrorMessage,
                    CommentsInDetailsSection = fullErrorMessage,
                    ListProfileReportFilePath = string.Empty,
                    MatchReportFilePath = string.Empty,
                    TargetTabName = string.Empty
                };

                return resultOnError;
            }


            string logEntry = "FinScanSearchAPI returned a Response";
            if (finScanResponse != null && finScanResponse.status == ResultTypeEnum.PASS)
            {
                logEntry += $" with status={(int)finScanResponse.status} (PASSED)";
                WriteLog(logEntry);
            }
            else
            {
                logEntry += $" with (status={(int)finScanResponse.status}" +
                            $"; code={finScanResponse.code} " +
                            $"; message='{finScanResponse.message}')";
                WriteLog(logEntry);

                throw new FinScanException(finScanResponse.status, finScanResponse.code,
                                           finScanResponse.message, logEntry);
            }

            string comments = $"{timeStamp} - No match";

            List<SearchMatch> allSearchMatches;
            List<SearchMatch> searchMatchesInRange;
            int searchMatchesInRangeCount;

            if (finScanResponse.HasPossibleMatches())
            {
                allSearchMatches = finScanResponse.searchResults.First().searchMatches;
                searchMatchesInRange = allSearchMatches.Where(searchMatchFilter).ToList();
                searchMatchesInRangeCount = searchMatchesInRange.Count;

                FinScanListProfileReport finScanListProfileReport = new(searchMatchFilter, finScanMatchesThreshold);
                finScanListProfileReport.AppendItemsFrom(keyword, finScanResponse);
                if (searchMatchesInRangeCount > 0)
                {
                    comments = $"{timeStamp} - {searchMatchesInRangeCount} match{(searchMatchesInRangeCount > 1 ? "es" : string.Empty)}";

                    _finScanListProfileReportConsolidated.AddReport(finScanListProfileReport);
                }
            }
            else
            {
                allSearchMatches = [];
                searchMatchesInRange = [];
                searchMatchesInRangeCount = 0;
            }

            WriteLog("finScanResponse --> " +
                $"{(searchMatchesInRangeCount > 0 ? searchMatchesInRangeCount : "No")}" +
                $" match{(searchMatchesInRangeCount > 1 ? "es" : string.Empty)} found");

            FinScanParser allSearchMatchesFinScanParser = new(finScanRequest, allSearchMatches);
            FinScanMatchReport finScanMatchReport = allSearchMatchesFinScanParser.GetMatchReport();
            _finScanMatchReportConsolidated.AddReport(finScanMatchReport);

            FinScanParser searchMatchesInRangeFinScanParser = new(finScanRequest, searchMatchesInRange);
            bool? isSanctioned = searchMatchesInRangeFinScanParser.AtLeastOneSearchMatchIsSanctioned(out additionalKeywords);

            LogPossibleAdditionalKeywords(additionalKeywords);                

            UnitGrid_FinscanEntity result = new()
            {
                KeywordUsed = keyword,
                SearchType = fullOrPartialNameStr,
                ActualMatches = searchMatchesInRangeCount,
                Relationship = relationship,
                Sanction = ReportedSanction(searchMatchesInRangeCount, isSanctioned),
                Source = FinScanConstants.MSG_APG,
                Comments = ReportedComments(searchMatchesInRangeCount, isSanctioned, keyword),
                CommentsInDetailsSection = ReportedCommentsInDetailsSection(searchMatchesInRangeCount, isSanctioned),

                // To be filled in by the topest-level caller, ProcessFinScanCheckForMultipleKeywords(), 
                // after the consolidated output report and the consolidated match report are generated.
                ListProfileReportFilePath = string.Empty,
                MatchReportFilePath = string.Empty,
                TargetTabName = string.Empty
            };

            return result;            
        }        
        catch (Exception ex)
        {
            WriteLog($"Call to method ProcessFinScanCheckForSingleKeywordAndGivenSearchType() failed - {additionalInfoOnError}");  // Writes log entry to .txt Log file
            LoggerInfo.LogException(ex, additionalInfoOnError);  // Writes log entry to SQL table CAU_ExceptionLog

            additionalKeywords = null;

            return null;
        }
    }


    private static void LogPossibleAdditionalKeywords(List<string> additionalKeywords)
    {
        if (!additionalKeywords.IsNullOrEmpty())
        { 
            foreach (string additionalKeyword in additionalKeywords)
            {
                WriteLog($"Additional keyword: '{additionalKeyword}'");
            }
        }
        Console.WriteLine();
    }


    private static void WriteLog(string logEntry)
    {
        string text = logEntry.Replace("\n", string.Empty);
        if(!text.FullTrim().IsNullOrEmpty())
        {
            Log.Information(text);
        }
        Console.WriteLine($"     {logEntry}");
    }


    

    private static string ReportedSanction(int actualMatches, bool? isSanctioned) => 
        (actualMatches < 1) ? string.Empty :
        (!isSanctioned.HasValue) ? string.Empty :
        ((bool)isSanctioned ? FinScanConstants.MSG_SANCTIONED : FinScanConstants.MSG_NOT_SANCTIONED);


    private static string ReportedComments(int actualMatches, bool? isSanctioned, string keyword) => 
        (actualMatches < 1) ? $"{FinScanConstants.MSG_NO_RESULTS_FOR} «{keyword}»" :
        (!isSanctioned.HasValue) ? FinScanConstants.MSG_NOT_SANCTIONED_CHECK_MANUALLY :
        ((bool)isSanctioned ? string.Empty : FinScanConstants.MSG_NOT_SANCTIONED_CHECK_MANUALLY);


    private static string ReportedCommentsInDetailsSection(int actualMatches, bool? isSanctioned) => 
        (actualMatches < 1) ? $"{FinScanConstants.MSG_NO_RESULTS_FOR} {FinScanConstants.MSG_SEARCH}. " :
        (!isSanctioned.HasValue) ? FinScanConstants.MSG_NOT_SANCTIONED_CHECK_MANUALLY :
        ((bool)isSanctioned ? FinScanConstants.MSG_REFER_TO_THE_ATTACHMENTS : FinScanConstants.MSG_NOT_SANCTIONED_CHECK_MANUALLY);


    private static bool IsParentCompany(this string keyword) => 
        keyword.StartsWith(ParentCompanyPattern, StringComparison.OrdinalIgnoreCase);

    public static string ParentCompanyPattern =>
        $"{FinScanConstants.ParentRelationship_PARENT_COMPANY}{FinScanConstants.ParentRelationship_SEPARATOR}";


    private static bool IsEmployerCompany(this string keyword) => 
        keyword.StartsWith(EmployerCompanyPattern, StringComparison.OrdinalIgnoreCase);


    public static string EmployerCompanyPattern => 
        $"{FinScanConstants.ParentRelationship_EMPLOYER}{FinScanConstants.ParentRelationship_SEPARATOR}";


    private static string GenerateConsolidatedListProfileReport(string masterWorkbookFilePath, string tabName)
    {
        string outputFilePath = OutputFilePath(masterWorkbookFilePath, tabName);

        FinScanListProfileReportExcelFormatter finScanListProfileReportExcelFormatter =
            new(outputFilePath, _finScanListProfileReportConsolidated);
        return finScanListProfileReportExcelFormatter.Run();
    }


    private static string GenerateConsolidatedMatchReport(string masterWorkbookFilePath, string tabName)
    {
        string matchReportTemplateFilePath = MatchReportTemplateFilePath();
        string matchReportFilePath = MatchReportFilePath(masterWorkbookFilePath, tabName);

        FinScanMatchReportExcelFormatter finScanMatchReportExcelFormatter =
            new(matchReportTemplateFilePath);
        return finScanMatchReportExcelFormatter.Run(matchReportFilePath, _finScanMatchReportConsolidated);
    }


    private static string OutputFilePath(string masterWorkbookFilePath, string tabName) =>
        BuildFilePath(masterWorkbookFilePath, tabName, FinScanConstants.OUTPUT_REPORT_SUFFIX_AND_EXTENSION);

    private static string MatchReportTemplateFilePath() =>
        Path.Combine(Directory.GetCurrentDirectory(), Program.FinScanMatchReportTemplateFilename);


    private static string MatchReportFilePath(string masterWorkbookFilePath, string tabName) =>
        BuildFilePath(masterWorkbookFilePath, tabName, FinScanConstants.MATCH_REPORT_SUFFIX_AND_EXTENSION);


    private static string BuildFilePath(string sFilePath, string tabName, string fileSuffixAndExtension)
    {
        string folderPath = $"{Path.GetDirectoryName(sFilePath)}\\{tabName}\\FinScan";
        FileSystem.EnsureFolderExists(folderPath);

        string fileName = $"{Path.GetFileNameWithoutExtension(sFilePath).StrRight("_")}_{tabName}{fileSuffixAndExtension}";
        
        string fullFilePath = Path.Combine(folderPath, fileName);

        return fullFilePath;
    }


    public static List<FileEmbedding> ConvertToListFileEmbedding(
        List<UnitGrid_FinscanEntity> lsUnitGrid_FinscanEntity,
        string targetSheetName, int startingRow)
    {
        if (lsUnitGrid_FinscanEntity.IsNullOrEmpty())
        {
            return null;
        }

        List<FileEmbedding> results = [];

        int row = startingRow + 1;
        foreach (var unitGrid_FinscanEntity in lsUnitGrid_FinscanEntity)
        {
            if (!string.IsNullOrEmpty(unitGrid_FinscanEntity.ListProfileReportFilePath))
            {
                results.Add(new FileEmbedding(
                                  unitGrid_FinscanEntity.ListProfileReportFilePath, targetSheetName, $"A{row}"));
                row += 5;
            }

            if (!string.IsNullOrEmpty(unitGrid_FinscanEntity.MatchReportFilePath))
            {
                results.Add(new FileEmbedding(
                                  unitGrid_FinscanEntity.MatchReportFilePath, targetSheetName, $"A{row}"));
                row += 5;
            }
            
            row += 1;
        }

        return results;
    }


    public static void WriteFinScanSummaryGridTitle(ExcelWorksheet worksheet, int row, string title)
    {
        worksheet.Cells[$"A{row}"].Value = title;
        worksheet.Cells[$"A{row}"].Style.Font.Color.SetColor(Color.White);
        worksheet.Cells[$"A{row}"].Style.Font.Bold = true;
        worksheet.Cells[$"A{row}"].Style.Font.Size = 9;
        
        worksheet.Cells[$"A{row}:D{row}"].Merge = true;

        var range = worksheet.Cells[$"A{row}:H{row}"];
        range.SetBackgroundColor(black);
        range.SetBorders(ExcelBorderStyle.Thin, ExcelBorderStyle.Thin);
    }


    public static int WriteFinScanSummaryGridBodyWithSingleMessage(ExcelWorksheet worksheet, int row, string text)
    {
        worksheet.Cells[$"A{row}"].Value = text;

        var range = worksheet.Cells[$"A{row}:H{row}"];
        range.Merge = true;
        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        range.SetBackgroundColor(brightTeal);
        range.SetBorders(ExcelBorderStyle.None, ExcelBorderStyle.Thin);
        range.AutoFitColumns(21, 35);

        return row;
    }


    public static int WriteFinScanSummaryGridBodyWithData(ExcelWorksheet worksheet, int row, List<UnitGrid_FinscanEntity> lsUnitGrid_FinscanEntity)
    {
        WriteFinScanSummaryGridColumnHeaders(worksheet, row);

        foreach (UnitGrid_FinscanEntity unitGrid_FinscanEntity in lsUnitGrid_FinscanEntity)
        {
            row++;
            worksheet.Cells[$"A{row}"].Value = GetContentsOrNaIfEmpty(unitGrid_FinscanEntity.KeywordUsed);
            worksheet.Cells[$"B{row}"].Value = GetContentsOrNaIfEmpty(unitGrid_FinscanEntity.SearchType);
            worksheet.Cells[$"C{row}"].Value = GetContentsOrNaIfEmpty(unitGrid_FinscanEntity.Relationship);
            worksheet.Cells[$"D{row}"].Value = GetContentsOrNaIfEmpty(unitGrid_FinscanEntity.Sanction);
            worksheet.Cells[$"E{row}"].Value = GetContentsOrNaIfEmpty(unitGrid_FinscanEntity.Source);
            worksheet.Cells[$"F{row}"].Value = GetContentsOrNaIfEmpty(unitGrid_FinscanEntity.Comments);
            worksheet.Cells[$"F{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            var range = worksheet.Cells[$"A{row}:H{row}"];
            range.AutoFitColumns(21, 35);
            range.SetBackgroundColor(brightTeal);
            range.SetBorders(ExcelBorderStyle.Thin, ExcelBorderStyle.Thin);
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            worksheet.Cells[$"F{row}:H{row}"].Merge = true;

            worksheet.AutoFitRowHeight(row, startCol: 'A', endCol: 'H', allMergedCols: [('F', 'H')]);
        }

        return row;
    }


    public static void WriteFinScanResultsSection(ExcelWorksheet worksheet, int startingRow,
        List<UnitGrid_FinscanEntity> lsUnitGrid_FinscanEntity)
    {
        if (lsUnitGrid_FinscanEntity is null || lsUnitGrid_FinscanEntity.Count == 0)
        {
            return;
        }

        int row = startingRow + 1;
        foreach (var unitGrid_FinscanEntity in lsUnitGrid_FinscanEntity)
        {
            worksheet.Cells[$"B{row}"].Value = unitGrid_FinscanEntity.KeywordUsed;
            worksheet.Cells[$"B{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"B{row + 1}"].Value = unitGrid_FinscanEntity.SearchType;
            worksheet.Cells[$"B{row + 1}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[$"B{row + 2}"].Value = unitGrid_FinscanEntity.CommentsInDetailsSection;
            worksheet.Cells[$"B{row + 2}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            row += 5;
        }
    }


    private static void WriteFinScanSummaryGridColumnHeaders(ExcelWorksheet worksheet, int row)
    {
        worksheet.Cells[$"A{row}"].Value = "Keyword used";
        worksheet.Cells[$"B{row}"].Value = "Search Type";
        worksheet.Cells[$"C{row}"].Value = "Relationship";
        worksheet.Cells[$"D{row}"].Value = "Sanction";
        worksheet.Cells[$"E{row}"].Value = "Source";
        worksheet.Cells[$"F{row}"].Value = "Comments";
        worksheet.Cells[$"F{row}:H{row}"].Merge = true;
        
        var range = worksheet.Cells[$"A{row}:H{row}"];
        range.SetBackgroundColor(darkGray);
        range.SetBorders(ExcelBorderStyle.Thin, ExcelBorderStyle.Thin);
        range.AutoFitColumns(21, 35);
    }


    public static bool IsSanctioned(List<UnitGrid_FinscanEntity> listUnitGrid_FinscanEntity)
    {
        if(listUnitGrid_FinscanEntity.IsNullOrEmpty())
        {
            return false;
        }

        bool result = listUnitGrid_FinscanEntity.Any(unitGrid_FinscanEntity =>
                        unitGrid_FinscanEntity.Sanction.Equals(FinScanConstants.MSG_SANCTIONED, 
                                                               StringComparison.OrdinalIgnoreCase));

        return result;
    }


    public static void SetFinScanAttachmentsFilePaths(ResearchSummary rs, List<UnitGrid_FinscanEntity> lsUnitGrid_FinscanEntity)
    {
        ArgumentNullException.ThrowIfNull(rs);

        rs.FinScanListProfileReportFilePath = string.Empty;
        rs.FinScanMatchReportFilePath = string.Empty;

        if (lsUnitGrid_FinscanEntity.IsNullOrEmpty())
        {
            return;
        }

        UnitGrid_FinscanEntity firstUnitGrid_FinscanEntity = lsUnitGrid_FinscanEntity.First();
        if (!string.IsNullOrWhiteSpace(firstUnitGrid_FinscanEntity.ListProfileReportFilePath))
        {
            rs.FinScanListProfileReportFilePath = firstUnitGrid_FinscanEntity.ListProfileReportFilePath;
        }

        if (!string.IsNullOrWhiteSpace(firstUnitGrid_FinscanEntity.MatchReportFilePath))
        {
            rs.FinScanMatchReportFilePath = firstUnitGrid_FinscanEntity.MatchReportFilePath;
        }
    }


    private static string GetContentsOrNaIfEmpty(string text) =>
        string.IsNullOrWhiteSpace(text) ? MSG_NO_DATA : text;
}
