namespace ConflictAutomation.Models.FinScan.SubClasses;

public class VesselType
{
    public string type { get; set; }
    public string callSign { get; set; }
    public string flag { get; set; }
    public string owner { get; set; }
    public string fullname { get; set; }
    public string tonnage { get; set; }
    public string vesseltype { get; set; }
    public string grossRegisteredTonnage { get; set; }
    public List<AdditionalVesselFieldsType> additionalVesselFields { get; set; }
}
