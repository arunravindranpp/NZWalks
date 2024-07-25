using ConflictAutomation.Extensions;
using ConflictAutomation.Models;
using ConflictAutomation.Models.ConclusionChecking;
using ConflictAutomation.Models.ResearchSummaryEngine;
using ConflictAutomation.Services.ConclusionChecking.enums;
using Serilog;

namespace ConflictAutomation.Services.ConclusionChecking;

// User Story #971644 - Consolidation 5b - Conclude Non-Client Side Entity Checks
// User Story #1019320 - Consolidation 5c - Conclude Client-Side Entity Checks
public class ConclusionOperations
{
    private const string SEP_MULTIPLE_SUBJECTS = " / ";

    private readonly List<ResearchSummary> _listResearchSummary;

    private readonly string _connectionString;

    private readonly long _conflictCheckID;

#pragma warning disable IDE0052 // Remove unread private members
    private readonly string _entityOrIndividualName;
#pragma warning restore IDE0052 // Remove unread private members
    private readonly string _gcoTeam;
    private readonly string _rmContactNames;

    private readonly ConclusionChecker _conclusionChecker;


    public ConclusionOperations(List<ResearchSummary> listResearchSummary, string connectionString, long conflictCheckID)
    {        
        _listResearchSummary = listResearchSummary;
        
        _conclusionChecker = new(_listResearchSummary);

        _connectionString = connectionString;

        _conflictCheckID = conflictCheckID;

        try
        {
            string country = GetMainClientCountry();
            CauDbQuery cauDbQuery = new(_connectionString);

            _entityOrIndividualName = _listResearchSummary.First().EntityName;
            _gcoTeam = cauDbQuery.GetGcoTeamByCountryName(country);
            _rmContactNames = cauDbQuery.GetRmContactsByCountryName(country);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred when instantiating a new object from class ConclusionOperations - Message: {ex}");
            LoggerInfo.LogException(ex, $" ConflictCheckID:{_conflictCheckID}");
        }
    }


    private string GetMainClientCountry()
    {
        string country = _listResearchSummary?.GetMainClientItem()?.Country;
        country ??= string.Empty;

        return country;
    }


    public void ProcessConclusion(long conflictCheckID, string masterWorkbookFullPath,
        List<ResearchSummaryEntry> researchSummaryGrid, CheckerQueue summary)
    {
        string side = "??? side";
        try
        {
            if(_conclusionChecker.IsNonClientSide())
            {
                side = "Non-Client side";
                ProcessConclusionForNonClientSide(conflictCheckID, masterWorkbookFullPath, 
                    researchSummaryGrid, summary);
                return;
            }

            if (_conclusionChecker.IsClientSide())
            {
                side = "Client side";
                ProcessConclusionForClientSide(conflictCheckID, masterWorkbookFullPath,
                    researchSummaryGrid, summary);
                return;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred in method ConclusionOperations.ProcessConclusion() - {side} - Message: {ex}");
            LoggerInfo.LogException(ex);
        }
    }


    private void ProcessConclusionForNonClientSide(long conflictCheckID, string masterWorkbookFullPath, 
        List<ResearchSummaryEntry> researchSummaryGrid, CheckerQueue summary)
    {
        ConclusionScenarioEnum scenario = _conclusionChecker.GetScenarioForNonClientSide();
        Conclusion conclusion = _conclusionChecker.GetConclusionForNonClientSide(scenario);

        List<string> listSanctionedSubjects = _conclusionChecker.ListResearchSummaryWithSanctions
                .Select(rs => rs.EntityName).Distinct().ToList();

        ConclusionWriter conclusionWriter = new(conclusion,
            string.Join(SEP_MULTIPLE_SUBJECTS, listSanctionedSubjects), _gcoTeam, _rmContactNames);
        conclusionWriter.UpdateExcel(masterWorkbookFullPath, summary);
        conclusionWriter.UpdatePACE(conflictCheckID, researchSummaryGrid, summary);
    }


    private void ProcessConclusionForClientSide(long conflictCheckID, string masterWorkbookFullPath, 
        List<ResearchSummaryEntry> researchSummaryGrid, CheckerQueue summary)
    {
        ConclusionScenarioEnum scenario = _conclusionChecker.GetScenarioForClientSide();
        Conclusion conclusion = _conclusionChecker.GetConclusionForClientSide(scenario);

        List<string> listSanctionedSubjects = _conclusionChecker.ListResearchSummaryWithSanctions
                .Select(rs => rs.EntityName).Distinct().ToList();

        ConclusionWriter conclusionWriter = new(conclusion,
            string.Join(SEP_MULTIPLE_SUBJECTS, listSanctionedSubjects), _gcoTeam, _rmContactNames);
        conclusionWriter.UpdateExcel(masterWorkbookFullPath, summary);
        conclusionWriter.UpdatePACE(conflictCheckID, researchSummaryGrid, summary);
    }
}
