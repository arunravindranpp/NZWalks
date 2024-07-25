using ConflictAutomation.Models.FinScan.SubClasses.enums;

namespace ConflictAutomation.Models.FinScan.SubClasses;

public class UBOResults
{
    public int networkKey { get; set; }
    public int returned { get; set; }
    public int notReturned { get; set; }
    public int hitCount { get; set; }
    public int pendingCount { get; set; }
    public int safeCount { get; set; }
    public int duplicateCount { get; set; }
    public int resultsCount { get; set; }
    public ResultTypeEnum status { get; set; }
    public List<UBOMemberResult> memberResults { get; set; }
}
