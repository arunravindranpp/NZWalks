using ConflictAutomation.Models.FinScan.SubClasses.enums;

namespace ConflictAutomation.Models.FinScan.SubClasses;

public class PremiumResult
{
    public string clientName { get; set; }
    public string clientApplicationId { get; set; }
    public string clientClientId { get; set; }
    public long clientClientKey { get; set; }
    public ResultTypeEnum clientStatus { get; set; }
    public int clientResultsCount { get; set; }
    public int clientHitCount { get; set; }
    public int clientPendingCount { get; set; }
    public int clientSafeCount { get; set; }
    public int clientDuplicateCount { get; set; }
    public List<ComplianceRecord> clientComplianceRecords { get; set; }
    public int networkKey { get; set; }
    public string parentApplicationId { get; set; }
    public string parentClientId { get; set; }
    public long parentClientKey { get; set; }
    public int parentStatus { get; set; }
    public int parentResultsCount { get; set; }
    public int parentHitCount { get; set; }
    public int parentPendingCount { get; set; }
    public int parentSafeCount { get; set; }
    public int parentDuplicateCount { get; set; }
    public List<ComplianceRecord> parentComplianceRecords { get; set; }
}
