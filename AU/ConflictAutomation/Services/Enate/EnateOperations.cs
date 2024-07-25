using ConflictAutomation.Extensions;
using ConflictAutomation.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using PACE;
using PACE.Domain.Services;
using Serilog;
using System.Data;
using System.Data.SqlClient;
using Nest;

namespace ConflictAutomation.Services.Enate;

#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable CS0168 // Variable is declared but never used
#pragma warning disable IDE0044 // Add readonly modifier
public class EnateOperations
{

    public List<string> Priority_1 = new List<string> { "AM", "AM", "AM", "AP", "AP", "AP", "AP", "AP", "AP", "AP", "AP", "AP", "EM", "EM", "EM", "EM", "EM", "EM", "AM", "AM", "AM", "AM", "AM", "AM" };
    public List<string> Priority_2 = new List<string> { "EM", "AP", "AP", "AM", "AM", "EM", "EM", "EM", "EM", "EM", "EM", "EM", "AP", "AP", "AP", "AP", "AP", "AP", "EM", "EM", "EM", "EM", "EM", "EM" };
    public List<string> Priority_3 = new List<string> { "AP", "EM", "EM", "EM", "EM", "AM", "AM", "AM", "AM", "AM", "AM", "AM", "AM", "AM", "AM", "AM", "AM", "AM", "AP", "AP", "AP", "AP", "AP", "AP" };

    TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

    // Unused variables
    // private static readonly bool bProcessGIS = true;
    // private static readonly bool bProcessFinScan = true;
    // private static readonly bool bCRR = true;
    // private static readonly bool bMercury = true;

