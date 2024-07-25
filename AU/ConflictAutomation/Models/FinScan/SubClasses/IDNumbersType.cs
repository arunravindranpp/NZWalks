namespace ConflictAutomation.Models.FinScan.SubClasses;

public class IDNumbersType
{
    public string type { get; set; }
    public string subtype { get; set; }
    public string value { get; set; }
    public string countryIssued { get; set; }
    public string dateIssued { get; set; }
    public List<AdditionalIDNumbersFieldsType> additionalIDNumbersFields { get; set; }
}
