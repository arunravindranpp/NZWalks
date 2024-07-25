using ConflictAutomation.Extensions;
using ConflictAutomation.Models.FinScan.SubClasses;
using ConflictAutomation.Services.FinScan;

namespace ConflictAutomation.Models.FinScan;

public class FinScanMatchReport
{
#pragma warning disable IDE0305 // Simplify collection initialization

    public string User { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public DateTime CreatedDateTime { get; set; } = DateTime.Now;
    public string CreatedTimestamp => CreatedDateTime.TimestampWithTimezoneFromLocal();
    public string UserOrganizationCreatedTimestamp => $"{User} | {Organization} | {CreatedTimestamp}";

    public string ApplicationName { get; set; } = string.Empty;
    public string SearchType { get; set; } = string.Empty;
    public string ClientSearchCode { get; set; } = string.Empty;
    public string RecordType => $"{SearchType} - {ClientSearchCode}";
    public string Name { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public List<Category> ListCategories { get; set; } = [];

    public List<MatchReportResult> ListFullResultSet { get; set; } = [];


    #region QUESTION CONCERNING THE NUMBER OF RECORDS
    // Question from EY:
    // "In the FinScan 'match file' (the one with title 'List Results' (...)), 
    //  there is a section with fields named 'Number of Records Reported',
    //  'Number of Records Returned' and 'Number of Records Not Returned'.
    //   What are the corresponding fields in Response JSON sent by FinScan
    //   when we make a call to the Search API ?"
    // 
    // Answer from Innovative System Support (March, 8th 2024) to question submitted by Grant F: 
    // "(..) two fields: 'returned' and 'notReturned'.
    //  The Reported value probably represents their sum. These figures denote
    //  the overall matches detected during the search, the count of returned
    //  matches (limited by FinScan's maximum threshold), and the excess matches
    //  beyond this limit, which are labeled as not returned."
    #endregion QUESTION CONCERNING THE NUMBER OF RECORDS
    public int NumberOfRecordsReturned => ListFullResultSet.Count;
    public int NumberOfRecordsNotReturned { get; set; } = 0;
    public int NumberOfRecordsReported => NumberOfRecordsReturned + NumberOfRecordsNotReturned;


    public void AddReport(FinScanMatchReport additionalMatchReport)
    {
        AccumulateValueIfInexistent(this, "User", FinScanConstants.STRING_SEPARATOR, additionalMatchReport);
        AccumulateValueIfInexistent(this, "Organization", FinScanConstants.STRING_SEPARATOR, additionalMatchReport);
        CreatedDateTime = (additionalMatchReport.CreatedDateTime > CreatedDateTime) 
                            ? additionalMatchReport.CreatedDateTime : CreatedDateTime;
        AccumulateValueIfInexistent(this, "ApplicationName", FinScanConstants.STRING_SEPARATOR, additionalMatchReport);
        AccumulateValueIfInexistent(this, "SearchType", FinScanConstants.STRING_SEPARATOR, additionalMatchReport);
        AccumulateValueIfInexistent(this, "ClientSearchCode", FinScanConstants.STRING_SEPARATOR, additionalMatchReport);
        AccumulateValueIfInexistent(this, "Name", FinScanConstants.STRING_SEPARATOR, additionalMatchReport);
        AccumulateValueIfInexistent(this, "ClientId", FinScanConstants.STRING_SEPARATOR, additionalMatchReport);
        AccumulateValueIfInexistent(this, "Address", FinScanConstants.STRING_SEPARATOR, additionalMatchReport);
        AccumulateValueIfInexistent(this, "Notes", FinScanConstants.STRING_SEPARATOR, additionalMatchReport);

        // ListCategories.AppendNewListObjectsIfInexistent(additionalMatchReport.ListCategories);
        // Both implementations are equivalent, but the second is lighter and faster.
        ListCategories.AddRange(additionalMatchReport.ListCategories);
        ListCategories = ListCategories
                            .DistinctBy(category => new { category.ListName, category.CategoryName })
                            .OrderBy(category => $"{category.ListName}|{category.CategoryName}")
                            .ToList();

        // ListFullResultSet.AppendNewListObjectsIfInexistent(additionalMatchReport.ListFullResultSet);
        // Both implementations are equivalent, but the second is lighter and faster.
        ListFullResultSet.AddRange(additionalMatchReport.ListFullResultSet);

        ListFullResultSet = ListFullResultSet
                                .DistinctBy(matchReport => $"{matchReport.ListProfileId}::{matchReport.ClientNameAndAddress}")
                                .OrderBy(matchReport => matchReport.ListProfileId)
                                .ToList();
    }


    private static object AccumulateValueIfInexistent(object obj, string stringPropertyName, string stringSeparator, object additionalObj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(stringSeparator);

        string originalValue = (string) obj.GetPropertyValue(stringPropertyName);
        string additionalValue = (string) additionalObj.GetPropertyValue(stringPropertyName);

        if(originalValue.IsNullOrEmpty())
        {
            obj.SetPropertyValue(stringPropertyName, additionalValue);
        }
        else if (!(originalValue.Split(stringSeparator).Contains(additionalValue)))
        {
            obj.SetPropertyValue(stringPropertyName, $"{originalValue}{stringSeparator}{additionalValue}");
        }
        
        return obj;
    }
#pragma warning restore IDE0305 // Simplify collection initialization
}
