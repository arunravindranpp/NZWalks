using ConflictAutomation.Models.FinScan.SubClasses.enums;

namespace ConflictAutomation.Models.FinScan.SubClasses;

public class UBOMemberResult
{
    public int hitCount { get; set; }
    public int pendingCount { get; set; }
    public int safeCount { get; set; }
    public int duplicateCount { get; set; }
    public int resultsCount { get; set; }
    public ResultTypeEnum status { get; set; }
    public long clientKey { get; set; }
    public string clientId { get; set; }
    public string applicationId { get; set; }
    public string fullName { get; set; }
    public string networkRelationship { get; set; }
    public string ownershipPercentage { get; set; }
    public int numberOfUBONetworkMemberships { get; set; }
    public YesNoEnum isOwnershipLeafFlag { get; set; }
    public SearchResult searchResults { get; set; }
    public List<ComplianceRecord> complianceRecords { get; set; }
}
