using ConflictAutomation.Models.PreScreening.SubClasses;

namespace ConflictAutomation.Models.PreScreening;

public class PreScreeningInfo
{
    public List<TriggerForCheck> ListTriggersForCheck { get; set; }
    public List<Note> ListNotes { get; set; }
    public List<TeamMember> ListTeamMembers { get; set; }
    public List<AdditionalParty> ListAdditionalParties { get; set; }
    public HostileQuestion HostileQuestion { get; set; }
    public LimitationsToAct LimitationsToAct { get; set; }
    public AnotherConflictCheck AnotherConflictCheck { get; set; }
    public DisputeLitigationInvolvement DisputeLitigationInvolvement { get; set; }
    public HighProfileEngagement HighProfileEngagement { get; set; }
    public ConsentToContactCounterparty ConsentToContactCounterparty { get; set; }
}
