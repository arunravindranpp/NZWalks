using ConflictAutomation.Extensions;
using ConflictAutomation.Models;
using Serilog;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace ConflictAutomation.Services;

#pragma warning disable IDE0017 // Simplify object initialization
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable CA1416 // Validate platform compatibility
internal class LoggerInfo
{
    /// <summary>
    /// Pass the exception object to this method
    /// </summary>
    /// <param Name="ex"></param>
    public static void LogException(Exception ex, string message ="")
    {
        try
        {
            string errorMessage = $"{ex.Message};{((ex.InnerException is null) ? string.Empty : ex.InnerException)}";
            message = message.Trim();
            message = (!string.IsNullOrEmpty(ex.StackTrace)) && (!string.IsNullOrEmpty(message)) 
                      && (!ex.StackTrace.Contains(message)) ? message : string.Empty;
            EYSql.ExecuteNonQuery(
                Program.PACEConnectionString,
                CommandType.Text,
                @"INSERT INTO CAU_ExceptionLog 
                                (TimeOfOccurence,ErrorMessage,StackTrace)          
                        VALUES (getdate(), @errorMessage, @stackTrace)"

               , new SqlParameter("@errorMessage", errorMessage)
               , new SqlParameter("@stackTrace", $"{ex.StackTrace} {message}".Trim()));
        }
        catch(Exception exe) {
            string errorMessage = $"{exe.Message};{exe.InnerException}";
            Log.Information(errorMessage);
        }
    }
    public static void LogThreeTrials<T>(T checks, int recurrence, string error, string env = "") where T : ICheckInfo
    {
        try
        {
            EYSql.ExecuteNonQuery(
                Program.PACEConnectionString,
                CommandType.Text,
                @"INSERT INTO CAU_ThreeTrials 
                                (ConflictCheckID,TimeOfOccurence,Recurrence,ErrorMessage,Environment)          
                        VALUES (@ConflictCheckID,getdate(),@Recurrence, @errorMessage,@environment)"
               , new SqlParameter("@ConflictCheckID", checks.CheckId)
               , new SqlParameter("@Recurrence", recurrence)
               , new SqlParameter("@errorMessage", error)
               , new SqlParameter("@environment", env));
        }
        catch (Exception exe)
        {
            string errorMessage = $"{exe.Message};{exe.InnerException}";
            Log.Information(errorMessage);
        }
    }
    public static void WriteEventLog(Exception ex)
    {
        try
        {
            string errorMessage = $"{ex.Message};{ex.InnerException};{ex.StackTrace}";
            EventLog eventLog = new EventLog("Application");
            eventLog.Source = "Application";
            eventLog.WriteEntry(errorMessage, EventLogEntryType.Error);
        }
        catch (Exception)
        {
            //Do Nothing
        }
    }


    public static void DefaultLogAction(Exception ex, string message = "")
    {
        string text = message.Replace("\n", " ").FullTrim();

        if (text.FullTrim().IsNullOrEmpty())
        {
            LogException(ex);  // Writes log entry to SQL table CAU_ExceptionLog
        }
        else
        { 
            LogException(ex, $" {text}");  // Writes log entry to SQL table CAU_ExceptionLog
            Log.Information(text);  // Writes log entry to .txt Log file
            Console.WriteLine($"     {text}");
        }
    }
}
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0017 // Simplify object initialization
