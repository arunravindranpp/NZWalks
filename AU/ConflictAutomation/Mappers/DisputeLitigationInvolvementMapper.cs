using ConflictAutomation.Extensions;
using ConflictAutomation.Models.PreScreening.SubClasses;
using ConflictAutomation.Models;

namespace ConflictAutomation.Mappers;

public static class DisputeLitigationInvolvementMapper
{
    private const string MSG_QUESTION_DISPUTE_LITIGATION = "Dispute/Litigation";


    public static DisputeLitigationInvolvement CreateFrom(List<QuestionnaireSummary> listQuestionnaires)
    {
        if (listQuestionnaires.IsNullOrEmpty())
        {
            return new();
        }

        var targetQuestion = listQuestionnaires.FirstOrDefault(IsQuestionConcerningDisputeLitigation);

        return new()
        {
            Comments = targetQuestion.Explanation
        };
    }


    private static bool IsQuestionConcerningDisputeLitigation(this QuestionnaireSummary question) =>
        question.Title.Equals(MSG_QUESTION_DISPUTE_LITIGATION, StringComparison.OrdinalIgnoreCase);
}
