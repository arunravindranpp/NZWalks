namespace ConflictAutomation.Models.FinScan.SubClasses;

public class AdditionalInformationType
{
    public string type { get; set; }
    public string value { get; set; }
    public List<AdditionalInfoFieldsType> additionalFields { get; set; }
}
