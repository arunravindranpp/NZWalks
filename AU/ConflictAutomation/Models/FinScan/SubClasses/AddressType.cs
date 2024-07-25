namespace ConflictAutomation.Models.FinScan.SubClasses;

public class AddressType
{
    public string type { get; set; }
    public AddressLinesType addressLines { get; set; }
    public string cityLine { get; set; }
    public string country { get; set; }
    public string countryCode { get; set; }
    public List<AdditionalAddressFieldsType> additionalAddressFields { get; set; }
}
