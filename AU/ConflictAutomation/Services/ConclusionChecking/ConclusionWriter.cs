using ConflictAutomation.Constants;
using ConflictAutomation.Extensions;
using ConflictAutomation.Models;
using ConflictAutomation.Models.ConclusionChecking;
using ConflictAutomation.Models.ResearchSummaryEngine;
using ConflictAutomation.Services.ConclusionChecking.enums;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PACE;
using PACE.Domain.Services;
using RSE = ConflictAutomation.Services.ResearchSummaryEngine;

namespace ConflictAutomation.Services.ConclusionChecking;

public class ConclusionWriter(Conclusion conclusion,
    string entityOrIndividualName, string gcoTeam, string rmContactNames)
{
    // Important Note from Manisha Viswanathan to Dev team on 2024-06-06: 
    // "- Scenario 1 - AU concludes the check:
    //    AU is to remove the pre-existing conclusion and attachment(s) as well as any condition(s)
    //    or disclaimer(s) related to the previous check from the Conclusion section in PACE and
    //    replace it with the conclusion and attachment and any condition(s) / disclaimer(s)
    //    for the current check.
    //  
    //  - Scenario 2 - AU cannot conclude the check:
    //    AU should NOT remove any pre-existing conclusion, attachment(s), condition(s) or disclaimer(s)
    //    related to the previous check from the Conclusion section in PACE (i.e., AU is to leave
    //    everything in PACE)."
    //  
    //  Note: In both scenarios, AU should NOT delete any pre-existing Research Template attachment(s)
    //        within the 'Research Form' section of the conflict check in PACE."  

    private const string SUMMARY_CELL_CASE_OPTION = "C30";
    private const string SUMMARY_CELL_ISSUE_DETAILS = "D31";
    private const string SUMMARY_CELL_CONCLUSION_OPTION = "C32";
    private const string SUMMARY_CELL_CONCLUSION_DETAILS = "D32";
    private const string SUMMARY_CELL_CONDITION_1_DETAILS = "D33";

    private const string PLACEHOLDER_NAME = "<NAME>";
    private const string PLACEHOLDER_GCOTEAM = "<GCOTEAM>";
    private const string PLACEHOLDER_RMCONTACTNAME = "<RMCONTACTNAME>";    

    private const int SUBSTATUS_ID_CHECK_PENDING_FOR_REVIEW = 7;
    
    // Create sub-status 'Check pre-screened' on PACE PROD just prior to AU launch. Already created it on UAT.
    private const int SUBSTATUS_ID_CHECK_PRE_SCREENED = 13;

    // Create sub-status 'Research completed' on PACE PROD just prior to AU launch. Already created it on UAT.
    private const int SUBSTATUS_ID_RESEARCH_COMPLETED = 14;

    private const string CONDITION_SANCTIONS_MATCH = "Sanctions match";
    private const string CONDITION_CONFLICT_TYPE = "Conflict";

    private const int DISCLAIMER_PURSUIT_ID = 1;
    private const string DISCLAIMER_PURSUIT_LABEL = "Pursuit check";
    private const string DISCLAIMER_PURSUIT_DESCRIPTION = "This pursuit check does not give you permission to start the engagement. The conclusion is a preliminary indication of the potential conflicts with the parties involved. The final conflicts clearance can only be provided once you submit a full conflict check with detailed service description and names of all the parties involved (including the client) and includes GDS consultation with the affected parties.";
    
    private const int DISCLAIMER_INDEPENDENCE_ID = 2;
    private const string DISCLAIMER_INDEPENDENCE_LABEL = "Independence";

    private readonly Conclusion _conclusion = conclusion;
    private readonly string _entityOrIndividualName = entityOrIndividualName;
    private readonly string _gcoTeam = gcoTeam;
    private readonly string _rmContactNames = rmContactNames;


    public void UpdateExcel(string masterWorkbookFullPath, CheckerQueue summary)
    {
        switch (_conclusion.Scenario)
        {
            case ConclusionScenarioEnum.NoSanctions:                     // Scenario #1
            case ConclusionScenarioEnum.ClientSideSanctions_MainRoles:   // Scenario #2
            case ConclusionScenarioEnum.ClientSideSanctions_OtherRoles:  // Scenario #3
                                                                         //   Scenario #3 never happens, because
                                                                         //   there is always a Main Client;
                                                                         //   but it is here for completeness.
            case ConclusionScenarioEnum.NonClientSideSanctions:          // Scenario #4
                UpdateExcelForIdentifiableScenarios(masterWorkbookFullPath, summary);
                break;

            case ConclusionScenarioEnum.Unidentified:
                UpdateExcelForUnidentifiedScenario(masterWorkbookFullPath);
                break;

            default:
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                throw new ArgumentOutOfRangeException(
                            nameof(_conclusion.Scenario), _conclusion.Scenario,
                            CAUConstants.MSG_INVALID_VALUE_FOR_CONCLUSION_SCENARIO);
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
        }
    }


    private void UpdateExcelForIdentifiableScenarios(string masterWorkbookFullPath, CheckerQueue summary)
    {
#pragma warning disable IDE0063 // Use simple 'using' statement
        using (ExcelPackage package = new(masterWorkbookFullPath))
        {
            ExcelWorksheet summaryWorksheet = package.GetWorksheet(CAUConstants.MASTER_WORKBOOK_SUMMARY_TAB);
            ArgumentNullException.ThrowIfNull(summaryWorksheet, nameof(summaryWorksheet));

            summaryWorksheet.Cells[SUMMARY_CELL_CASE_OPTION].Value = _conclusion.Case;
            summaryWorksheet.Cells[SUMMARY_CELL_ISSUE_DETAILS].Value = _conclusion.Issue;
            summaryWorksheet.Cells[SUMMARY_CELL_CONCLUSION_OPTION].Value = _conclusion.Outcome;            

            string conclusionDetails = _conclusion.DetailsTemplateString();
            if(summary.IsPursuit() && (!summary.IsPursuitException()))
            {
                conclusionDetails += $"\n\n{DISCLAIMER_PURSUIT_DESCRIPTION}";
            }
            summaryWorksheet.Cells[SUMMARY_CELL_CONCLUSION_DETAILS].WriteMultipleLines(ApplyValuesToTemplateString(conclusionDetails));
            summaryWorksheet.Cells[SUMMARY_CELL_CONCLUSION_DETAILS].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            summaryWorksheet.Cells[SUMMARY_CELL_CONCLUSION_DETAILS].EntireRow.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

            summaryWorksheet.Cells[SUMMARY_CELL_CONDITION_1_DETAILS].WriteMultipleLines(ApplyValuesToTemplateString(_conclusion.ConditionDescription));
            summaryWorksheet.Cells[SUMMARY_CELL_CONDITION_1_DETAILS].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            summaryWorksheet.Cells[SUMMARY_CELL_CONDITION_1_DETAILS].EntireRow.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

            ConclusionAutoFitRows(summaryWorksheet);

            package.Save();
        }
#pragma warning restore IDE0063 // Use simple 'using' statement
    }


    private void UpdateExcelForUnidentifiedScenario(string masterWorkbookFullPath)
    {
#pragma warning disable IDE0063 // Use simple 'using' statement
        using (ExcelPackage package = new(masterWorkbookFullPath))
        {
            ExcelWorksheet summaryWorksheet = package.GetWorksheet(CAUConstants.MASTER_WORKBOOK_SUMMARY_TAB);
            ArgumentNullException.ThrowIfNull(summaryWorksheet, nameof(summaryWorksheet));

            summaryWorksheet.Cells[SUMMARY_CELL_CASE_OPTION].Value = _conclusion.Case;
            summaryWorksheet.Cells[SUMMARY_CELL_ISSUE_DETAILS].Value = _conclusion.Issue;
            summaryWorksheet.Cells[SUMMARY_CELL_CONCLUSION_OPTION].Value = _conclusion.Outcome;

            string conclusionDetails = _conclusion.DetailsTemplateString();
            summaryWorksheet.Cells[SUMMARY_CELL_CONCLUSION_DETAILS].WriteMultipleLines(ApplyValuesToTemplateString(conclusionDetails));
            summaryWorksheet.Cells[SUMMARY_CELL_CONCLUSION_DETAILS].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            summaryWorksheet.Cells[SUMMARY_CELL_CONCLUSION_DETAILS].EntireRow.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

            summaryWorksheet.Cells[SUMMARY_CELL_CONDITION_1_DETAILS].WriteMultipleLines(ApplyValuesToTemplateString(_conclusion.ConditionDescription));
            summaryWorksheet.Cells[SUMMARY_CELL_CONDITION_1_DETAILS].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            summaryWorksheet.Cells[SUMMARY_CELL_CONDITION_1_DETAILS].EntireRow.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

            ConclusionAutoFitRows(summaryWorksheet);

            package.Save();
        }
#pragma warning restore IDE0063 // Use simple 'using' statement
    }


    private static void ConclusionAutoFitRows(ExcelWorksheet summaryWorksheet)
    {
        summaryWorksheet.AutoFitRowHeight(rowNumber: SUMMARY_CELL_CASE_OPTION.GetRow(),
            startCol: 'B', endCol: 'J', allMergedCols: [('D', 'J')]);
        
        summaryWorksheet.AutoFitRowHeight(rowNumber: SUMMARY_CELL_ISSUE_DETAILS.GetRow(),
            startCol: 'B', endCol: 'J', allMergedCols: [('D', 'J')]);
        
        summaryWorksheet.AutoFitRowHeight(rowNumber: SUMMARY_CELL_CONCLUSION_DETAILS.GetRow(),
            startCol: 'B', endCol: 'J', allMergedCols: [('D', 'J')], forceWrapText: true,
            minHeight: string.IsNullOrWhiteSpace(summaryWorksheet.Cells[SUMMARY_CELL_CONCLUSION_OPTION].Text) 
                ? CAUConstants.STD_MIN_HEIGHT : 85.5) ;

        for(int rowNumber = SUMMARY_CELL_CONCLUSION_DETAILS.GetRow(); 
            rowNumber < SUMMARY_CELL_CONCLUSION_DETAILS.GetRow() + 4; rowNumber++)
        {
            summaryWorksheet.AutoFitRowHeight(rowNumber,
                startCol: 'B', endCol: 'J', allMergedCols: [('D', 'J')], forceWrapText: true,
                minHeight: 90);
        }
    }


    public void UpdatePACE(long conflictCheckID, 
        List<ResearchSummaryEntry> researchSummaryGrid, CheckerQueue summary)
    {
        switch (_conclusion.Scenario)
        {
            case ConclusionScenarioEnum.NoSanctions:                     // Scenario #1
            case ConclusionScenarioEnum.ClientSideSanctions_MainRoles:   // Scenario #2
            case ConclusionScenarioEnum.ClientSideSanctions_OtherRoles:  // Scenario #3
                                                                         //   Scenario #3 never happens, because
                                                                         //   there is always a Main Client;
                                                                         //   but it is here for completeness.
            case ConclusionScenarioEnum.NonClientSideSanctions:          // Scenario #4
                UpdatePaceForIdentifiableScenarios(conflictCheckID, researchSummaryGrid, summary);
                break;

            case ConclusionScenarioEnum.Unidentified:
                UpdatePaceForUnidentifiedScenario(conflictCheckID, researchSummaryGrid, summary);
                break;

            default:
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                throw new ArgumentOutOfRangeException(
                            nameof(_conclusion.Scenario), _conclusion.Scenario,
                            CAUConstants.MSG_INVALID_VALUE_FOR_CONCLUSION_SCENARIO);
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
        }
    }


    private void UpdatePaceForIdentifiableScenarios(long conflictCheckID,
        List<ResearchSummaryEntry> researchSummaryGrid, CheckerQueue summary)
    {
        ConflictCheckAttachmentUtility conflictCheckAttachmentUtility = new(Program.PACEConnectionString);

        if (Program.UpdateCAUOutputInPACE)
        { 
            if (_conclusion.Scenario != ConclusionScenarioEnum.Unidentified)
            {
                // When it is an identifiable scenario, removes previous attachments (except for the Research Template)
                conflictCheckAttachmentUtility.RemoveExistingAttachmentsExceptForResearchTemplate(conflictCheckID);
            }
        }

        var conflictCheckService = GetConflictCheckService();
        var conflictCheck = conflictCheckService.GetConflictCheckByID(conflictCheckID);

        // Always update the Sub-Status, even when the setting UpdateCAUOutputInPACE is false
        conflictCheck.SubStatusID = SUBSTATUS_ID_CHECK_PENDING_FOR_REVIEW;

        if (Program.UpdateCAUOutputInPACE)
        {
            // Remove previous Subordinate records (Disclaimers and/or Conditions)
            conflictCheck = RemovePossibleSubordinateRecords(conflictCheck);

            conflictCheck.ConflictConclusion = _conclusion.Outcome;
            // In scenario #1, the 'Rationale Instructions' are empty,
            //    so the final comment is just the 'Conflict ConclusionForNonClientSide'.
            // In scenario #4, the 'Rationale Instructions' are empty,
            //    so the final comment is just the 'Conflict ConclusionForNonClientSide', 
            //    which has a 'However' clause at its end.
            conflictCheck.ConclusionCommentsForAssessmentTeam = ApplyValuesToTemplateString(
                $"{_conclusion.ConflictConclusion}\n{_conclusion.RationaleInstructions}");
        
            // Disclaimers
            if(!string.IsNullOrEmpty(_conclusion.DisclaimerIndependence))
            {
                conflictCheck.ConflictDisclaimers.Add(
                    MakeNewDisclaimer(conflictCheckID, DISCLAIMER_INDEPENDENCE_ID, 
                        _conclusion.DisclaimerIndependence, DISCLAIMER_INDEPENDENCE_LABEL));
            }
            if(summary.IsPursuit() && (!summary.IsPursuitException()))
            {
                conflictCheck.ConflictDisclaimers.Add(
                    MakeNewDisclaimer(conflictCheckID, DISCLAIMER_PURSUIT_ID,
                        DISCLAIMER_PURSUIT_DESCRIPTION, DISCLAIMER_PURSUIT_LABEL));            
            }

            // Conditions        
            if (!string.IsNullOrEmpty(_conclusion.ConditionDescription))
            {
                conflictCheck.ConflictCheckConditions.Add(
                    MakeNewCondition(conflictCheckID, CONDITION_SANCTIONS_MATCH,
                        ApplyValuesToTemplateString(_conclusion.ConditionDescription), CONDITION_CONFLICT_TYPE));
            }

            // Research Form - Consolidated Research Results
            conflictCheck.ConsolidatedResearchResults = RSE.ResearchSummaryEngine.GetOverallSummary(
                                                            researchSummaryGrid, summary.EngagementDesc);
        }

        // Saves: Sub-Status and, optionally: 
        //                          ConclusionForNonClientSide, Disclaimers, Conditions,
        //                          Research Form - Consolidated Research Results
        var checkerGUI = Program.US.GUI;
        conflictCheckService.SaveConflictCheck(conflictCheck, checkerGUI);

        if (Program.UpdateCAUOutputInPACE) 
        {            
            if (_conclusion.Scenario != ConclusionScenarioEnum.NoSanctions)
            {
                // When there are sanctions, saves FinScan List Profile reports
                AttachFinScanListProfileReports(conflictCheckAttachmentUtility, conflictCheckID);
            }
        }
    }


    private void UpdatePaceForUnidentifiedScenario(long conflictCheckID,
        List<ResearchSummaryEntry> researchSummaryGrid, CheckerQueue summary)
    {
        var conflictCheckService = GetConflictCheckService();
        var conflictCheck = conflictCheckService.GetConflictCheckByID(conflictCheckID);

        // Sub-Status
        conflictCheck.SubStatusID = (_conclusion.ResearchFailedInAnyOfTheDataSources == true)
                                        ? SUBSTATUS_ID_CHECK_PRE_SCREENED
                                        : SUBSTATUS_ID_RESEARCH_COMPLETED;

        if (Program.UpdateCAUOutputInPACE)
        {
            // Research Form - Consolidated Research Results
            conflictCheck.ConsolidatedResearchResults = RSE.ResearchSummaryEngine.GetOverallSummary(
                                                            researchSummaryGrid, summary.EngagementDesc);
        }

        // Saves: Sub-Status and, optionally, Research Form - Consolidated Research Results
        var checkerGUI = Program.US.GUI;
        conflictCheckService.SaveConflictCheck(conflictCheck, checkerGUI);
    }


    private ConflictCheckDisclaimers MakeNewDisclaimer(long conflictCheckID, 
        int disclaimerID, string disclaimerDescription, string disclaimerLabel) => 
        new()
        {
            Id = 0,
            ConflictCheckID = conflictCheckID,
            DisclaimerId = disclaimerID,
            DisclaimerDescription = ApplyValuesToTemplateString(disclaimerDescription),
            Disclaimer = disclaimerLabel
        };


    private ConflictCheckCondition MakeNewCondition(long conflictCheckID,
        string condition, string conditionDescription, string conditionType) =>
        new()
        {
            ConflictCheckConditionID = 0,
            ConflictCheckID = conflictCheckID,
            Condition = condition,
            ConditionDescription = ApplyValuesToTemplateString(conditionDescription),
            ConditionType = conditionType
        };


    private static IConflictCheckServices GetConflictCheckService() => 
        Program._serviceProvider.GetService<IConflictCheckServices>();
  

    private void AttachFinScanListProfileReports(ConflictCheckAttachmentUtility conflictCheckAttachmentUtility, long conflictCheckID)
    {
        if (_conclusion.Attachments.IsNullOrEmpty())
        {
            return;
        }

        var conflictCheckService = GetConflictCheckService();

        foreach (var filePath in _conclusion.Attachments)
        {
            long newAttachmentID = conflictCheckAttachmentUtility.InsertAttachment(conflictCheckID, filePath);
            conflictCheckService.UpdateFileEntityToDB(newAttachmentID, conflictCheckID,
                CAUConstants.ENTITY_TYPE_ID_CONFLICT_CHECK, CAUConstants.ATTACHMENTS_FOR_ASSESSMENT_TEAM);
        }
    }


    private string ApplyValuesToTemplateString(string templateString)
    {
        string result = templateString;

        if (!string.IsNullOrWhiteSpace(_entityOrIndividualName))
        {
            result = result.Replace(PLACEHOLDER_NAME, string.IsNullOrEmpty(_entityOrIndividualName) ? "<none>" : _entityOrIndividualName);
        }
        if (!string.IsNullOrEmpty(_gcoTeam))
        {
            result = result.Replace(PLACEHOLDER_GCOTEAM, _gcoTeam);
        }
        if (!string.IsNullOrEmpty(_rmContactNames))
        {
            result = result.Replace(PLACEHOLDER_RMCONTACTNAME, _rmContactNames);
        }

        return result;
    }


    private ConflictCheck RemovePossibleSubordinateRecords(ConflictCheck conflictCheck)
    {
        if (_conclusion.Scenario == ConclusionScenarioEnum.Unidentified)  // Scenario 2
        {
            return conflictCheck;
        }

        if ((conflictCheck is null) || (conflictCheck.ConflictCheckID < 1))
        {
            return conflictCheck;
        }

        if (!conflictCheck.ConflictDisclaimers.IsNullOrEmpty())
        {
            conflictCheck.ConflictDisclaimers.Clear();
        }

        if (!conflictCheck.ConflictCheckConditions.IsNullOrEmpty())
        {
            conflictCheck.ConflictCheckConditions.Clear();
        }

        return conflictCheck;
    }
}
