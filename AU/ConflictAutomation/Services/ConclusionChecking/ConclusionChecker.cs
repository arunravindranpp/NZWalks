using ConflictAutomation.Constants;
using ConflictAutomation.Extensions;
using ConflictAutomation.Models;
using ConflictAutomation.Models.ConclusionChecking;
using ConflictAutomation.Services.ConclusionChecking.enums;
using Serilog;

namespace ConflictAutomation.Services.ConclusionChecking;

public class ConclusionChecker
{
    #region Messages - Both Client Side and Non-Client Side
    public const string MSG_CONCLUSION = "Conclusion";

    private const string MSG_NO_ISSUES = "No issues";

    private const string MSG_OUTCOME_PASS = "Pass";    
    private const string MSG_OUTCOME_CONDITIONAL = "Conditional";
    #endregion

    #region Messages - Non-Client Side

    private const string MSG_NON_CLIENT_SIDE_CONFLICT_CONCLUSION_CASES_1_to_3 = "No conflict of interest has been found which precludes the acceptance of this engagement.";
    private const string MSG_NON_CLIENT_SIDE_CONFLICT_CONCLUSION_CASE_4 = "No conflicts of interest have been identified which preclude the acceptance of this engagement. However, the condition(s) raised must be met prior to the acceptance of this engagement.";

    private const string MSG_NON_CLIENT_SIDE_DISCLAIMER_INDEPENDENCE_CASES_1_to_4 = "This clearance only addresses Conflicts considerations. No independence issues regarding the scope permissibility have been considered.";

    private const string MSG_NON_CLIENT_SIDE_RATIONALE_INSTRUCTIONS_CASE_1 = "";
    private const string MSG_NON_CLIENT_SIDE_RATIONALE_INSTRUCTIONS_CASE_4 = "";

    private const string MSG_NON_CLIENT_SIDE_CONDITION_DESCRIPTION_1 = "";
    private const string MSG_NON_CLIENT_SIDE_CONDITION_DESCRIPTION_4 = "Please note that <NAME> entity is a part of Sanctions list, hence, kindly liaise with the local GCO team <GCOTEAM> and Local RM contact <RMCONTACTNAME> with respect to the feasibility of acceptance of this engagement. We have attached a match report in the conclusion for your reference.";
    #endregion

    #region Messages - Client Side

    private const string MSG_CLIENT_SIDE_CONFLICT_CONCLUSION_CASES_1_to_3 = "No conflict of interest has been found which precludes the acceptance of this engagement.";

    private const string MSG_CLIENT_SIDE_DISCLAIMER_INDEPENDENCE_CASES_1_to_3 = "This clearance only addresses Conflicts considerations. No independence issues regarding the scope permissibility have been considered.";

    private const string MSG_CLIENT_SIDE_RATIONALE_INSTRUCTIONS_CASE_1 = "";
    private const string MSG_CLIENT_SIDE_RATIONALE_INSTRUCTIONS_CASE_2 = "Since <NAME> is subject to economic or trade sanctions, kindly liaise with the local GCO team <GCOTEAM> and Local RM contact <RMCONTACTNAME> with respect to the feasibility of acceptance of this engagement (if not already done). We have attached a match report in the conclusion for your reference.";
    private const string MSG_CLIENT_SIDE_RATIONALE_INSTRUCTIONS_CASE_3 = "Since <NAME> is subject to economic or trade sanctions, kindly liaise with the local GCO team <GCOTEAM> and Local RM contact <RMCONTACTNAME> with respect to the feasibility of acceptance of this engagement (if not already done). We have attached a match report in the conclusion for your reference.";

    private const string MSG_CLIENT_SIDE_CONDITION_DESCRIPTION_1 = "";
    private const string MSG_CLIENT_SIDE_CONDITION_DESCRIPTION_2 = "";
    private const string MSG_CLIENT_SIDE_CONDITION_DESCRIPTION_3 = "";
    #endregion


