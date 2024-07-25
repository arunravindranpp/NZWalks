using ConflictAutomation.Models.FinScan.SubClasses.enums;

namespace ConflictAutomation.Models.FinScan.SubClasses;

public class SearchMatch
{
    public string displayOrder { get; set; }
    public string listId { get; set; }
    public string listName { get; set; }
    public string version { get; set; }
    public int listProfileKey { get; set; }
    public string listProfileId { get; set; }
    public string recordType { get; set; }
    public string displayLine { get; set; }
    public YesNoEnum isParentFlag { get; set; }
    public DynamicFields dynamicFields { get; set; }
    public string fnsCategories { get; set; }
    public string rankString { get; set; }
    public double rankScore { get; set; }
    public RankTypeCodeEnum rankType { get; set; }
    public int configurationKey { get; set; }
    public int rankLabelKey { get; set; }
    public string rankValue { get; set; }
    public string reviewStatus { get; set; }
    public string reviewReason { get; set; }
    public string reviewComment { get; set; }
    public ListRecordDetail listRecordDetail { get; set; }
}
