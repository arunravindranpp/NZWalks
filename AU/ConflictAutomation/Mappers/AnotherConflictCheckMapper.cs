using ConflictAutomation.Extensions;
using ConflictAutomation.Models;
using ConflictAutomation.Models.PreScreening.SubClasses;

namespace ConflictAutomation.Mappers;

public static class AnotherConflictCheckMapper
{
    private const string MSG_QUESTION_PURSUIT_CHECK = "Pursuit Check Performed";


    public static AnotherConflictCheck CreateFrom(List<QuestionnaireSummary> listQuestionnaires)
    {
        if (listQuestionnaires.IsNullOrEmpty())
        {
            return new();
        }

        var targetQuestion = listQuestionnaires.FirstOrDefault(IsQuestionConcerningPursuitCheck);

        return new()
        {
            Comments = targetQuestion.Explanation
        };
    }


    private static bool IsQuestionConcerningPursuitCheck(this QuestionnaireSummary question) =>
        question.Title.Equals(MSG_QUESTION_PURSUIT_CHECK, StringComparison.OrdinalIgnoreCase);
}
