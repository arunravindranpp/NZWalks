namespace ConflictAutomation.Models.FinScan.SubClasses;

public class ClientExtendedRisk
{
    public string clientRiskSectionMnemonic { get; set; }
    public List<ClientRiskSubSection> clientRiskSubsections { get; set; }
}
