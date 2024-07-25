using ConflictAutomation.Extensions;
using ConflictAutomation.Models.PreScreening.SubClasses;
using ConflictAutomation.Models;

namespace ConflictAutomation.Mappers;

public static class LimitationsToActMapper
{
    private const string MSG_QUESTION_LIMITATIONS_TO_ACT = "Contractually limit our ability";


    public static LimitationsToAct CreateFrom(List<QuestionnaireSummary> listQuestionnaires)
    {
        if (listQuestionnaires.IsNullOrEmpty())
        {
            return new();
        }

        var targetQuestion = listQuestionnaires.FirstOrDefault(IsQuestionConcerningLimitationsToAct);

        return new() 
        { 
            YesNo = targetQuestion.Answer 
        };
    }


    private static bool IsQuestionConcerningLimitationsToAct(this QuestionnaireSummary question) =>
        question.Title.Equals(MSG_QUESTION_LIMITATIONS_TO_ACT, StringComparison.OrdinalIgnoreCase);
}
