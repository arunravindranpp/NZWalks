namespace ConflictAutomation.Models.FinScan.SubClasses;

public class PhysicalAttributesType
{
    public string type { get; set; }
    public string value { get; set; }
    public List<PhysicalAttributesType> additionalPhysAttrFields { get; set; }
}
