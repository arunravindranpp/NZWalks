using ConflictAutomation.Models.FinScan.SubClasses.enums;

namespace ConflictAutomation.Models.FinScan.SubClasses;

public class ClientRiskDetail
{
    public ClientRiskRatingCodeEnum overallClientRiskRating { get; set; }
    public int overallClientRiskScore { get; set; }
    public int cipRiskScore { get; set; }
    public int screeningResultsRiskScore { get; set; }
    public List<ClientExtendedRisk> clientExtendedRisk { get; set; }
    public List<ClientRiskHistory> clientRiskHistory { get; set; }
}
