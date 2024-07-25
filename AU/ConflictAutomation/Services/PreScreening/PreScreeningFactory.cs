using ConflictAutomation.Mappers;
using ConflictAutomation.Models;
using ConflictAutomation.Models.PreScreening;
using PACE;

namespace ConflictAutomation.Services.PreScreening;

public static class PreScreeningFactory
{
    public static PreScreeningInfo GetPreScreeningInfo
        (ConflictCheck conflictCheck, List<QuestionnaireSummary> listQuestionnaires,List<QuestionnaireAdditionalParties> listAdditionalParties) => new()
        {
            ListTriggersForCheck = TriggerForCheckMapper.CreateFrom(conflictCheck.Assessment?.conflictCheckTriggers), 
            ListNotes = NoteMapper.CreateFrom(conflictCheck.Notes),
            ListTeamMembers = TeamMemberMapper.CreateFrom(
                                conflictCheck.Assessment?.TeamMembers,
                                conflictCheck.ConflictCheckTeamMembers),
            ListAdditionalParties = AdditionalPartyMapper.CreateFrom(listAdditionalParties),
            HostileQuestion = HostileQuestionMapper.CreateFrom(listQuestionnaires),
            LimitationsToAct = LimitationsToActMapper.CreateFrom(listQuestionnaires),
            AnotherConflictCheck = AnotherConflictCheckMapper.CreateFrom(listQuestionnaires),
            DisputeLitigationInvolvement = DisputeLitigationInvolvementMapper.CreateFrom(listQuestionnaires),
            HighProfileEngagement = HighProfileEngagementMapper.CreateFrom(listQuestionnaires),
            ConsentToContactCounterparty = ConsentToContactCounterpartyMapper.CreateFrom(listQuestionnaires)
        };
}
