
namespace ConflictAutomation.Models
{
    public class CheckerQueue
    {
        public long ConflictCheckID { get; set; }
        public string ConflictCheckType { get; set; }
        public string Region { get; set; }
        public string ClientName { get; set; }
        public string CountryName { get; set; }
        public string EngagementName { get; set; }
        public string SubServiceLineCode { get; set; }
        public string SubServiceLine { get; set; }
        public string Services { get; set; }
        public string onShore { get; set; }
        public string EngagementDesc { get; set; }
        public bool IsUKI { get; set; }
        public bool IsCRRGUP { get; set; }
        public bool pursuitCheckPerformed { get; set; }
        public string pursuitCheckID { get; set; }
        public long AssessmentID { get; set; }
        public string Confidential { get; set; }
        public string checkPerformed { get; set; }
        public string SubTypeofCheck { get; set; }
        public string AttachmentsinPACE{ get; set; }
        public string ConfidentialPace { get; set; }
        public string Sanctioned { get; set; }
        public string Contractuallylimit { get; set; }
        public string DisputeLitigation { get; set; }
        public string Hostile { get; set; }
        public string Auction { get; set; }
        public string AuditPartner { get; set; }
        public string TimeStamp { get; set; }
        public string GovtEntity { get; set; }
        public string CheckerName { get; set; }
        public string ReviewerName { get; set; }
        public string Case { get; set; }
        public string CaseDescription { get; set; }
        public string Issue { get; set; }
        public string IssueDescription { get; set; }
        public string Conclusion { get; set; }
        public string ConclusionDescription { get; set; }
        public string Condition1 { get; set; }
        public string Condition1Description { get; set; }
        public string Condition2 { get; set; }
        public string Condition2Description { get; set; }
        public string Condition3 { get; set; }
        public string Condition3Description { get; set; }
        public string Condition4 { get; set; }
        public string Condition4Description { get; set; }
        public List<ResearchSummary> researchSummary { get; set; }
        public List<QuestionnaireSummary> questionnaireSummary { get; set; }
        public List<QuestionnaireAdditionalParties> questionnaireAdditionalParties { get; set; }
    }

    public class ResearchSummary
    {
        public int SummaryRowNo { get; set; }
        public int BotUnitRowNo { get; set; }
        public string WorksheetNo { get; set; }
        public int OrderNo { get; set; }
        public string EntityName { get; set; }
        public string Role { get; set; }
        public bool IsClientSide { get; set; }
        public bool NoEYRelationshipWithCounterparties { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string DUNSNumber { get; set; }
        public string MDMID { get; set; }
        public string GISID { get; set; }
        public long AdditionalPartyID { get; set; }
        public string AdditionalComments { get; set; }
        public string Type { get; set; }
        public string GIS { get; set; } = "NA";
        public string Mercury { get; set; } = "NA";
        public string CRR { get; set; } = "NA";
        public string SPL { get; set; } = "NA";
        public string Finscan { get; set; } = "NA";
        public string SourceSystem { get; set; } = "PACE APG";
        public bool Sanctions { get; set; }
        public bool PerformResearch {get;set;}
        public string FinScanListProfileReportFilePath { get; set; }
        public string FinScanMatchReportFilePath { get; set; }
        public string EntityWithoutLegalExt { get; set; }
        public string ParentalRelationship { get; set; }
        public bool IsSPLResearch { get; set; } = true;
        public bool IsGISResearch { get; set; } = true;
        public bool IsCERResearch { get; set; } = true;
        public bool IsFinscanResearch { get; set; } = true;
        public bool IsCRRResearch { get; set; } = true;
        public bool IsGISAdditionalBNFuzzy { get; set; } = false;
        public ClientRelationshipSummary ClientRelationshipSummary { get; set; }
        public bool Rework { get; set; } = false;
        public string PartyInvolved { get; set; }
        public string Summary { get; set; }
        public bool IsAdditionalLoop { get; set; } = false;        
    }
    public class QuestionnaireSummary
    {
        public string Title { get; set; }
        public string Answer { get; set; }
        public string Explanation { get; set; }
        public string QuestionNumber { get; set; }
    }
    public class ClientRelationshipSummary
    {
        public string SearchDesc { get; set; } = "";
        public string Duns { get; set; }
        public string GCSP { get; set; }
        public string LAP { get; set; }
        public string Restrictions { get; set; }
        public string PIE { get; set; }
        public string PIEAffiliate { get; set; }
        public string GISNotes { get; set; }
        public string GISDesc { get; set; }
        public string CERNotes { get; set; }
        public string CERDesc { get; set; }
    }
    public class QuestionnaireAdditionalParties 
    {
        public string Name { get; set; }
        public string Position { get; set; }
        public string OtherInformation { get; set; }
    }
}
