namespace ConflictAutomation.Models.FinScan.SubClasses;

public class DowJonesListRecord
{
    public DowJonesPerson Person { get; set; }
    public DowJonesEntity Entity { get; set; }
    public List<Associate> Associations { get; set; }
    public string listId { get; set; }
    public string listVersion { get; set; }
    public DateTime loadDate { get; set; }
    public string deleted { get; set; }
    public string listCategory { get; set; }
    public int listProfileKey { get; set; }
    public string listType { get; set; }
}