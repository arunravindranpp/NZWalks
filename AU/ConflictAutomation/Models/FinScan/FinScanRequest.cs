using ConflictAutomation.Models.FinScan.SubClasses;
using ConflictAutomation.Models.FinScan.SubClasses.enums;
using PACE.Proxies.Mercury.Model;

namespace ConflictAutomation.Models.FinScan;

public class FinScanRequest
{
    public string organizationName { get; set; }
    public string userName { get; set; }
    public string password { get; set; }
    public string applicationId { get; set; }
    public SearchTypeEnum searchType { get; set; }
    public ClientSearchCodeEnum clientSearchCode { get; set; }
    public string clientId { get; set; }
    public ClientStatusEnum clientStatus { get; set; }
    public GenderEnum gender { get; set; }
    public string nameLine { get; set; }
    public YesNoEnum adverseMediaRequested { get; set; }
    public List<AlternateName> alternateNames { get; set; }
    public string addressLine1 { get; set; }
    public string addressLine2 { get; set; }
    public string addressLine3 { get; set; }
    public string addressLine4 { get; set; }
    public string addressLine5 { get; set; }
    public string addressLine6 { get; set; }
    public string addressLine7 { get; set; }
    public List<string> specificElement { get; set; }
    public List<int> userFieldsSearch { get; set; }
    public string userField1Label { get; set; }
    public string userField1Value { get; set; }
    public string userField2Label { get; set; }
    public string userField2Value { get; set; }
    public string userField3Label { get; set; }
    public string userField3Value { get; set; }
    public string userField4Label { get; set; }
    public string userField4Value { get; set; }
    public string userField5Label { get; set; }
    public string userField5Value { get; set; }
    public string userField6Label { get; set; }
    public string userField6Value { get; set; }
    public string userField7Label { get; set; }
    public string userField7Value { get; set; }
    public string userField8Label { get; set; }
    public string userField8Value { get; set; }
    public string comment { get; set; }
    public string passthrough { get; set; }
    public List<SubClasses.CustomStatus> customStatus { get; set; }
    public SearchReportTypeEnum generateSearchReports { get; set; }
    public SearchReportTypeCodeEnum reportTypeCode { get; set; }
    public string UBO_Id { get; set; }
    public List<int> userFieldsCountry { get; set; }
    public PremiumFields premiumFields { get; set; }
    public List<ComplianceList> lists { get; set; }
    public RequestOptions options { get; set; }
}
