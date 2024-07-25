namespace ConflictAutomation.Models.FinScan.SubClasses;

public class SearchResult
{
    public string searchName { get; set; }
    public string clientId { get; set; }
    public long clientKey { get; set; }
    public string clientName { get; set; }
    public string specificElement { get; set; }
    public int returned { get; set; }
    public int notReturned { get; set; }
    public int sequenceNumber { get; set; }
    public DateTime searchDateTime { get; set; }
    public List<SearchMatch> searchMatches { get; set; }
    public List<SearchReport> searchReports { get; set; }
}
