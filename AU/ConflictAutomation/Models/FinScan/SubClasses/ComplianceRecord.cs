using ConflictAutomation.Models.FinScan;

namespace ConflictAutomation.Models.FinScan.SubClasses;

public class ComplianceRecord
{
    public string applicationId { get; set; }
    public string clientId { get; set; }
    public long clientKey { get; set; }
    public string clientFullName { get; set; }
    public int listKey { get; set; }
    public string listName { get; set; }
    public string listId { get; set; }
    public string listVersion { get; set; }
    public string listModifyDate { get; set; }
    public string listProfileId { get; set; }
    public int listProfileKey { get; set; }
    public string linkSingleStringName { get; set; }
    public string listParentSingleStringName { get; set; }
    public string listCategory { get; set; }
    public string listPEPCategory { get; set; }
    public string listDOBs { get; set; }
    public string listCountries { get; set; }
    public string rankString { get; set; }
    public decimal rankScore { get; set; }
    public string ranktype { get; set; }
    public string rankweight { get; set; }
    public string pairLoadDate { get; set; }
    public string modifyDate { get; set; }
    public string modifiedByUser { get; set; }
    public List<string> finscanCategory { get; set; }
    public List<string> sourceLists { get; set; }
    public ListRecordDetail listRecordDetail { get; set; }
    public string listReviewLink { get; set; }
}
