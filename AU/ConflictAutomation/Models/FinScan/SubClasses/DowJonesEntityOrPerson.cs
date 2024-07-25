namespace ConflictAutomation.Models.FinScan.SubClasses;

public class DowJonesEntityOrPerson
{
    // Common fields applicable to child classes DowJonesEntity and DowJonesPerson
    public string id { get; set; }
    public string action { get; set; }
    public string ActiveStatus { get; set; }
    public List<Name> NameDetails { get; set; }
    public List<Date> DateDetails { get; set; }
    public List<Description> Descriptions { get; set; }
    public List<ID> IDNumberTypes { get; set; }
    public List<Country> CountryDetails { get; set; }
    public string ProfileNotes { get; set; }
    public List<Source> SourceDescription { get; set; }
    public List<SanctionReference> SanctionsReferences { get; set; }
}
