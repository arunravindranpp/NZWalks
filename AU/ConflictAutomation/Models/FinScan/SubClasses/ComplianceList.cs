namespace ConflictAutomation.Models.FinScan.SubClasses;

public class ComplianceList
{
    public string listName { get; set; }
    public string listId { get; set; }
    public int listType { get; set; }
    public List<ComplianceListCategory> listCategories { get; set; }
}
