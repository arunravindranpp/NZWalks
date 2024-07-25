using ConflictAutomation.Models.FinScan.SubClasses.enums;

namespace ConflictAutomation.Models.FinScan.SubClasses;

public class RequestOptions
{
    public YesNoEnum addClient { get; set; }
    public YesNoEnum sendToReview { get; set; }
    public YesNoEnum returnCategory { get; set; }
    public YesNoEnum returnSearchComplianceRecords { get; set; }
    public YesNoEnum returnComplianceRecords { get; set; }
    public YesNoEnum returnSourceLists { get; set; }
    public YesNoEnum returnSearchDetails { get; set; }
    public YesNoEnum generateclientId { get; set; }
    public YesNoEnum updateUserFields { get; set; }
    public YesNoEnum skipSearch { get; set; }
    public YesNoEnum processUBO { get; set; }
    public YesNoEnum searchUBOMembers { get; set; }
    public YesNoEnum skipClientUpdate { get; set; }
    public YesNoEnum overwritePassthrough { get; set; }
    public YesNoEnum processFinScanPremium { get; set; }
    public YesNoEnum disablePremiumNameSplit { get; set; }
    public YesNoEnum disablePremiumAddressLineClientGeneration { get; set; }
    public YesNoEnum disablePremiumCrossApplicationMatching { get; set; }
    public YesNoEnum returnClientRiskRating { get; set; }
}
