using ConflictAutomation.Extensions;
using ConflictAutomation.Models.PreScreening.SubClasses;
using ConflictAutomation.Models;

namespace ConflictAutomation.Mappers;

public static class HighProfileEngagementMapper
{
    private const string MSG_QUESTION_HIGH_PROFILE = "High Profile";


    public static HighProfileEngagement CreateFrom(List<QuestionnaireSummary> listQuestionnaires)
    {
        if (listQuestionnaires.IsNullOrEmpty())
        {
            return new();
        }

        var targetQuestion = listQuestionnaires.FirstOrDefault(IsQuestionConcerningHighProfile);

        return new()
        {
            YesNo = targetQuestion?.Answer,
            Comments = targetQuestion.Explanation
        };
    }


    private static bool IsQuestionConcerningHighProfile(this QuestionnaireSummary question) =>
        question.Title.Equals(MSG_QUESTION_HIGH_PROFILE, StringComparison.OrdinalIgnoreCase);
}
