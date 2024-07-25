namespace ConflictAutomation.Models;

public class MercuryEntity
{
    public long CERID { get; set; }
    public string Source { get; set; }
    public string EngagementID { get; set; }
    public string Engagement { get; set; }
    public string EngagementType { get; set; }
    public string EngagementStatus { get; set; }
    public string ClientID { get; set; }
    public string Client { get; set; }
    public string DunsNumber { get; set; }
    public string DunsName { get; set; }
    public string DunsLocation { get; set; }
    public string UltimateDunsNumber { get; set; }
    public string Account { get; set; }
    public string PACEID { get; set; }
    public string PACEStatus { get; set; }
    public string EngagementGlobalService { get; set; }
    public string EngagementServiceLine { get; set; }
    public string EngagementSubServiceLine { get; set; }
    public string EngagementCountry { get; set; }
    public string EngagementRegion { get; set; }
    public string EngagementStatusEffectiveDate { get; set; }
    public string EngagementCreationDate { get; set; }
    public string EngagementLastTimeChargedDate { get; set; }
    public string EngagementLastTimeChargedDateGFIS { get; set; }
    public string LatestInvoiceIssuedDate { get; set; }
    public string LatestInvoiceIssuedDateGFIS { get; set; }

    public string GCSP {  get; set; }
    public string GCSPEmail { get; set; }
    public string EngagementPartner { get; set; }
    public string EngagementPartnerEmail { get; set; }
    public string EngagementManager { get; set; }
    public string TechnologyIndicatorCdJoin { get; set; }
    public string NER {  get; set; }
    public string ChargedHours { get; set; }
    public string  BilledFees { get; set; }
    public string CurrencyCode { get; set; }
    public string AccountChannel {  get; set; }


    public string AccountingCycleDate { get; set; }
    public string EngagementOpenDateFrom { get; set; }
    public string EngagementOpenDateTo { get; set; }
    public string SECFlag { get; set; }
    public string EngagementReportingOrg { get; set; }

    public int BN_Fuzzy { get; set; }   //Client Fuzzy % - computed column
    public int AN_Fuzzy { get; set; }   //DUNS Name Fuzzy % - computed column
}
