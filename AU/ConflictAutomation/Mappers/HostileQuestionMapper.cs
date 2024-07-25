using ConflictAutomation.Extensions;
using ConflictAutomation.Models;
using ConflictAutomation.Models.PreScreening.SubClasses;
using PACE.WebServices;

namespace ConflictAutomation.Mappers;

public static class HostileQuestionMapper
{ 
    private const string MSG_QUESTION_HOSTILE = "Hostile";
    private const string MSG_YES = "Yes";
    private const string MSG_NO = "No";
    private static readonly List<string> hostileQuestionNumbers = ["041349", "597835"];
    

    public static HostileQuestion CreateFrom(List<QuestionnaireSummary> listQuestionnaires)
    {
        if(listQuestionnaires.IsNullOrEmpty())
        {
            return new();
        }

        var targetQuestion = listQuestionnaires.FirstOrDefault(q => q.IsQuestionConcerningHostile() && q.IsAnswerYes())
                              ?? listQuestionnaires.FirstOrDefault(q => q.IsQuestionConcerningHostile() && q.IsAnswerNo());

        return new()
        {
            Question_1_4_Answer = targetQuestion?.Answer ?? string.Empty,
            Question_1_4_Comments = targetQuestion?.Explanation ?? string.Empty
        };
    }


    private static bool IsQuestionConcerningHostile(this QuestionnaireSummary question) => 
        question.Title.Equals(MSG_QUESTION_HOSTILE, StringComparison.OrdinalIgnoreCase) 
        && hostileQuestionNumbers.Contains(question.QuestionNumber);


    private static bool IsAnswerYes(this QuestionnaireSummary question) => 
        question.Answer.Equals(MSG_YES, StringComparison.OrdinalIgnoreCase);


    private static bool IsAnswerNo(this QuestionnaireSummary question) => 
        question.Answer.Equals(MSG_NO, StringComparison.OrdinalIgnoreCase);
}
