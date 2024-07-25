namespace ConflictAutomation.Models.FinScan.SubClasses;

public class AssociationsType
{
    public string type { get; set; }

    public string value { get; set; }
    public string singleStringName { get; set; }
    public int listProfileKey { get; set; }
    public List<AdditionalAssocFieldsType> additionalAssocFields { get; set; }
}
