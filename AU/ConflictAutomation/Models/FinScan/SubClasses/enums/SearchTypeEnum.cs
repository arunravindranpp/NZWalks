namespace ConflictAutomation.Models.FinScan.SubClasses.enums;

public enum SearchTypeEnum
{
    // 0 Perform an Individual Search (Client is an Individual) (default).
    Individual,

    // 1 Perform an Organization Search (Client is an Organization).
    Organization,

    // 2 Perform a Specific Element Search (e.g. Country or Currency).
    SpecificElement,

    // 3 Perform Both Individual and Specific Element Searches (Client is an Individual).
    Individual_Both,

    // 4 Perform Both Individual and Organization Searches (Client is an Organization).
    Organization_Both,

    // 5 Perform Individual, Organization, and Specific Element Searches (Client is an Individual).
    Individual_SpecificElement,

    // 6 Perform Both Organization and Specific Element Searches (Client is an Organization).
    Organization_SpecificElement,

    // 7 Perform Individual, Organization, and Specific Element Searches (Client is an Individual).
    Individual_All,

    // 8 Perform Individual, Organization, and Specific Element Searches (Client is an Organization).
    Organization_All,

    // 9 For SLRescreen Only. Perform Search based on the Client's isOrganization value within FinScan. Will use Individual if used outside of SLRescren.
    Default,

    // 10 Perform Individual and Organization Searches (Client type is Unknown).
    Unknown_Both,

    // 11 Perform Specific Element Search (Client type is Unknown).
    Unknown_SpecificElement,

    // 12 Perform Individual, Organization, and Specific Element (Client type is Unknown).
    Unknown_All
}
