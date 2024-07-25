namespace ConflictAutomation.Models.FinScan.SubClasses;

public class ListRecord
{
    public DowJonesListRecord dowJones { get; set; }
    public DowJonesListRecord dowJonesAdverseMediaEntities { get; set; }
    public InnovativeListRecord innovative { get; set; }
}