using ConflictAutomation.Models;
using ConflictAutomation.Models.PreScreening;
using ConflictAutomation.Models.PreScreening.SubClasses;
using PACE;
using Serilog;
namespace ConflictAutomation.Services.PreScreening;

public static class PreScreeningOperations
{
    public static void WritePrescreeningTab(
        AppConfigure configuration, long conflictCheckID, ConflictCheck conflictCheck,
        List<QuestionnaireSummary> listQuestionnaires, List<QuestionnaireAdditionalParties> listAdditionalParties ,List<ApprovalRungQuestion> processQuestionnaire,string destinationFilePath)
    {
        try
        {
            var preScreeningInfo = PreScreeningFactory.GetPreScreeningInfo(conflictCheck, listQuestionnaires,listAdditionalParties);
            if (processQuestionnaire != null && processQuestionnaire.Any())
            {
                var triggerForCheckList = new List<TriggerForCheck>();
                foreach (var item in processQuestionnaire)
                {
                    TriggerForCheck triggerForCheck = new TriggerForCheck();
                    triggerForCheck.TriggerType = item.QuestionType;
                    if (item.QuestionType == "Question")
                    {
                        triggerForCheck.Details = "Q:" + item.Question + "\n" + "A:" + item.Answer;
                    }
                    else if(item.QuestionType == "Evaluation")
                    {
                        triggerForCheck.Details = "Evaluation:" +item.Section;
                    }
                    triggerForCheckList.Add(triggerForCheck);
                }
                if(triggerForCheckList.Any())
                {
                    var questionTrigger = preScreeningInfo.ListTriggersForCheck.FirstOrDefault(i => i.TriggerType == "CONCH");
                    preScreeningInfo.ListTriggersForCheck?.Remove(questionTrigger);
                    preScreeningInfo.ListTriggersForCheck.AddRange(triggerForCheckList);
                }
            }
            var preScreeningWriter = new PreScreeningWriter(destinationFilePath);
            preScreeningWriter.Run(preScreeningInfo);
            Console.WriteLine("Prescreening Completed");
            Log.Information($"SavePrescreeningTab completed for: {conflictCheckID}");
            Log.Information($"_configuration.SPLLocalPath: {configuration.SPLLocalPath}");
        }
        catch (Exception ex)
        {
            Log.Error("An error occurred when running method PreScreeningOperations.WritePrescreeningTab()" + 
                      $" for conflictCheckID {conflictCheckID}. Message: {ex}");
            LoggerInfo.LogException(ex);
        }
    }
    public static void WritePrescreeningPursuitTab(PreScreeningInfo preScreeningInfo, string destinationFilePath)
    {
        try
        {
            var preScreeningWriter = new PreScreeningWriter(destinationFilePath);
            preScreeningWriter.Run(preScreeningInfo,true);
            Console.WriteLine("Prescreening Pursuit Completed");
        }
        catch (Exception ex)
        {
            Log.Error("An error occurred when running method PreScreeningOperations.WritePrescreeningPursuitTab()" +
                      $"Message: {ex}");
            LoggerInfo.LogException(ex, "WritePrescreeningPursuitTab");
        }
    }
    public static void WritePrescreeningResubmittedTab(PreScreeningInfo preScreeningInfo, string destinationFilePath)
    {
        try
        {
            var preScreeningWriter = new PreScreeningWriter(destinationFilePath);
            preScreeningWriter.Run(preScreeningInfo, false,true);
            Console.WriteLine("Prescreening Pursuit Completed");
        }
        catch (Exception ex)
        {
            Log.Error("An error occurred when running method PreScreeningOperations.WritePrescreeningResubmittedTab()" +
                      $"Message: {ex}");
            LoggerInfo.LogException(ex, "WritePrescreeningResubmittedTab");
        }
    }
}