    private readonly List<ResearchSummary> _listResearchSummary;
    private readonly List<ResearchSummary> _listResearchSummaryClientSide;
    private readonly List<ResearchSummary> _listResearchSummaryNonClientSide;
    private readonly List<ResearchSummary> _listResearchSummaryWithMultipleCloseMatchesAsPerGis;
    private readonly List<ResearchSummary> _listResearchSummaryWithMultipleCloseMatchesAsPerGisOrCer;
    private readonly List<ResearchSummary> _listResearchSummaryNonClientSideLackingUidAsPerGisOrCer;
    private readonly List<ResearchSummary> _listResearchSummaryClientSideWithSanctions;
    private readonly List<ResearchSummary> _listResearchSummaryNonClientSideWithSanctions;

    public List<ResearchSummary> ListResearchSummary => _listResearchSummary;
    public List<ResearchSummary> ListResearchSummaryClientSide => _listResearchSummaryClientSide;
    public List<ResearchSummary> ListResearchSummaryNonClientSide => _listResearchSummaryNonClientSide;
    public List<ResearchSummary> ListResearchSummaryWithMultipleCloseMatchesAsPerGis => _listResearchSummaryWithMultipleCloseMatchesAsPerGis;
    public List<ResearchSummary> ListResearchSummaryWithMultipleCloseMatchesAsPerGisOrCer => _listResearchSummaryWithMultipleCloseMatchesAsPerGisOrCer;
    public List<ResearchSummary> ListResearchSummaryNonClientSideLackingUidAsPerGisOrCer => _listResearchSummaryNonClientSideLackingUidAsPerGisOrCer;
    public List<ResearchSummary> ListResearchSummaryClientSideWithSanctions => _listResearchSummaryClientSideWithSanctions;
    public List<ResearchSummary> ListResearchSummaryNonClientSideWithSanctions => _listResearchSummaryNonClientSideWithSanctions;
    
    public List<ResearchSummary> ListResearchSummaryWithSanctions 
    { 
        get
        { 
            List<ResearchSummary> result = [];
            result.AddRange(_listResearchSummaryClientSideWithSanctions);
            result.AddRange(_listResearchSummaryNonClientSideWithSanctions);
            return result.Distinct().ToList();
        } 
    }