    public static bool DoWork(CaseCreation cc)
    {
        bool setToDo = false;
        //1. Authenticate 
        string authToken = EnateCore.Authenticate();

        //1. Create a Case
        CasePacket _create = CreateCase(authToken, cc);

        if (string.IsNullOrEmpty(_create.Result.Reference))
        {
            return false;
        }
        else
        {
            //2. SetTodo with created Case.
            CasePacket _setToDo = new CasePacket();
            Thread.Sleep(1000);
            setToDo = EnateCore.Case_SetToDo(authToken, _create, cc.checkInfo.CheckProcess);//From Draft to ToDo in Enate
            Thread.Sleep(1000);
            if (!setToDo)
                return false;
        }

        //3.GetMoreWork
        Work_GetMoreWork _getMoreWork = EnateCore.Work_GetMoreWork(authToken);
        /*
         Notes:
            1. When Work_GetMoreWork is called, Enate will assign one action irrespective of the region. 
            2. AU has to identify if it is live region or not
                2.a. Now, if not live, then do the Work_GetMoreWork API call again. Also, store the current action GUID in AU's temp file with the region name and due date.
                2.b. If live region action, proceed to next step. ie, set to inprocess. API is /Action/SetInProgress
            3. Once all live regions are processed, when GetMoreWork is performed, the error message will be like 'No content'
            4. Now, AU have to goto the temp file, filter for the next live region and filter for priority by Due Date column and pick that action GUID column. Then it has to do a call to API /Action/Get.
            5. When two regions are live, TBD           

        */

        //4. Get Case //Todo. Dilip/Tejal. This needs to be converted to /Action/Get 
        //string caseGUID = _setToDo.Result.GUID;
        //string existingPacketActivityGUID = string.Empty;
        //ProcessContext_Processes _getCase = EnateCore.Case_Get(authToken, caseGUID, existingPacketActivityGUID);

        return true;
    }
    public static bool CaseCreation(CaseCreation cc, out string errorMessage)
    {
        errorMessage = "";
        bool setToDo = false;
        try
        {
            //1. Authenticate 
            string authToken = EnateCore.Authenticate();

            if (string.IsNullOrEmpty(authToken))
                return false;
            else
            {
                //1. Create a Case
                CasePacket _create = CreateCase(authToken, cc);

                if (_create == null)
                    return false;

                //2. SetTodo with created Case.
                CasePacket _setToDo = new CasePacket();
                if (!string.IsNullOrEmpty(_create.Result.Reference))
                {
                    Thread.Sleep(1000);
                    setToDo = EnateCore.Case_SetToDo(authToken, _create, cc.checkInfo.CheckProcess);//From Draft to ToDo in Enate
                    Thread.Sleep(1000);
                    if (!setToDo)
                        return false;
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"{ex.Message};{ex.InnerException}";
            return false;
        }
    }
    public static CasePacket CreateCase(string authToken, CaseCreation cc)
    {
        if (!string.IsNullOrEmpty(authToken))
        {

            try
            {
                List<ProcessContext_Company> lsCompanies = EnateCore.GetCompanies(authToken);

                if (lsCompanies.Count > 0)
                {
                    string companyGUID = lsCompanies.FirstOrDefault().GUID;

                    if (companyGUID != null)
                    {
                        ProcessContext_Contracts processContext_Contracts = EnateCore.GetContracts(authToken, companyGUID, 0, 100);
                        if (processContext_Contracts.Items.Count > 0)
                        {
                            string contractGUID = processContext_Contracts.Items.Where(x => x.Name.ToLower().Equals(cc.checkInfo.Region.ToLower())).FirstOrDefault().GUID;

                            if (contractGUID != null)
                            {
                                List<ProcessContext_Services> processContext_Services = EnateCore.GetServices(authToken, contractGUID);

                                if (processContext_Services.Count > 0)
                                {
                                    string serviceGUID = processContext_Services.Where(x => x.Name.ToLower().Equals(cc.checkInfo.ServiceLine.ToLower())).FirstOrDefault().GUID;

                                    ProcessContext_Processes processContext_Processes = EnateCore.GetProcesses(authToken, serviceGUID, 0, 100);

                                    if (processContext_Processes.Items.Count > 0)
                                    {
                                        var processContext_Processes_V8 = processContext_Processes.Items.Where(y => y.VersionState == 8 && y.ProcessType == 1).ToList(); //1: Case

                                        //string processGuid = processGuid = processContext_Processes_V8.Where(x => x.Name.Equals("AU QA Checks"))
                                        //                                                .FirstOrDefault().AttributeVersionGuid;

                                        string processGuid = processGuid = processContext_Processes_V8.Where(x => x.Name.Equals("All checks"))
                                                                                        .FirstOrDefault().AttributeVersionGuid;


                                        CasePacket casePacket = EnateCore.Case_CreateCase(authToken, processGuid, cc.checkInfo);
                                        return casePacket;
                                    }
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error creating case {cc.checkInfo.CheckId}");
                return null;
            }
        }

        return new CasePacket();
    }

    public void ProcessENateRequest(ConflictService conflictService, Microsoft.Extensions.DependencyInjection.ServiceProvider _serviceProvider, AppConfigure config, PACE.Domain.Models.UserSessionViewModel US)
    {
        //1. Authenticate 
        string authToken = EnateCore.Authenticate();

        //var test = EnateCore.Action_Get("2d184ea5-183f-ef11-921e-bd8ee4ec1289", authToken);
        bool _completedWork = false;
        if (authToken != null)
        {
            conflictService.UpdateFileshare();
            while (!_completedWork)
            {
                string _Priority1;
                string _Priority2;
                string _Priority3;
                Work_GetMoreWork _getMoreWork;

                //Get conflict check in queue first for the current region
                Log.Logger.Information("Current time in UTC:" + DateTime.UtcNow.ToString());
                DateTime ist_now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);
                Log.Logger.Information("Converted time in IST:" + ist_now.ToString());
                List<string> lstPriority = conflictService.GetPriority(ist_now);
                if (lstPriority != null)
                {
                    _Priority1 = lstPriority[0].ToString().Trim();
                    _Priority2 = lstPriority[1].ToString().Trim();
                    _Priority3 = lstPriority[2].ToString().Trim();
                }
                else
                {
                    _Priority1 = Priority_1[ist_now.Hour];
                    _Priority2 = Priority_2[ist_now.Hour];
                    _Priority3 = Priority_3[ist_now.Hour];
                }

                Log.Logger.Information("Region priority: " + $"{_Priority1}, {_Priority2}, {_Priority3}");
                string _currentPriority = _Priority1;

                //Moved to its own process

                //_getMoreWork = EnateCore.Work_GetMoreWork(authToken);
                //if (_getMoreWork == null)
                //{
                //    Log.Logger.Information("No work received from eNate.");
                //}
                //else
                //{
                //    var conflictCheckId = EnateCore.Action_Get(_getMoreWork.GUID, authToken);
                //    _getMoreWork.ConflictCheckId = conflictCheckId.DataFields.CheckId; ;
                //    Console.WriteLine("Retrived Case: " + _getMoreWork.Reference + " with conflict check id: " + _getMoreWork.ConflictCheckId);
                //    conflictService.AddWorkToQueue(_getMoreWork, false);
                //    Console.WriteLine("Added case to work queue");
                //}

                Console.WriteLine("Getting case from work queue");
                _getMoreWork = conflictService.GetWorkQueueItem(_currentPriority);


                //_getMoreWork.ConflictCheckId = "1412657"; //Debug
                if (_getMoreWork == null)
                {
                    _getMoreWork = conflictService.GetWorkQueueItem(_Priority2);
                    _currentPriority = _Priority2;
                }
                if (_getMoreWork == null)
                {
                    _getMoreWork = conflictService.GetWorkQueueItem(_Priority3);
                    _currentPriority = _Priority3;
                }


                if (_getMoreWork != null)
                {
                    Console.WriteLine("Processing Case: " + _getMoreWork.Reference + " with conflict check id: " + _getMoreWork.ConflictCheckId);

                    //Assign case in eNate to the current eNate login
                    //authToken = EnateCore.Authenticate();
                    Log.Information($"Assign user: {Program.eNateUserGUID} for case {_getMoreWork.GUID}");
                    UserAssign(Program.eNateUserGUID, _getMoreWork.GUID, authToken);

                    ConflictCheck conflictCheck = new ConflictCheck();
                    var ProcessedLog = new ProcessedChecks();
                    long ProcessLogID = 0;
                    ProcessLog processlog = new ProcessLog(config);

                    if (_getMoreWork.ContractName.StartsWith(_currentPriority))
                    {
                        conflictService.AddWorkToQueue(_getMoreWork, true); //won't add to queue here, just update with now as start date

                        //Set action to in progress
                        authToken = EnateCore.Authenticate();
                        Log.Information($"Set In Progress for {_getMoreWork.Reference}, {_getMoreWork.Title}");
                        EnateCore.Action_SetInProgress(_getMoreWork, authToken);

                        string Rework = "No";
                        bool MultiEntity = false;


                        if (!_getMoreWork.ConflictCheckId.IsNullOrEmpty() && _getMoreWork.ConflictCheckId != "0")
                        {
                            if (_getMoreWork.ProcessName == "Multi Entity AU PreScreening")//ProcessNames.MultiEntityCheck)
                                MultiEntity = true;
                            //Usual flow of conflict process
                            long _conflictCheckId = Convert.ToInt64(_getMoreWork.ConflictCheckId);
                            ProcessedChecks result = new ProcessedChecks();
                            //----------- comment this section to clear eNate queue
                            try
                            {
                                Log.Information($"Get conflict check {_conflictCheckId} from PACE");
                                conflictCheck = _serviceProvider.GetService<IConflictCheckServices>().GetConflictCheckByID(_conflictCheckId);
                            }
                            catch (Exception e)
                            {
                                Log.Information($"Get conflict check error from PACE for {_getMoreWork.ConflictCheckId}: " + e.Message);
                                result.IsErrored = true;
                                result.ErrorMessage = e.Message;
                            }

                            if (_getMoreWork.Reference.Contains('(')) //Reference containing parenthesis indicate the case is a rework
                                Rework = "Yes";

                            //Set the substatus
                            if (!result.IsErrored)
                            {

                                ConflictCheckSubStatus CSS = new ConflictCheckSubStatus();
                                DataTable dtSubStatus = EYSql.ExecuteDataset(config.ConnectionString, CommandType.Text,
                                           @"Select * from WF_ConflictCheckSubStatus WHERE ShortVersion ='Research in progress' ").Tables[0];
                                foreach (DataRow drSubStatus in dtSubStatus.Rows)
                                {
                                    DBMarshaller.Load(dtSubStatus.Columns, drSubStatus, CSS);
                                }

                                var logger = _serviceProvider.GetService<ILogger<ConflictCheckServices>>();

                                try
                                {

                                    result.IsErrored = false;
                                    ProcessedLog.ProcessStart = DateTime.Now.TimestampWithTimezoneFromLocal("India Standard Time", "", "yyyy-MM-ddTHH:mm:ss");
                                    ProcessLogID = processlog.StartLog(_conflictCheckId, ProcessedLog.ProcessStart, config.Environment);

                                    Log.Information($"Update ownership for {_conflictCheckId} in PACE");
                                    conflictCheck = _serviceProvider.GetService<IConflictCheckServices>().AssignOwnership(_conflictCheckId, US.GUI.ToString(), true, logger);

                                    conflictCheck.ConflictCheckSubStatus = _serviceProvider.GetService<IConflictCheckServices>().SaveAdminSubStatus(CSS, US.GUI.ToString());
                                    conflictCheck.SubStatusID = Convert.ToInt16(conflictCheck.ConflictCheckSubStatus.SubStatusID);
                                    Log.Information($"Save conflict checks for {_conflictCheckId} in PACE");
                                    _serviceProvider.GetService<IConflictCheckServices>().SaveConflictCheck(conflictCheck, US.GUI.ToString());
                                    Log.Information("Check Processed-" + _conflictCheckId);
                                    Console.WriteLine("\nConflictCheckID Processed: " + _conflictCheckId);
                                    result = conflictService.ProcessCheck(_conflictCheckId, ProcessLogID, ProcessedLog.ProcessStart, conflictCheck, _serviceProvider, Rework == "Yes", MultiEntity);

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Error processing conflict check id: " + _conflictCheckId);
                                    Console.WriteLine($"{ex.Message}");
                                    logger.LogError(ex.Message);
                                    result.IsErrored = true;
                                    result.ErrorMessage = ex.Message;
                                }

                            }
                            //----------------------------------

                            Log.Information($"Update end time for {_conflictCheckId}");
                            conflictService.UpdateWorkQueue(_getMoreWork);
                            authToken = EnateCore.Authenticate(); //reauthenticate in case the current session timeout because the processing takes too long.
                            if (!result.IsErrored)
                            {
                                Log.Information($"{_conflictCheckId} resolved successfully");
                                //Mark conflict check as complete in queue
                                EnateCore.Action_ResolveSuccessfully(_getMoreWork, authToken);
                            }
                            else
                            {
                                Log.Information($"{_conflictCheckId} resolved unsuccessfully, {result.ErrorMessage}");
                                //SEt action completed unsuccessfully in enate with a comment
                                EnateCore.Action_ResolveUnSuccessfully(_getMoreWork, authToken, result.ErrorMessage);
                            }
                        }
                        else
                        {
                            Log.Information("Case " + _getMoreWork.Reference + " have conflict check id empty or 0");
                            EnateCore.Action_SetInProgress(_getMoreWork, authToken);
                            EnateCore.Action_ResolveUnSuccessfully(_getMoreWork, authToken, "Conflict check id field is empty or is 0");
                        }

                        _completedWork = true;
                    }
                    else
                    {
                        //conflictService.AddWorkToQueue(_getMoreWork, false);
                        throw new Exception("No work received or in work queue for this run.");
                    }
                }
                else
                {
                    break; //Break the loop and exit when there's no more work
                    throw new Exception("No work received or in work queue for this run.");
                }
            }

        }
        else
        {
            Console.WriteLine("Unable to authenticate with eNate exiting.");
            System.Environment.Exit(1);
        }
    }

    public static void ResetPassword(AppConfigure config, string userGUID)
    {
        string authToken = EnateCore.Authenticate();
        string NewPassword = GenerateRandomString(15);
        if (EnateCore.ChangeUserPassword(authToken, userGUID, NewPassword))
        {
            NewPassword = Utilities.Cryptography.Encrypt3DES(NewPassword);
            string strQuery = @"UPDATE dbo.CAU_Settings SET eNatePassword = '" + NewPassword + "', eNatePwdLastUpdate = getdate() WHERE eNateUserGUID = @userGUID";
            EYSql.ExecuteNonQuery(config.ConnectionString, CommandType.Text, strQuery, new SqlParameter("@userGUID", userGUID));

        }
        else
        {
            Log.Error("Unable to update password for account " + userGUID);
            System.Environment.Exit(-1);
        }



    }

    public static bool UserAssign(string UserGUID, string CaseGUID, string authToken)
    {
        UserAssignmentPacket packet = new UserAssignmentPacket();
        packet.AllowChangingExistingAssignment = true;
        packet.UserGUID = UserGUID;
        packet.PacketGUID = CaseGUID;

        return EnateCore.Packet_Assign(packet, authToken);

    }

    public static string GenerateRandomString(int length)
    {
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";

        var bytes = RandomNumberGenerator.GetBytes(length);
        var result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = validChars[bytes[i] % validChars.Length];
        }
        return new string(result);

    }

    public static bool eNateCaseCreation(CaseCreationLog cclog, long BatchID, CaseCreationLogModel casecreationLog, CheckInfo checkInfo, CaseCreation cc)
    {
        bool eNateSuccess = false;
        long casecreationID = cclog.CaseCreationStartLog(Convert.ToInt64(checkInfo.CheckId), checkInfo.SubmittedDateTime, BatchID);
        try
        {
            EnateOperations EnateOperations = new EnateOperations();

            casecreationLog.CheckCategory = cc.processNames;
            casecreationLog.NoOfEntities = checkInfo.EntitySPaceApg;

            cc.checkInfo = checkInfo;

            //Call eNate - Create Case and SetToDo
            eNateSuccess = CaseCreation(cc, out string errorMessage);
            Thread.Sleep(1000); //Sleeping for 2 second for testing
            if (eNateSuccess)
            {
                casecreationLog.IsErrored = false;
                cclog.UpdateCaseCreationLog(casecreationID, casecreationLog);
            }
            else
            {
                casecreationLog.IsErrored = true;
                cclog.UpdateCaseCreationLog(casecreationID, casecreationLog);
            }
            //  checkInfo.ErrorMessage = errorMessage;
            return eNateSuccess;
        }
        catch (Exception ex)
        {
            casecreationLog.IsErrored = true;
            //   checkInfo.ErrorMessage = $"{ex.Message};{ex.InnerException}";
            cclog.UpdateCaseCreationLog(casecreationID, casecreationLog);
            Console.WriteLine(ex.ToString());
            LoggerInfo.LogException(ex, "eNateCaseCreation - " + checkInfo.CheckId);
            return eNateSuccess;
        }
    }

    public static void AddeNateCaseToQueue(AppConfigure config, ConflictService conflictService)
    {
        Log.Information("Start getting more work from eNate.");
        Work_GetMoreWork _getMoreWork;
        string authToken = EnateCore.Authenticate();

        if (!authToken.IsNullOrEmpty())
        {
            authToken = EnateCore.Authenticate();
            _getMoreWork = EnateCore.Work_GetMoreWork(authToken);
            while (_getMoreWork != null)
            {
                Thread.Sleep(1000);
                var conflictCheckId = EnateCore.Action_Get(_getMoreWork.GUID, authToken);
                try
                {
                    _getMoreWork.ConflictCheckId = conflictCheckId.DataFields.CheckId;
                    
                }
                catch (Exception ex)
                {
                    Log.Error($"Error getting conflict check id for case {_getMoreWork.Reference}");
                    _getMoreWork.ConflictCheckId = "0";
                }
                Log.Information("Retrived Case: " + _getMoreWork.Reference + " with conflict check id: " + _getMoreWork.ConflictCheckId);
                conflictService.AddWorkToQueue(_getMoreWork, false);

                Thread.Sleep(1000); //rest for 2 seconds for testing
                _getMoreWork = EnateCore.Work_GetMoreWork(authToken);
            }
            Log.Information("No more new cases.");
        }
        else
        {
            Log.Information("Unable to authenticate with eNate, exiting");
            System.Environment.Exit(1);
        }


    }

    internal void CheckPreviousRun(ConflictService conflictService, AppConfigure config)
    {
        Work_GetMoreWork _result = new Work_GetMoreWork();
        try
        {
            
            string strQuery = @"
	SELECT TOP 1 [ID]
      ,[GUID]
      ,[Reference]
      ,[Title]
      ,[DueDate]
      ,[Status]
      ,[ProcessType]
      ,[CustomerName]
      ,[ContractName]
      ,[ServiceName]
      ,[ProcessName]
      ,[ConflictCheckId]
      ,[IsStarted]
      ,[IsOnHold]
      ,[StartTime]
      ,[EndTime]
      ,[DataEntryDate]  FROM dbo.CAU_WorkQueue WHERE ISNULL(IsStarted, 0) = 1 and ISNULL(IsOnHold, 0) = 1 AND StartTime is NOT null  AND EndTime is NULL and ServerName = '" + System.Environment.MachineName + @"' ORDER BY DueDate DESC
	
	";

            DataSet _ds = EYSql.ExecuteDataset(config.ConnectionString, CommandType.Text, strQuery);

            if (_ds.Tables[0].Rows.Count > 0)
            {
                _result = new Work_GetMoreWork();
                _result.GUID = _ds.Tables[0].Rows[0]["GUID"].ToString();
                _result.Reference = _ds.Tables[0].Rows[0]["Reference"].ToString();
                _result.Title = _ds.Tables[0].Rows[0]["Title"].ToString();
                _result.DueDate = Convert.ToDateTime(_ds.Tables[0].Rows[0]["DueDate"]);
                _result.Status = Convert.ToInt16(_ds.Tables[0].Rows[0]["Status"]);
                _result.ProcessType = Convert.ToInt16(_ds.Tables[0].Rows[0]["ProcessType"]);
                _result.CustomerName = _ds.Tables[0].Rows[0]["CustomerName"].ToString();
                _result.ContractName = _ds.Tables[0].Rows[0]["ContractName"].ToString();
                _result.ServiceName = _ds.Tables[0].Rows[0]["ServiceName"].ToString();
                _result.ProcessName = _ds.Tables[0].Rows[0]["ProcessName"].ToString();
                _result.ConflictCheckId = _ds.Tables[0].Rows[0]["ConflictCheckId"].ToString();

                Log.Information($"Case {_result.Reference}/check {_result.ConflictCheckId} did not complete in the previous run, marking as resolve unsuccessfully.");

                string authToken = EnateCore.Authenticate();
                if (authToken != null)
                {
                    EnateCore.Action_ResolveUnSuccessfully(_result, authToken, "AU unable to complete processing this case.");
                    Log.Information($"Update end time for {_result.ConflictCheckId}");
                    conflictService.UpdateWorkQueue(_result);
                } else
                {
                    Log.Information($"Unable to authenticate to eNate");
                }
            }
            
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, "CheckPreviousRun");
        }
    }
}
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore CS0168 // Variable is declared but never used
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0028 // Simplify collection initialization
#pragma warning restore IDE0059 // Unnecessary assignment of a value
