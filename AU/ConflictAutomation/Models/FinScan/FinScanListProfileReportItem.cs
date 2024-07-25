using ConflictAutomation.Models.FinScan.SubClasses;

namespace ConflictAutomation.Models.FinScan;

public class FinScanListProfileReportItem
{
    public string ListId { get; init; }
    public string Name { get; init; }
    public string Gender { get; init; }
    public string RecordType { get; init; }
    public string Uid  { get; init; }
    public string Status { get; init; }
    public string LoadDate { get; init; }
    public string OriginalScriptName { get; init; }
    public string Version { get; init; }
    public string Deleted { get; init; }

    public List<Date> DatesColl { get; init; }
    public List<Description> DescriptionsColl { get; init; }
    public List<SanctionReference> SanctionsReferencesColl { get; init; }
    public List<Name> NamesColl { get; init; }
    public List<Address> AddressesColl { get; init; }
    public List<ID> IdsColl { get; init; }
    public List<Country> CountriesColl { get; init; }
    public List<Place> BirthPlaceColl { get; init; }
    public List<Associate> AssociatesColl { get; init; }
    public string ProfileNotes { get; init; }
    public List<Image> ImagesColl { get; init; }
    public List<Source> SourcesColl { get; init; }


    public List<InnovativeAddress> InnovativeAddressesColl { get; init; }
    public List<InnovativeDate> InnovativeDatesColl { get; init; }
    public List<string> InnovativeEntitiesColl { get; init; }
    public List<string> InnovativePersonsColl { get; init; }
    public List<InnovativeID> InnovativeIDsColl { get; init; }
    public List<InnovativeTextInformation> InnovativeTextInformationsColl { get; init; }
    public List<InnovativeTrackingInformation> InnovativeTrackingInformationsColl { get; init; }
    public List<string> InnovativeOriginalTypesColl { get; init; }
    public List<InnovativeURL> InnovativeURLsColl { get; init; }
    
}
