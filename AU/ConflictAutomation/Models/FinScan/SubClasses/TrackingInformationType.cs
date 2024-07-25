namespace ConflictAutomation.Models.FinScan.SubClasses;

public class TrackingInformationType
{
    public string type { get; set; }
    public string value { get; set; }
    public List<AdditionalTrackingFieldsType> additionalTrackingFields { get; set; }
}
