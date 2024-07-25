namespace ConflictAutomation.Models.FinScan.SubClasses;

public class NameAddressRecordType
{
    public string id { get; set; }
    public string active { get; set; }
    public string recordType { get; set; }
    public List<TrackingInformationType> trackingInformation { get; set; }
    public List<InnovativePerson> person { get; set; }
    public List<InnovativeEntity> entity { get; set; }
    public List<PhysicalAttributesType> physicalAttributes { get; set; }
    public List<SocialAttributesType> socialAttributes { get; set; }
    public List<VesselType> vessel { get; set; }
    public List<AddressType> address { get; set; }
    public List<DatesType> dates { get; set; }
    public List<IDNumbersType> idNumbers { get; set; }
    public List<TextInfoType> textInfo { get; set; }
    public List<AssociationsType> associations { get; set; }
    public List<LinkedToType> linkedTo { get; set; }
    public List<URLsType> URLS { get; set; }
    public List<AdditionalInformationType> additionalInformation { get; set; }
}
