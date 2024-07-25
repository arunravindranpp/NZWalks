namespace ConflictAutomation.Models.ResearchSummaryEngine;

public class ResearchSummaryEntry(
    string worksheetTabName,
    string partyInvolved,
    string country,
    string role,
    string summary)
{
    public string WorksheetTabName { get; init; } = worksheetTabName;
    public string PartyInvolved { get; init; } = partyInvolved;
    public string Country { get; init; } = country;
    public string Role { get; init; } = role;
    public string Summary { get; init; } = summary;
}
