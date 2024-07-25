namespace ConflictAutomation.Models;

public class MercuryEntity_SpreadSheet
{
    public long CERID { get; set; }
    public DateTime EngagementOpenDate { get; set; }
    public string ClientID { get; set; }
    public string Client { get; set; }
    public string DunsNumber { get; set; }
    public string DunsName { get; set; }
    public string DunsLocation { get; set; }
    public string UltimateDunsNumber { get; set; }
    public string Account { get; set; }
    public string EngagementID { get; set; }
    public string Engagement { get; set; }

    public string PACEID { get; set; }
    public string PACEStatus { get; set; }
    public string EngagementGlobalService { get; set; }
    public string EngagementServiceLine { get; set; }
    public string EngagementSubServiceLine { get; set; }
    public string EngagementCountry { get; set; }
    public string EngagementStatus { get; set; }
    public DateTime? EngagementStatusEffectiveDate { get; set; }
    public DateTime? EngagementOpenDateShowcase { get; set; }
    public DateTime? EngagementOpenDateFrom { get; set; }
    public DateTime? EngagementOpenDateTo { get; set; }
    public DateTime? EngagementLastTimeChargedDate { get; set; }
    public DateTime? LatestInvoiceIssuedDate { get; set; }
    public string EngagementType { get; set; }


    public string GCSP { get; set; }
    public string GCSPEmail { get; set; }
    public string EngagementPartner { get; set; }
    public string EngagementPartnerEmail { get; set; }
    public string EngagementManager { get; set; }
    public string TechnologyIndicatorCdJoin { get; set; }
    public string NER { get; set; }
    public string ChargedHours { get; set; }
    public string BilledFees { get; set; }
    public string CurrencyCode { get; set; }
    public string AccountChannel { get; set; }
    public string SECFlag { get; set; }
    public string EngagementReportingOrg { get; set; }
    public int BN_Fuzzy { get; set; }   //Client Fuzzy % - computed column
    public int AN_Fuzzy { get; set; }   //DUNS Name Fuzzy % - computed column
    public string SearchMode { get; set; }
    public string KeywordUsed { get; set; }

    public string KeywordType { get; set; }
    public string EntityWithoutLegalExt { get; set; }
    public string DUNSNameWithoutLegalExt { get; set; }


    // User Story 1019884 - CER Search and Extract Cont'd ----------
    public bool IsEngagementMoreRecentThan(int deltaYears) =>
        EngagementOpenDate >= DateTime.UtcNow.Date.AddYears(deltaYears);  // deltaYears must be negative to result in past dates


    public bool IsFinancialStatementAudit() => 
        EngagementGlobalService.Contains("(35)") || EngagementGlobalService.Contains("(10067)");
    // User Story 1019884 - CER Search and Extract Cont'd ----------
}

public class CEREmbeddFiles
{
    public int EntityMatchCount;
    public List<MercuryEntity_SpreadSheet> EntityMatchRecords;
}