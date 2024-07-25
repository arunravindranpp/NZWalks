namespace ConflictAutomation.Models.FinScan.SubClasses;

public class ClientRiskHistory
{
    public int overallClientRiskRating { get; set; }
    public int overallClientRiskScore { get; set; }
    public int cipRiskScore { get; set; }
    public int screeningResultsRiskScore { get; set; }
    public DateTime modifyDate { get; set; }
}
