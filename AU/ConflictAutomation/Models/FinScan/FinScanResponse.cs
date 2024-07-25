using ConflictAutomation.Models.FinScan.SubClasses;
using ConflictAutomation.Models.FinScan.SubClasses.enums;

namespace ConflictAutomation.Models.FinScan;

public class FinScanResponse
{
    public ResultTypeEnum status { get; set; }
    public int code { get; set; }
    public string message { get; set; }
    public int returned { get; set; }
    public int notReturned { get; set; }
    public int resultsCount { get; set; }
    public int hitCount { get; set; }
    public int pendingCount { get; set; }
    public int safeCount { get; set; }
    public int duplicateCount { get; set; }
    public List<ComplianceRecord> complianceRecords { get; set; }
    public string clientId { get; set; }
    public long clientKey { get; set; }
    public string version { get; set; }
    public string isiReserved { get; set; }
    public List<SearchResult> searchResults { get; set; }
    public UBOResults uboResults { get; set; }
    public List<PremiumResult> premiumResults { get; set; }
    public ClientRiskDetail riskDetails { get; set; }
}
