namespace ConflictAutomation.Services.ConclusionChecking.enums;

public enum ConclusionScenarioEnum
{
    Unidentified,
    NoSanctions,
    ClientSideSanctions_MainRoles,   // Main Client / Owner / Shareholder of client / GUP of these roles
    ClientSideSanctions_OtherRoles,  // This will never happen, because listResearchSummary[0] is always the main client.
    NonClientSideSanctions
}
