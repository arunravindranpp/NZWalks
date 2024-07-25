namespace ConflictAutomation.Models.FinScan.SubClasses;

public class InnovativeListRecord
{
    public int listKey { get; set; }
    public string listId { get; set; }
    public string listVersion { get; set; }
    public string listType { get; set; }
    public int listProfileKey { get; set; }
    public NameAddressRecordType nameAddressRecord { get; set; }
    public SpecificRecordType specificRecord { get; set; }
    public string singleStringName { get; set; }
    public DateTime loadDate { get; set; }
    public string deleted { get; set; }
    public string listCategory { get; set; }
    public string brandingDisplayURL { get; set; }
    public string brandingHomeURL { get; set; }
    public string brandingCopyright { get; set; }
}