    public ConclusionChecker(List<ResearchSummary> listResearchSummary)
    {
        try
        {
            if (listResearchSummary.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(listResearchSummary));
            }

            _listResearchSummary = listResearchSummary;

            _listResearchSummaryClientSide = _listResearchSummary.GetClientSideItems();

            _listResearchSummaryNonClientSide = _listResearchSummary.GetNonClientSideItems();

            _listResearchSummaryWithMultipleCloseMatchesAsPerGis = _listResearchSummary.Where(rs =>
                   GISNotes(rs).Equals(CAUConstants.GIS_MULTIPLE_CLOSE_MATCHES, StringComparison.OrdinalIgnoreCase)).ToList();

            _listResearchSummaryWithMultipleCloseMatchesAsPerGisOrCer = _listResearchSummary.Where(rs =>
                   GISNotes(rs).Equals(CAUConstants.GIS_MULTIPLE_CLOSE_MATCHES, StringComparison.OrdinalIgnoreCase)
                || CERNotes(rs).Equals(CAUConstants.CER_MULTIPLE_CLOSE_MATCHES, StringComparison.OrdinalIgnoreCase)).ToList();

            _listResearchSummaryNonClientSideLackingUidAsPerGisOrCer = _listResearchSummaryNonClientSide.Where(rs => 
                    (
                           GISDesc(rs).Equals(CAUConstants.GIS_ENTITY_UNDER_AUDIT__NO_RECORD_WITH_UID, StringComparison.OrdinalIgnoreCase)
                        || GISDesc(rs).Equals(CAUConstants.GIS_ENTITY_NOT_UNDER_AUDIT__NO_RECORD_WITH_UID, StringComparison.OrdinalIgnoreCase)
                        || GISDesc(rs).Equals(CAUConstants.GIS_NO_RECORD_WITH_UID, StringComparison.OrdinalIgnoreCase)
                        || CERDesc(rs).Equals(CAUConstants.CER_NO_RECORD_WITH_UID_1, StringComparison.OrdinalIgnoreCase)
                        || CERDesc(rs).Contains(CAUConstants.CER_NO_RECORD_WITH_UID_2, StringComparison.OrdinalIgnoreCase)
                    )
                    && (!CERDesc(rs).Contains(CAUConstants.NON_AUDIT_CLIENT, StringComparison.OrdinalIgnoreCase))
                    && (!GISDesc(rs).Contains(CAUConstants.NON_AUDIT_CLIENT, StringComparison.OrdinalIgnoreCase))
                ).ToList();

            _listResearchSummaryClientSideWithSanctions = _listResearchSummaryClientSide.Where(rs => rs.Sanctions).ToList();

            _listResearchSummaryNonClientSideWithSanctions = _listResearchSummaryNonClientSide.Where(rs => rs.Sanctions).ToList();
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred when instantiating a ConclusionChecker object. Message: {ex}");
            LoggerInfo.LogException(ex);
        }
    }

    
    public bool ThereIsAtLeastOneRecordWithMultipleCloseMatchesAsPerGis() =>
        _listResearchSummaryWithMultipleCloseMatchesAsPerGis.Count > 0;


    public bool ThereIsAtLeastOneRecordWithMultipleCloseMatchesAsPerGisOrCer() =>
        _listResearchSummaryWithMultipleCloseMatchesAsPerGisOrCer.Count > 0;


    public bool AllNonClientSideRecordsLackUidAsPerGisOrCer() =>
        (_listResearchSummaryNonClientSide.Count == _listResearchSummaryNonClientSideLackingUidAsPerGisOrCer.Count);


    public bool NoEYRelationshipWithCounterparties()
    {
        bool thereIsNoRecordWithMultipleCloseMatches = !ThereIsAtLeastOneRecordWithMultipleCloseMatchesAsPerGisOrCer();
        bool allNonClientSideRecordsLackUID = AllNonClientSideRecordsLackUidAsPerGisOrCer();
        return thereIsNoRecordWithMultipleCloseMatches && allNonClientSideRecordsLackUID;
    }


    public bool ClientSideIsSanctioned() => _listResearchSummaryClientSideWithSanctions.Count > 0;

    public bool NonClientSideIsSanctioned() => _listResearchSummaryNonClientSideWithSanctions.Count > 0;

    public bool IsSanctioned() => ClientSideIsSanctioned() || NonClientSideIsSanctioned();


    public bool ResearchFailed_CRR() =>
        _listResearchSummary.Any(rs => rs.CRR.Equals("F", StringComparison.OrdinalIgnoreCase));

    public bool ResearchFailed_FinScan() =>
        _listResearchSummary.Any(rs => rs.Finscan.Equals("F", StringComparison.OrdinalIgnoreCase));
    
    public bool ResearchFailed_GIS() =>
        _listResearchSummary.Any(rs => rs.GIS.Equals("F", StringComparison.OrdinalIgnoreCase));

    public bool ResearchFailed_Mercury() =>
        _listResearchSummary.Any(rs => rs.Mercury.Equals("F", StringComparison.OrdinalIgnoreCase));

    public bool ResearchFailed_SPL() =>
        _listResearchSummary.Any(rs => rs.SPL.Equals("F", StringComparison.OrdinalIgnoreCase));

    public bool ResearchFailed_Any() =>
        ResearchFailed_CRR() || ResearchFailed_FinScan() || ResearchFailed_GIS() 
        || ResearchFailed_Mercury() || ResearchFailed_SPL();


    public ConclusionScenarioEnum GetScenarioForNonClientSide()
    {
        try
        {
            if (ResearchFailed_Any())
            {
                return ConclusionScenarioEnum.Unidentified;
            }

            if (!NoEYRelationshipWithCounterparties())
            {
                return ConclusionScenarioEnum.Unidentified;
            }

            if (!IsSanctioned())
            {
                return ConclusionScenarioEnum.NoSanctions;
            }

            if (ClientSideIsSanctioned())
            {
                return ConclusionScenarioEnum.ClientSideSanctions_MainRoles;
            }

            // Case 3. ConclusionScenarioEnum.ClientSideSanctions_OtherRoles
            // will never happen, because listResearchSummary[0] is
            // always the main client.

            if (NonClientSideIsSanctioned())
            {
                return ConclusionScenarioEnum.NonClientSideSanctions;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred in method ConclusionChecker.GetScenarioForNonClientSide() - Message: {ex}");
            LoggerInfo.LogException(ex);
        }

        return ConclusionScenarioEnum.Unidentified;
    }


    public Conclusion GetConclusionForNonClientSide(ConclusionScenarioEnum conclusionScenario) =>
        conclusionScenario switch
        {
            ConclusionScenarioEnum.Unidentified => new Conclusion()  // Scenario #0
            {
                Scenario = conclusionScenario, 
                Case = string.Empty,
                Issue = string.Empty, 
                Outcome = string.Empty,
                ConflictConclusion = string.Empty,
                DisclaimerIndependence = string.Empty,
                RationaleInstructions = string.Empty,
                ConditionDescription = string.Empty,
                Attachments = [], 
                ResearchFailedInAnyOfTheDataSources = ResearchFailed_Any()
            },
            ConclusionScenarioEnum.NoSanctions => new Conclusion()  // Scenario #1
            {
                Scenario = conclusionScenario,
                Case = MSG_NO_ISSUES,
                Issue = MSG_NO_ISSUES, 
                Outcome = MSG_OUTCOME_PASS,
                ConflictConclusion = MSG_NON_CLIENT_SIDE_CONFLICT_CONCLUSION_CASES_1_to_3,
                DisclaimerIndependence = MSG_NON_CLIENT_SIDE_DISCLAIMER_INDEPENDENCE_CASES_1_to_4,
                RationaleInstructions = MSG_NON_CLIENT_SIDE_RATIONALE_INSTRUCTIONS_CASE_1,
                ConditionDescription = MSG_NON_CLIENT_SIDE_CONDITION_DESCRIPTION_1,
                Attachments = [],
                ResearchFailedInAnyOfTheDataSources = null
            },
            ConclusionScenarioEnum.ClientSideSanctions_MainRoles => new Conclusion()  // Scenario #2
            {
                Scenario = conclusionScenario,
                Case = MSG_NO_ISSUES,
                Issue = MSG_NO_ISSUES,
                Outcome = MSG_OUTCOME_PASS,
                ConflictConclusion = MSG_CLIENT_SIDE_CONFLICT_CONCLUSION_CASES_1_to_3,
                DisclaimerIndependence = MSG_CLIENT_SIDE_DISCLAIMER_INDEPENDENCE_CASES_1_to_3,
                RationaleInstructions = MSG_CLIENT_SIDE_RATIONALE_INSTRUCTIONS_CASE_2,
                ConditionDescription = MSG_CLIENT_SIDE_CONDITION_DESCRIPTION_2,
                Attachments = new(FinScanListProfileReportFilePaths()),
                ResearchFailedInAnyOfTheDataSources = null
            },
            ConclusionScenarioEnum.ClientSideSanctions_OtherRoles => new Conclusion()  // Scenario #3
            {
                Scenario = conclusionScenario,
                Case = MSG_NO_ISSUES,
                Issue = MSG_NO_ISSUES,
                Outcome = MSG_OUTCOME_PASS,
                ConflictConclusion = MSG_CLIENT_SIDE_CONFLICT_CONCLUSION_CASES_1_to_3,
                DisclaimerIndependence = MSG_CLIENT_SIDE_DISCLAIMER_INDEPENDENCE_CASES_1_to_3,
                RationaleInstructions = MSG_CLIENT_SIDE_RATIONALE_INSTRUCTIONS_CASE_3,
                ConditionDescription = MSG_CLIENT_SIDE_CONDITION_DESCRIPTION_3,
                Attachments = new(FinScanListProfileReportFilePaths()),
                ResearchFailedInAnyOfTheDataSources = null
            },
            ConclusionScenarioEnum.NonClientSideSanctions => new Conclusion()  // Scenario #4
            {
                Scenario = conclusionScenario,
                Case = MSG_NO_ISSUES,
                Issue = MSG_NO_ISSUES,
                Outcome = MSG_OUTCOME_CONDITIONAL,
                ConflictConclusion = MSG_NON_CLIENT_SIDE_CONFLICT_CONCLUSION_CASE_4,
                DisclaimerIndependence = MSG_NON_CLIENT_SIDE_DISCLAIMER_INDEPENDENCE_CASES_1_to_4,
                RationaleInstructions = MSG_NON_CLIENT_SIDE_RATIONALE_INSTRUCTIONS_CASE_4,
                ConditionDescription = MSG_NON_CLIENT_SIDE_CONDITION_DESCRIPTION_4,
                Attachments = new(FinScanListProfileReportFilePaths()),
                ResearchFailedInAnyOfTheDataSources = null
            },
            _ => throw new ArgumentOutOfRangeException(
                            nameof(conclusionScenario), conclusionScenario,
                            CAUConstants.MSG_INVALID_VALUE_FOR_CONCLUSION_SCENARIO)
        };


    public ConclusionScenarioEnum GetScenarioForClientSide()
    {
        try
        {
           if (ResearchFailed_Any())
            {
                return ConclusionScenarioEnum.Unidentified;
            }

            if (ThereIsAtLeastOneRecordWithMultipleCloseMatchesAsPerGis())
            {
                return ConclusionScenarioEnum.Unidentified;
            }

            if (!IsSanctioned())
            {
                return ConclusionScenarioEnum.NoSanctions;
            }

            if (ClientSideIsSanctioned())
            {
                return ConclusionScenarioEnum.ClientSideSanctions_MainRoles;
            }

            // Case 3. ConclusionScenarioForClientSideEnum.ClientSideSanctions_OtherRoles
            // will never happen, because listResearchSummary[0] is
            // always the main client.
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred in method ConclusionChecker.GetScenarioForClientSide() - Message: {ex}");
            LoggerInfo.LogException(ex);
        }

        return ConclusionScenarioEnum.Unidentified;
    }


    public Conclusion GetConclusionForClientSide(ConclusionScenarioEnum conclusionScenario) =>
        conclusionScenario switch
        {
            ConclusionScenarioEnum.Unidentified => new Conclusion()  // Scenario #0
            {
                Scenario = conclusionScenario,
                Case = string.Empty,
                Issue = string.Empty,
                Outcome = string.Empty,
                ConflictConclusion = string.Empty,
                DisclaimerIndependence = string.Empty,
                RationaleInstructions = string.Empty,
                ConditionDescription = string.Empty,
                Attachments = [],
                ResearchFailedInAnyOfTheDataSources = ResearchFailed_Any()
            },
            ConclusionScenarioEnum.NoSanctions => new Conclusion()  // Scenario #1
            {
                Scenario = conclusionScenario,
                Case = MSG_NO_ISSUES,
                Issue = MSG_NO_ISSUES,
                Outcome = MSG_OUTCOME_PASS,
                ConflictConclusion = MSG_CLIENT_SIDE_CONFLICT_CONCLUSION_CASES_1_to_3,
                DisclaimerIndependence = MSG_CLIENT_SIDE_DISCLAIMER_INDEPENDENCE_CASES_1_to_3,
                RationaleInstructions = MSG_CLIENT_SIDE_RATIONALE_INSTRUCTIONS_CASE_1,
                ConditionDescription = MSG_CLIENT_SIDE_CONDITION_DESCRIPTION_1,
                Attachments = [],
                ResearchFailedInAnyOfTheDataSources = null
            },
            ConclusionScenarioEnum.ClientSideSanctions_MainRoles => new Conclusion()  // Scenario #2
            {
                Scenario = conclusionScenario,
                Case = MSG_NO_ISSUES,
                Issue = MSG_NO_ISSUES,
                Outcome = MSG_OUTCOME_PASS,
                ConflictConclusion = MSG_CLIENT_SIDE_CONFLICT_CONCLUSION_CASES_1_to_3,
                DisclaimerIndependence = MSG_CLIENT_SIDE_DISCLAIMER_INDEPENDENCE_CASES_1_to_3,
                RationaleInstructions = MSG_CLIENT_SIDE_RATIONALE_INSTRUCTIONS_CASE_2,
                ConditionDescription = MSG_CLIENT_SIDE_CONDITION_DESCRIPTION_2,
                Attachments = new(FinScanListProfileReportFilePaths()),
                ResearchFailedInAnyOfTheDataSources = null
            },
            ConclusionScenarioEnum.ClientSideSanctions_OtherRoles => new Conclusion()  // Scenario #3
            {
                Scenario = conclusionScenario,
                Case = MSG_NO_ISSUES,
                Issue = MSG_NO_ISSUES,
                Outcome = MSG_OUTCOME_PASS,
                ConflictConclusion = MSG_CLIENT_SIDE_CONFLICT_CONCLUSION_CASES_1_to_3,
                DisclaimerIndependence = MSG_CLIENT_SIDE_DISCLAIMER_INDEPENDENCE_CASES_1_to_3,
                RationaleInstructions = MSG_CLIENT_SIDE_RATIONALE_INSTRUCTIONS_CASE_3,
                ConditionDescription = MSG_CLIENT_SIDE_CONDITION_DESCRIPTION_3,
                Attachments = new(FinScanListProfileReportFilePaths()),
                ResearchFailedInAnyOfTheDataSources = null
            },
            _ => throw new ArgumentOutOfRangeException(
                            nameof(conclusionScenario), conclusionScenario,
                            CAUConstants.MSG_INVALID_VALUE_FOR_CONCLUSION_SCENARIO)
        };


    public List<string> FinScanListProfileReportFilePaths() =>
        _listResearchSummary
            .Where(rs => !string.IsNullOrWhiteSpace(rs.FinScanListProfileReportFilePath))
            .Select(rs => rs.FinScanListProfileReportFilePath)
            .Distinct().ToList();


    public List<string> FinScanMatchReports() =>
        _listResearchSummary
            .Where(rs => !string.IsNullOrWhiteSpace(rs.FinScanMatchReportFilePath))
            .Select(rs => rs.FinScanMatchReportFilePath)
            .Distinct().ToList();


    public bool IsClientSide() =>
        (_listResearchSummaryClientSide.Count == _listResearchSummary.Count)
        && (_listResearchSummaryNonClientSide.Count == 0);


    public bool IsNonClientSide() => !IsClientSide();


    private static string GISNotes(ResearchSummary rs) => rs?.ClientRelationshipSummary?.GISNotes.FullTrim() ?? string.Empty;
    private static string GISDesc(ResearchSummary rs) => rs?.ClientRelationshipSummary?.GISDesc.FullTrim() ?? string.Empty;

    private static string CERNotes(ResearchSummary rs) => rs?.ClientRelationshipSummary?.CERNotes.FullTrim() ?? string.Empty;
    private static string CERDesc(ResearchSummary rs) => rs?.ClientRelationshipSummary?.CERDesc.FullTrim() ?? string.Empty;
}
