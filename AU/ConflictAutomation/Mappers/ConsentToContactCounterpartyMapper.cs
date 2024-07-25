using ConflictAutomation.Extensions;
using ConflictAutomation.Models.PreScreening.SubClasses;
using ConflictAutomation.Models;

namespace ConflictAutomation.Mappers;

public static class ConsentToContactCounterpartyMapper
{
    private const string MSG_QUESTION_CONSENT_TO_CONTACT_COUNTERPARTY = "Whether to contact counterparty (G)CSP/audit partner?";


    public static ConsentToContactCounterparty CreateFrom(List<QuestionnaireSummary> listQuestionnaires)
    {
        if (listQuestionnaires.IsNullOrEmpty())
        {
            return new();
        }

        var targetQuestion = listQuestionnaires.FirstOrDefault(IsQuestionConcerningConsentToContactCounterparty);

        return new()
        {
            Comments = targetQuestion.Explanation
        };
    }


    private static bool IsQuestionConcerningConsentToContactCounterparty(this QuestionnaireSummary question) =>
        question.Title.Equals(MSG_QUESTION_CONSENT_TO_CONTACT_COUNTERPARTY, StringComparison.OrdinalIgnoreCase);
}
