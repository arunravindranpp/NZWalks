namespace ConflictAutomation.Models.FinScan.SubClasses;

public class LinkedToType
{
    public string type { get; set; }

    public string id { get; set; }
    public List<AdditionalLinkedFieldsType> additionalLinkedFields { get; set; }
}
