using ConflictAutomation.Extensions;
using ConflictAutomation.Models;
using ConflictAutomation.Services;
using ConflictAutomation.Services.Enate;
using ConflictAutomation.Services.FinScan;
using ConflictAutomation.Services.KeyGen;
using ConflictAutomation.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PACE;
using PACE.Api;
using PACE.Domain.Services;
using PACE.Services;
using Serilog;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;

namespace ConflictAutomation;

#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable CA2211 // Non-constant fields should not be visible
#pragma warning disable IDE0057 // Use range operator
#pragma warning disable IDE0063 // Use simple 'using' statement
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
#pragma warning disable CA1806 // Do not ignore method results
#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable CS0219 // Variable is assigned but its value is never used
#pragma warning disable CA1827 // Do not use Count() or LongCount() when Any() can be used

public class Program
{
    private static IConfigurationRoot configuration;
    public static KeyValuePairs KeyValuePairs { get; private set; }
    public static KeyGenFactory KeywordGeneratorFactory { get; private set; }
    public static KeyGenForEntities KeywordGeneratorForEntities { get; private set; }
    public static KeyGenForIndividuals KeywordGeneratorForIndividuals { get; private set; }
    public static FinScanRequestFactory FinScanRequestFactory { get; private set; }
    public static FinScanSearchAPIWithRetries FinScanSearch { get; private set; }
    public static ServiceProvider _serviceProvider;

    public static bool FinScanDebug => bool.Parse(configuration["FinScan:Debug"]);
    public static int FinScanMinRankScore => int.Parse(configuration["FinScan:MinRankScore"]);
    public static int FinScanMaxRankScore => int.Parse(configuration["FinScan:MaxRankScore"]);
    public static int FinScanMatchesThreshold => int.Parse(configuration["FinScan:MatchesThreshold"]);
    public static string FinScanMatchReportTemplateFilename => configuration["FinScan:MatchReportTemplateFilename"];
    public static bool UpdateCAUOutputInPACE => bool.Parse(configuration["PACE:UpdateCAUOutputInPACE"]);

    public static string eNateUserName = "";
    public static string eNatePassword = "";
    public static string eNateUserGUID = "";

    //Tejal - I hardcoded BOT GUI to save the record. Will request new HR record
    public static readonly PACE.Domain.Models.UserSessionViewModel US = new PACE.Domain.Models.UserSessionViewModel()
    { GUI = "2240212", FirstName = "HR ROBOT 174", LastName = "RMS_Conflct_5", Email = "P.XIACBPBPRMSCON0.5@xe04.ey.com" };


    public static void Main(string[] args)
    {
        Console.WriteLine($"START TIME: {DateTime.Now:hh\\:mm\\:ss}");

        Stopwatch stopWatch = new();
        stopWatch.Start();

        var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var directory = Path.GetDirectoryName(path);
        configuration = new ConfigurationBuilder()
           .SetBasePath(directory)
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .Build();

        AppConfigure config = new AppConfigure();
        var starter = Args(args);
        ShowStarterOptions(starter);
        try
        {
            LoadKeyValuePairs();
            LoadKeywordGenerators();
            LoadFinScanSearch();
            config.Environment = configuration["Environment"];
            string PACEConnection = GetPACEConnectionString(config);
            config.logpath = configuration["LogPath"];
            config.DEVLookupTable = configuration["DEVLookupTable"];
            config.onshoreContact = configuration["OnshoreContact"];
            config.SPLPath = configuration["SPLPath"];
            config.RegionalPath = configuration["RegionalNuances"];
            config.sourceFilePath = configuration["MasterTemplate"];
            config.destinationFilePath = configuration["DestinationFilePath"];
            config.ReworkFilePath = configuration["ReworkFilePath"];
            config.ConnectionString = PACEConnection;
            config.GISConnectionString = GetConnectionString("GISDatabase", configuration);
            config.InputFilePath = configuration["InputFilePath"];
            config.configurationRoot = configuration;
            config.IsTestMode = bool.Parse(configuration["IsTestMode"]);
            config.TestModeUserName = configuration["TestModeUserName"];
            config.TestModePassword = configuration["TestModePassword"];
            config.IsProtoType = bool.Parse(configuration["IsProtoType"]);
            config.IsManualRun = bool.Parse(configuration["IsManualRun"]);
            config.IsWorkQueue = bool.Parse(configuration["IsWorkQueue"]);
            config.IsGUPLaw = bool.Parse(configuration["IsGUPLaw"]);
            config.LastCheckFromBOTs = configuration["LastCheckFromBOTs"];
            config.FileSharePath = configuration["FileShare:FileSharePath"];
            config.SaveToFileShare = bool.Parse(configuration["FileShare:SaveToFileShare"]);
            config.MaximumDaysForFiles = configuration["MaximumDaysForFiles"];
            Log.Logger = new LoggerConfiguration()
                             .WriteTo.File(config.logpath, rollingInterval: RollingInterval.Month, retainedFileCountLimit: null)
                             .CreateLogger();
            _serviceProvider = new ServiceCollection()
                    .AddScoped<IConfiguration, ConfigurationRoot>()
                    .AddScoped<IConfigurationService, ConfigurationService>()
                    .AddScoped<ICountryConfigurationService, CountryConfigurationService>()
                    .AddScoped<IUserSessionService, UserSessionService>()
                    .AddScoped<INotificationService, NotificationService>()
                    .AddScoped<IServiceProvider, ServiceProvider>()
                    .AddScoped<ILogicTrace, LogicTrace>()
                    .AddScoped<IDraftAssessmentService, DraftAssessmentService>()
                    .AddScoped<IProcessQuestionnaireService, ProcessQuestionnaireService>()
                    .AddScoped<IProcessApprovalsService, ProcessApprovalsService>()
                    .AddScoped<IAssessmentService, AssessmentService>()
                    .AddScoped<IConflictCheckServices, ConflictCheckServices>()
                    .AddScoped<IEntitySearchService, EntitySearchService>()
                    .AddScoped<IStartAssessmentService, StartAssessmentService>()
                    .AddScoped<IAdditionalPartiesService, AdditionalPartiesService>()
                    .AddScoped<IFolderListService, FolderListService>()
                    .AddScoped<IGISQueueServices, GISQueueServices>()
                    .AddScoped<ILoggerFactory, LoggerFactory>()
                    .AddScoped<ArcherController>()
                    .AddScoped<IReportServices, ReportServices>()
                    .AddScoped<IConflictResearchService, ConflictResearchService>()
                    .AddScoped<IEngagementInfoService, EngagementInfoService>()
                    .AddScoped<ILogger<IConflictCheckServices>, Logger<ConflictCheckServices>>()
                    .AddScoped<IConfiguration>(_ => configuration)
                    .AddLogging()
                    .BuildServiceProvider();
            Log.Information("ConflictAutomation Process started..");
            if (starter.DeleteOldFiles)
            {
                DeleteOldFiles(config);
            }
            if (starter.CopyFiles)
            {
                CopyFiles(config);
            }
            if (starter.ProcessCheckWithouteNate)
            {
                ProcessCheckWithouteNate(config);
            }
            if (starter.CaseCreation)
            {
                GetSettingsFromDB(config, "case_creation");
                CaseCreation(config);
            }
            if (starter.ProcesseNateRequest)
            {
                Log.Information("Processing eNate Request on " + System.Environment.MachineName);
                GetSettingsFromDB(config, System.Environment.MachineName);
                ProcesseNateRequest(config);
            }
            if (starter.ReseteNatePassword)
            {
                GetSettingsFromDB(config, System.Environment.MachineName);
                EnateOperations.ResetPassword(config, eNateUserGUID);
                GetSettingsFromDB(config, System.Environment.MachineName);
                string authtoken = EnateCore.Authenticate();
                if (authtoken == "")
                {
                    Console.WriteLine("eNate credential test failed. Account unable to login to eNate.");
                    System.Environment.Exit(1);

                }
            }
            if (starter.AddeNateCaseToQueue)
            {

                GetSettingsFromDB(config, "work_queue");
                ConflictService conflictService = new ConflictService(config);
                EnateOperations.AddeNateCaseToQueue(config, conflictService);
            }
            if (starter.SaveToExcel)
            {
                SaveToExcelAndFileShare(config);
            }
            Console.WriteLine("ConflictAutomation Process ended.");
            Log.Information("ConflictAutomation Process ended.");

            stopWatch.Stop();
            Console.WriteLine($"\nELAPSED TIME: {stopWatch.Elapsed:hh\\:mm\\:ss}");

        }
        catch (Exception ex)
        {
            //TODO
            //LoggerInfo(ex.ToString());
            Console.WriteLine(ex.ToString());
            Log.Error(ex.ToString());
        }
    }



    private static void GetSettingsFromDB(AppConfigure _config, string ServerName)
    {
        string strQuery = "SELECT eNateUserName, eNatePassword, eNatePwdLastUpdate, eNateUserGUID FROM CAU_Settings WHERE ServerName = @Servername";
        DataSet _ds = EYSql.ExecuteDataset(_config.ConnectionString, CommandType.Text, strQuery, new SqlParameter("@Servername", ServerName));
        DateTime _lastPwdUpdate = DateTime.Now;
        if (_ds.Tables[0].Rows.Count > 0)
        {
            Enate_UserName = _ds.Tables[0].Rows[0]["eNateUserName"].ToString();
            Enate_Password = Utilities.Cryptography.Decrypt3DES(_ds.Tables[0].Rows[0]["eNatePassword"].ToString());
            Enate_UserGUID = _ds.Tables[0].Rows[0]["eNateUserGUID"].ToString();
            _lastPwdUpdate = Convert.ToDateTime(_ds.Tables[0].Rows[0]["eNatePwdLastUpdate"].ToString());

        }
        else
        {
            Console.WriteLine("Error retriving credential for eNate");
            System.Environment.Exit(1);
        }
    }

    private static void ShowStarterOptions(AUStarter starter)
    {
        if (!(starter.ProcessCheckWithouteNate || starter.CaseCreation || starter.ProcesseNateRequest || starter.DeleteOldFiles || starter.CopyFiles))
        {
            return;
        }

        Console.WriteLine("Starter options:");
        if (starter.ProcessCheckWithouteNate)
        {
            Console.WriteLine("  -processcheckwithoutenate");
        }
        if (starter.CaseCreation)
        {
            Console.WriteLine("  -casecreation");
        }
        if (starter.ProcesseNateRequest)
        {
            Console.WriteLine("  -processenaterequest");
        }
        if (starter.AddeNateCaseToQueue)
        {
            Console.WriteLine("  -addenatecasetoqueue");
        }
        if (starter.DeleteOldFiles)
        {
            Console.WriteLine("  -deleteoldfiles");
        }
        if (starter.CopyFiles)
        {
            Console.WriteLine("  -CopyFiles");
        }
    }


    public static void ProcessCheckWithouteNate(AppConfigure config)
    {
        Console.WriteLine("ProcessCheckWithouteNate started");
        ConflictService conflictService = new ConflictService(config);
        conflictService.UpdateFileshare();
        //ToDo. Load CheckerQueue list        
        var summary = new CheckerQueue();
        var gisList = new List<SearchEntitiesResponseItemViewModel>();
        ConflictCheck conflictCheck = new ConflictCheck();
        ProcessLog processlog = new ProcessLog(config);
        var ProcessedLog = new ProcessedChecks();
        long ProcessLogID = 0;
        List<ProcessedChecks> failedCheks = new List<ProcessedChecks>();
        if (config.IsProtoType)
        {

            foreach (string file in Directory.EnumerateFiles(Path.Combine(config.InputFilePath, config.Environment), "*.xlsx"))
            {
                DataTable tbl = ConflictService.GetDataTableFromExcel(file);
                StringBuilder sbIDs = new StringBuilder();
                for (int i = 0; i < tbl.Rows.Count; i++)
                {
                    long ConflictCheckID = 0;
                    try
                    {
                        string Rework = "No";
                        bool MultiEntity = false;
                        ConflictCheckID = Convert.ToInt64(tbl.Rows[i][0]);
                        DataColumnCollection columns = tbl.Columns;
                        if (columns.Contains("Rework"))
                        {
                            if (!string.IsNullOrEmpty(tbl.Rows[i][1].ToString()))
                                Rework = Convert.ToString(tbl.Rows[i][1]);
                        }

                        Stopwatch CCstopWatch = new();
                        CCstopWatch.Start();
                        Console.WriteLine($"\nConflictCheckID Process started: " + ConflictCheckID);
                        Console.WriteLine($"\nCHECK PROCESSING START TIME: {DateTime.Now:hh\\:mm\\:ss}");

                        ProcessedLog.ProcessStart = DateTime.Now.TimestampWithTimezoneFromLocal("India Standard Time", "", "yyyy-MM-ddTHH:mm:ss");
                        ProcessLogID = processlog.StartLog(ConflictCheckID, ProcessedLog.ProcessStart, config.Environment);

                        // ***** Carlos 2024-03-04
                        if (FinScanDebug)
                        {
                            goto PROCESS_CHECK;
                        }
                        // ***** Carlos 2024-03-04


                        Stopwatch stopWatch = new();
                        stopWatch.Start();

                        conflictCheck = _serviceProvider.GetService<IConflictCheckServices>().GetConflictCheckByID(ConflictCheckID);

                        stopWatch.Stop();
                        Console.WriteLine($"\nGetService<IConflictCheckServices> took: {stopWatch.Elapsed:hh\\:mm\\:ss}");


                        //Set the substatus
                        ConflictCheckSubStatus CSS = new ConflictCheckSubStatus();
                        DataTable dtSubStatus = EYSql.ExecuteDataset(config.ConnectionString, CommandType.Text,
                                   @"Select * from WF_ConflictCheckSubStatus WHERE ShortVersion ='Research in progress' ").Tables[0];
                        foreach (DataRow drSubStatus in dtSubStatus.Rows)
                        {
                            DBMarshaller.Load(dtSubStatus.Columns, drSubStatus, CSS);
                        }

                        var logger = _serviceProvider.GetService<ILogger<ConflictCheckServices>>();
                        conflictCheck = _serviceProvider.GetService<IConflictCheckServices>().AssignOwnership(ConflictCheckID, US.GUI.ToString(), true, logger);

                        conflictCheck.ConflictCheckSubStatus = _serviceProvider.GetService<IConflictCheckServices>().SaveAdminSubStatus(CSS, US.GUI.ToString());
                        conflictCheck.SubStatusID = Convert.ToInt16(conflictCheck.ConflictCheckSubStatus.SubStatusID);

                        _serviceProvider.GetService<IConflictCheckServices>().SaveConflictCheck(conflictCheck, US.GUI.ToString());

                    PROCESS_CHECK:

                        var processedcheck = conflictService.ProcessCheck(ConflictCheckID, ProcessLogID, ProcessedLog.ProcessStart, conflictCheck, _serviceProvider, Rework == "Yes", MultiEntity);
                        Log.Information("Check Processed-" + ConflictCheckID);
                        Console.WriteLine("\nConflictCheckID Processed: " + ConflictCheckID);
                        if (processedcheck.IsErrored)
                        {
                            processedcheck.CheckId = ConflictCheckID.ToString();
                            processedcheck.conflictCheck = conflictCheck;
                            processedcheck.ReWork = Rework;
                            processedcheck.MultiEntity = MultiEntity;
                            failedCheks.Add(processedcheck);
                        }
                        CCstopWatch.Stop();
                        Console.WriteLine($"\nCHECK PROCESSING ELAPSED TIME: {CCstopWatch.Elapsed:hh\\:mm\\:ss}");

                        //add logs and other methods                       

                        // ***** Carlos 2024-03-04
                        if (FinScanDebug)
                        {
                            break;
                        }

                        if (!FinScanDebug)
                        {
                            File.Delete(file);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerInfo.LogException(ex, $"{ConflictCheckID};{"ProcessCheckWithouteNate"}");
                        continue;
                    }
                }
            }

            //3 attempts before failure
            int maxRetryCount = 3;
            for (int retryCount = 1; retryCount <= maxRetryCount; retryCount++)
            {
                foreach (var item in failedCheks.Where(i => i.IsErrored))
                {
                    try
                    {
                        LoggerInfo.LogThreeTrials(item, retryCount, "Processing Checks Failure", config.Environment);
                        Console.WriteLine($"\nConflictCheckID Process retry {retryCount} started: " + item.CheckId);
                        Log.Information($"\nConflictCheckID Process retry {retryCount} started: " + item.CheckId);
                        Console.WriteLine($"\nCHECK PROCESSING START TIME: {DateTime.Now:hh\\:mm\\:ss}");
                        ProcessedLog.ProcessStart = DateTime.Now.TimestampWithTimezoneFromLocal("India Standard Time", "", "yyyy-MM-ddTHH:mm:ss");
                        ProcessLogID = processlog.StartLog(Convert.ToInt64(item.CheckId), ProcessedLog.ProcessStart, config.Environment);
                        var check = conflictService.ProcessCheck(Convert.ToInt64(item.CheckId), ProcessLogID, ProcessedLog.ProcessStart, conflictCheck, _serviceProvider, item.ReWork == "Yes", item.MultiEntity);
                        item.IsErrored = check.IsErrored;
                        Console.WriteLine("\nConflictCheckID Processed: " + item.CheckId);
                        Log.Information("\nConflictCheckID Processed: " + item.CheckId);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }
    }

    public static void ProcesseNateRequest(AppConfigure config)
    {
        ConflictService conflictService = new ConflictService(config);
        try
        {
            //if (config.IsWorkQueue)
            //{
            EnateOperations eNate = new EnateOperations();
            eNate.CheckPreviousRun(conflictService, config);
            eNate.ProcessENateRequest(conflictService, _serviceProvider, config, US);
            //}
        }
        catch (Exception ex)
        {
            Console.WriteLine("Processing eNate Case: " + ex.Message);
        }
    }

    public static void CaseCreation(AppConfigure config)
    {
        CaseCreationLog cclog = new CaseCreationLog(config);
        var casecreationLog = new CaseCreationLogModel();
        var filters = new List<FilterBase>();
        try
        {
            //get the batchID
            long a_nBatchID = 1;

            var sqlbatch = $"SELECT MAX(BatchID) + 1 FROM CAU_CaseCreationLog";

            DataTable dtBatch = EYSql.ExecuteDataset(Program.PACEConnectionString, CommandType.Text, sqlbatch).Tables[0];
            foreach (DataRow row in dtBatch.Rows)
            {
                a_nBatchID = Convert.ToInt64(row[0]);
            }

            var skipCountries = ConflictAULookUp.GetSkipCountries(config);

            DateTime LastRunDate = DateTime.UtcNow;
            List<long> ExistingCheckIDs = new List<long>();
            List<long> ProcessingCheckIDs = new List<long>();
            List<long> ResubmittedCheckIDs = new List<long>();

            string caseSQL = @"DECLARE @CurrentYear INT
                    SELECT @CurrentYear = YEAR(GETDATE())
                    
                    SELECT MAX(LastRun) as LastRun FROM CAU_CaseDataLog WHERE IsErrored = '0' --0

                    SELECT TOP 1000 ID, ConflictCheckID FROM CAU_WorkQueue  ORDER BY 1 DESC  --1

                    SELECT DISTINCT C.ConflictCheckID, ISNULL(C.[ConflictLastSubmissionDateTime], C.[ConflictInitiatedDateTime]) [ConflictInitiatedDateTime] FROM WF_ConflictChecks C with (nolock) 
                    INNER JOIN CAU_CaseCreationLog CC  with (nolock) ON C.ConflictCheckID = CC.ConflictCheckID 
                        and   YEAR(ISNULL(C.[ConflictLastSubmissionDateTime], C.[ConflictInitiatedDateTime])) > @CurrentYear - 3
                        and  ISNULL(C.[ConflictLastSubmissionDateTime], C.[ConflictInitiatedDateTime]) > CC.ConflictInitiatedDate
                    WHERE C.Resubmitted = 1 and C.StageOfConflict NOT IN ('Archived', 'Cancelled', 'Complete') --2
                    ";

            DataSet dsLog = EYSql.ExecuteDataset(Program.PACEConnectionString, CommandType.Text, caseSQL, new SqlParameter("@a_nBAtchID", a_nBatchID - 1));

            if (dsLog.Tables[0].Rows.Count > 0)
            {
                LastRunDate = Convert.ToDateTime(dsLog.Tables[0].Rows[0]["LastRun"]);
            }
            foreach (DataRow dr in dsLog.Tables[1].Rows)
            {
                ExistingCheckIDs.Add(dr.GetLong("ConflictCheckID"));
            }
            foreach (DataRow dr in dsLog.Tables[2].Rows)
            {
                ResubmittedCheckIDs.Add(dr.GetLong("ConflictCheckID"));
            }



            LastRunDate = LastRunDate.AddMinutes(-15); //setting it to look back 60 minutes to make sure there are no check missed from the time checkqueue is called to end of processing.
            Log.Information($"LastRunDate: {LastRunDate}");

            string processingSQL = @"SELECT DISTINCT ConflictCheckID FROM CAU_ManualRun ";
            DataSet dsProcessing = EYSql.ExecuteDataset(Program.PACEConnectionString, CommandType.Text, processingSQL);

            foreach (DataRow dr in dsProcessing.Tables[0].Rows)
            {
                ProcessingCheckIDs.Add(dr.GetLong("ConflictCheckID"));
            }

            //            ProcessingCheckIDs.Add(1414352);
            //            ProcessingCheckIDs.Add(1414679);
            //            ProcessingCheckIDs.Add(1466551);
            //ProcessingCheckIDs.Add(1458705);
            //ProcessingCheckIDs.Add(1457767);

            //ExistingCheckIDs.Add(1460006);

            //DateTime StartDateTime = DateTimeOffset.Parse("2023-10-02").DateTime;
            //DateTime EndDateTime = DateTimeOffset.Parse("2023-10-03").DateTime;

            long initialCheckID = 1420810; // 1416396;
            long lastCheckID = 1457227;// 1416458;
            //FOR DEBUG ONLY
            //var checkerQueuelisttemp = _serviceProvider.GetService<IFolderListService>()
            //                        .GetCheckerQueue(filters)
            //                        .Where(queueItem => !skipCountries.Any(skipCountry => skipCountry.CountryCode.ToLower() == queueItem.CountryCode.ToLower()
            //                                            && String.IsNullOrWhiteSpace(skipCountry.SSLName))
            //                                            && queueItem.ConflictCheckID >= initialCheckID 
            //                                            && queueItem.ConflictCheckID <= lastCheckID
            //                                            && !ExistingCheckIDs.Contains(queueItem.ConflictCheckID)
            //                                     //       && queueItem.ConflictInitiatedDateTime >= LastRunDate
            //                                            && queueItem.ConflictInitiatedDateTime < DateTime.UtcNow
            //                                           )
            //                        .ToList();


            //FOR UAT ONLY
            //var checkerQueuelisttemp = _serviceProvider.GetService<IFolderListService>()
            //                       .GetCheckerQueue(filters)
            //                       .Where(queueItem => !skipCountries.Any(skipCountry => skipCountry.CountryCode.ToLower() == queueItem.CountryCode.ToLower()
            //                                           && String.IsNullOrWhiteSpace(skipCountry.SSLName))
            //                                           && ProcessingCheckIDs.Contains(queueItem.ConflictCheckID)
            //                                           && !ExistingCheckIDs.Contains(queueItem.ConflictCheckID)
            //                                          )
            //                       .ToList();

            //var t = checkerQueuelisttemp.Where(x=>x.ConflictCheckID == 1457207).ToJson();

            List<PACE.Domain.Models.CheckerQueueViewModel> caseCreationModel = new List<PACE.Domain.Models.CheckerQueueViewModel>();
            List<PACE.Domain.Models.CheckerQueueViewModel> checkerQueuelisttemp = new List<PACE.Domain.Models.CheckerQueueViewModel>();
            List<PACE.Domain.Models.CheckerQueueViewModel> checkerQueuelist = new List<PACE.Domain.Models.CheckerQueueViewModel>();

            if (config.IsManualRun)
            {
                checkerQueuelisttemp = _serviceProvider.GetService<IFolderListService>()
                                   .GetCheckerQueue(filters)
                                   .Where(queueItem => !skipCountries.Any(skipCountry => skipCountry.CountryCode.ToLower() == queueItem.CountryCode.ToLower()
                                                       && String.IsNullOrWhiteSpace(skipCountry.SSLName))
                                                       && ProcessingCheckIDs.Contains(queueItem.ConflictCheckID)                                                      
                                                      )
                                   .ToList();
            }
            else
            {              
                 checkerQueuelisttemp = _serviceProvider.GetService<IFolderListService>()
                                        .GetCheckerQueue(filters)
                                        .Where(queueItem => !skipCountries.Any(skipCountry => skipCountry.CountryCode.ToLower() == queueItem.CountryCode.ToLower()
                                                            && String.IsNullOrWhiteSpace(skipCountry.SSLName))
                                                            && queueItem.ConflictInitiatedDateTime >= LastRunDate
                                                            //&& queueItem.ConflictCheckID > Convert.ToInt64(config.LastCheckFromBOTs) 
                                                            // && (!ExistingCheckIDs.Contains(queueItem.ConflictCheckID) || (queueItem.ResubmittedCheck == "Yes" && ResubmittedCheckIDs.Contains(queueItem.ConflictCheckID)))
                                                            && queueItem.ConflictInitiatedDateTime < DateTime.UtcNow)
                                        .ToList();
            }

            //NLD - Law
            checkerQueuelist = checkerQueuelisttemp.Where(queueItem => !skipCountries.Any(skipCountry => skipCountry.SSLName.ToLower() == queueItem.SubServiceLine.ToLower()
                                                        && skipCountry.CountryCode == queueItem.CountryCode && skipCountry.CountryCode == "NLD")).ToList();          

            if (config.IsManualRun)
                caseCreationModel.AddRange(checkerQueuelist);
            else
            {
                //filter down for resbumitted
                var resubmitted = checkerQueuelist.Where(queueItem => (queueItem.ResubmittedCheck == "Yes" && ResubmittedCheckIDs.Contains(queueItem.ConflictCheckID)));
                caseCreationModel.AddRange(resubmitted);

                var removeExisting = checkerQueuelist.Where(queueItem => !ExistingCheckIDs.Contains(queueItem.ConflictCheckID));
                caseCreationModel.AddRange(removeExisting);
            }

            List<AreaRegion> area = new List<AreaRegion>();
            string SQL = @"SELECT DISTINCT CountryCode, Area FROM WF_AreaRegion WHERE Area <> 'Not Availalbe' ";
            DataTable dt = EYSql.ExecuteDataset(config.ConnectionString, CommandType.Text, SQL).Tables[0];

            foreach (DataRow dr in dt.Rows)
            {
                string areaModified = dr.GetString("Area");
                if (areaModified == "Asia-Pac" || areaModified == "Japan")
                    areaModified = "APAC";
                area.Add(new AreaRegion()
                {
                    Area = areaModified,
                    CountryCode = dr.GetString("CountryCode")
                });
            }

            CheckInfo checkInfo = new CheckInfo();
            List<CheckInfo> failedCheks = new List<CheckInfo>();
            CaseCreation cc = new CaseCreation();
            foreach (var cq in caseCreationModel.Distinct().OrderBy(i => i.ConflictInitiatedDateTime))
            {
                // one check for local                
                //  if (cq.ConflictCheckID == 1456797)
                //                if (cq.ConflictCheckID == 1422812 ||
                //cq.ConflictCheckID == 1422814 ||
                //cq.ConflictCheckID == 1422815 ||
                //cq.ConflictCheckID == 1422816 ||
                //cq.ConflictCheckID == 1422817 ||
                //cq.ConflictCheckID == 1422819 ||
                //cq.ConflictCheckID == 1422820 ||
                //cq.ConflictCheckID == 1422823 ||
                //cq.ConflictCheckID == 1422831 ||
                //cq.ConflictCheckID == 1422856 ||
                //cq.ConflictCheckID == 1422858)
                {

                    var areaResult = area.Where(x => x.CountryCode.Equals(cq.CountryCode));
                    string areaName = "";
                    if (areaResult.Count() > 0)
                    {
                        areaName = area.Where(x => x.CountryCode.Equals(cq.CountryCode)).FirstOrDefault().Area;
                        if (string.IsNullOrEmpty(areaName))
                            areaName = "Not Availalbe";
                    }
                    else
                    {
                        areaName = "Not Availalbe";
                    }
                    checkInfo.CheckId = cq.ConflictCheckID.ToString();
                    checkInfo.EntityName = cq.ClientName;
                    checkInfo.CheckType = cq.ConflictCheckType;
                    checkInfo.Confidentiality = cq.Confidential;
                    checkInfo.Country = casecreationLog.Country = cq.Country;
                    checkInfo.EntitySPaceApg = cq.NoOfOtherParties;
                    checkInfo.G360 = cq.AccountType.Contains("G360") ? "Yes" : "No";
                    checkInfo.PaceRegion = cq.Region;
                    checkInfo.Region = casecreationLog.Region = areaName;
                    checkInfo.ResubmittedCheck = cq.ResubmittedCheck;
                    checkInfo.ServiceLine = casecreationLog.SLName = string.IsNullOrEmpty(cq.ServiceLine) ? "No SL" : cq.ServiceLine;
                    checkInfo.SubServiceLine = cq.SubServiceLine.Length > 399 ? cq.SubServiceLine.Substring(0, 400) : cq.SubServiceLine;
                    checkInfo.SortService = cq.PrimaryServiceDisplayName.ToString();
                    checkInfo.UrgentCheck = cq.UrgentCheck;
                    checkInfo.SubmittedDateTime = cq.ConflictInitiatedDateTime;
                    checkInfo.BasedOnCompletedPursuitCheck = cq.CheckbasedCompletedPursuit;
                    checkInfo.ServiceCode = cq.AdditionalServiceDisplayName.Length > 399 ? cq.AdditionalServiceDisplayName.Substring(0, 400) : cq.AdditionalServiceDisplayName;
                    checkInfo.PrimarySort = cq.PrimarySORTServiceName.Length > 399 ? cq.PrimarySORTServiceName.Substring(0, 400) : cq.PrimarySORTServiceName;
                    checkInfo.AdditionalSort = cq.AdditionalSORTServiceName.Length > 399 ? cq.AdditionalSORTServiceName.Substring(0, 400) : cq.AdditionalSORTServiceName;
                    checkInfo.ResubmissionReasonS = cq.ResubmissionReason.Length > 399 ? cq.ResubmissionReason.Substring(0, 400) : cq.ResubmissionReason;
                    checkInfo.ResubmissionComments = cq.ResubmissionReasonComment.Length > 399 ? cq.ResubmissionReasonComment.Substring(0, 400) : cq.ResubmissionReasonComment;

                    if (checkInfo.EntitySPaceApg > 25)
                    {
                        cc.processNames = ProcessNames.MultiEntityCheck;
                        checkInfo.CheckProcess = CheckProcess.MultiEntityCheck;
                    }
                    else if (checkInfo.G360 == "Yes")
                    {
                        cc.processNames = ProcessNames.G360NormalCheck;
                        checkInfo.CheckProcess = CheckProcess.G360NormalCheck;
                    }
                    else if (checkInfo.ResubmittedCheck == "Yes")
                    {
                        cc.processNames = ProcessNames.ResubmittedNormalCheck;
                        checkInfo.CheckProcess = CheckProcess.ResubmittedNormalCheck;
                    }
                    else if (checkInfo.EntitySPaceApg <= 25)
                    {
                        cc.processNames = ProcessNames.NormalCheck;
                        checkInfo.CheckProcess = CheckProcess.NormalCheck;
                    }
                    else
                        cc.processNames = ProcessNames.NormalCheckManual;

                    Log.Information($"creating case for check {checkInfo.CheckId}, CheckInitiatedDatetime: {cq.ConflictInitiatedDateTime}, LastRun: {LastRunDate}");
                    //   a_nBatchID = 4;
                    bool eNateSuccess = EnateOperations.eNateCaseCreation(cclog, a_nBatchID, casecreationLog, checkInfo, cc);
                    if (!eNateSuccess)
                    {
                        if (!failedCheks.Contains(checkInfo))
                            failedCheks.Add(checkInfo);
                    }

                }
            }
            int maxRetryCount = 3;
            for (int retryCount = 1; retryCount <= maxRetryCount; retryCount++)
            {
                foreach (var item in failedCheks.Distinct())
                {
                    try
                    {
                        LoggerInfo.LogThreeTrials(item, retryCount, "Case Creation Error", config.Environment);
                        EnateOperations.eNateCaseCreation(cclog, a_nBatchID, casecreationLog, checkInfo, cc);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            string InsertSql = @"INSERT INTO CAU_CaseDataLog(IsErrored) SELECT 0";
            DataSet ds = EYSql.ExecuteDataset(config.ConnectionString, CommandType.Text, InsertSql);
        }
        catch (Exception ex)
        {
            string InsertSql = @"INSERT INTO CAU_CaseDataLog(IsErrored) SELECT 1";
            DataSet ds = EYSql.ExecuteDataset(config.ConnectionString, CommandType.Text, InsertSql);
            LoggerInfo.LogException(ex);
        }
    }
    public static void DeleteOldFiles(AppConfigure config)
    {
        int maximumDaysForFiles = 0;
        List<string> excludeList = new List<string>();
        try
        {
            if (!int.TryParse(config.MaximumDaysForFiles, out maximumDaysForFiles))
                Console.WriteLine("Invalid value for MaximumDaysForFiles");
            excludeList.Add(Path.GetFileName(config.onshoreContact));
            excludeList.Add(Path.GetFileName(config.RegionalPath));
            excludeList.Add(Path.GetFileName(config.SPLPath));
            ConflictService.DeleteOldFiles(config.destinationFilePath, maximumDaysForFiles, excludeList);
            if (Directory.Exists(config.FileSharePath))
            {
                ConflictService.DeleteOldFiles(config.FileSharePath, maximumDaysForFiles, excludeList);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Processing DeleteOldFiles: {ex.Message}");
        }
    }
    public static void CopyFiles(AppConfigure config)
    {
        try
        {
            var SPLLocalPath = Path.Combine(config.destinationFilePath, Path.GetFileName(config.SPLPath));
            var RegionalLocalPath = Path.Combine(config.destinationFilePath, Path.GetFileName(config.RegionalPath));
            var onshoreContactLocalPath = Path.Combine(config.destinationFilePath, Path.GetFileName(config.onshoreContact));
            File.Copy(config.SPLPath, SPLLocalPath, true);
            File.Copy(config.RegionalPath, RegionalLocalPath, true);
            File.Copy(config.onshoreContact, onshoreContactLocalPath, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Process CopyFiles: {ex.Message}");
        }
    }
    public static void SaveToExcelAndFileShare(AppConfigure config)
    {
        try
        {
            DateTime now = DateTime.Now;
            string timestamp = now.ToString("MM_dd_yyyy");
            var excelPath = Path.GetDirectoryName(config.onshoreContact);
            var excelfile = $"EXCEL_{timestamp}.xlsx";
            excelfile = Path.Combine(excelPath, "ConflictChecks", excelfile);
            Directory.CreateDirectory(Path.Combine(excelPath, "ConflictChecks"));
            var sqlQuery = string.Format("EXEC CAU_Summary '{0}'", 1498720); // Will replace once new sproc is available
            var dataSet = EYSql.ExecuteDataset(config.ConnectionString, CommandType.Text, sqlQuery);
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                ExcelOperations.SaveDataTableToExcel(dataSet, excelfile);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SaveToExcelAndFileShare: {ex.Message}");
        }
    }
    private static AUStarter Args(string[] args)
    {
        AUStarter options = new AUStarter();
        foreach (var cmdArg in args)
        {
            if (cmdArg.Trim().ToLowerInvariant() == "-processcheckwithoutenate")
                options.ProcessCheckWithouteNate = true;
            if (cmdArg.Trim().ToLowerInvariant() == "-casecreation")
                options.CaseCreation = true;
            if (cmdArg.Trim().ToLowerInvariant() == "-processenaterequest")
                options.ProcesseNateRequest = true;
            if (cmdArg.Trim().ToLowerInvariant() == "-resetenatepassword")
                options.ReseteNatePassword = true;
            if (cmdArg.Trim().ToLowerInvariant() == "-addenatecasetoqueue")
                options.AddeNateCaseToQueue = true;
            if (cmdArg.Trim().ToLowerInvariant() == "-deleteoldfiles")
                options.DeleteOldFiles = true;
            if (cmdArg.Trim().ToLowerInvariant() == "-copyfiles")
                options.CopyFiles = true;
            if (cmdArg.Trim().ToLowerInvariant() == "-savetoexcel")
                options.SaveToExcel = true;
        }
        return options;
    }
    public static string ExcelExport(string sPath, string sdestinationPath, DataTable dt, string FileName, bool printOptionalHeaderText = true, bool printHeader = true)
    {
        FileInfo templateFile = new FileInfo(sPath);
        string retPath;
        try
        {
            using (FileStream FS = templateFile.OpenRead())
            {
                using (OfficeOpenXml.ExcelPackage EP = new OfficeOpenXml.ExcelPackage(FS))
                {
                    EP.Workbook.Properties.Title = FileName;
                    OfficeOpenXml.ExcelWorksheet ws = EP.Workbook.Worksheets[0];
                    ws.Name = Path.GetFileNameWithoutExtension(FileName).Replace("_Template", "");
                    string sStartCells = "A2:Z2";
                    if (!printHeader)
                        sStartCells = "A1:Z1";

                    ws.Cells[sStartCells].LoadFromDataTable(dt, true);

                    ws.Cells.AutoFitColumns();

                    byte[] arrExcelContents = null;
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        EP.SaveAs(ms);
                        ms.Position = 0;
                        arrExcelContents = ms.ToArray();
                    }
                    DirectoryInfo downloadedMessageInfo = new DirectoryInfo(Path.Combine(sdestinationPath, "CachedFiles"));
                    string path = Path.GetRandomFileName();

                    sPath = downloadedMessageInfo + @"\" + FileName + @"\" + path.Substring(0, 8);

                    Directory.CreateDirectory(sPath);

                    sPath = sPath + @"\" + FileName + ".xlsx";

                    using (FileStream fs = new FileStream(sPath,
                            FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(arrExcelContents, 0, arrExcelContents.Length);
                    }

                    retPath = FileName + @"\\" + path.Substring(0, 8) + @"\\" + FileName + ".xlsx";
                    sPath = sPath.Replace('\\', '/');

                }
            }

            return retPath;

        }

        catch (Exception ex)
        {
            return ex.ToString();
        }
    }


    #region Read Config Keys
    public static string PACEConnectionString
    {
        get
        {
            string sCS = configuration.GetConnectionString("PACEDatabase").ToString();
            if (sCS.Contains("initial catalog") || sCS.Contains("database"))
                return configuration.GetConnectionString("PACEDatabase");
            else
                return Utilities.Cryptography.Decrypt3DES(configuration.GetConnectionString("PACEDatabase").ToString());
        }
    }

    public static string GISOrigin
    {
        get
        {
            return $"{configuration["GISURL:GISOrigin"]}";
        }
    }
    public static string GISEntitySearchAPIRelativeURL
    {
        get
        {
            return $"{configuration["GISURL:GISEntitySearchAPIRelativeURL"]}";
        }
    }

    public static string FinScanSearchAPI_URL => $"{configuration["FinScanSearchAPI:URL"]}";
    public static string FinScanSearchAPI_OrganizationName => $"{configuration["FinScanSearchAPI:OrganizationName"]}";
    public static string FinScanSearchAPI_UserName
    {
        get
        {
            return Utilities.Cryptography.Decrypt3DES(configuration["FinScanSearchAPI:UserName"]);
        }
    }
    public static string FinScanSearchAPI_Password
    {
        get
        {
            return Utilities.Cryptography.Decrypt3DES(configuration["FinScanSearchAPI:Password"]);
        }
    }
    public static string FinScanSearchAPI_ApplicationId => $"{configuration["FinScanSearchAPI:ApplicationId"]}";


    public static string Enate_APIURL
    {
        get
        {
            return $"{configuration["Enate:APIURL"]}";
        }
    }

    public static string Enate_UserName
    {
        get
        {
            //return Utilities.Cryptography.Decrypt3DES(configuration["Enate:UserName"]);
            return eNateUserName;
        }
        set { eNateUserName = value; }
    }

    public static string Enate_Password
    {
        get
        {
            //return Utilities.Cryptography.Decrypt3DES(configuration["Enate:UserName"]);
            return eNatePassword;
        }
        set { eNatePassword = value; }
    }
    public static string Enate_UserGUID
    {
        get
        {
            //return Utilities.Cryptography.Decrypt3DES(configuration["Enate:UserName"]);
            return eNateUserGUID;
        }
        set { eNateUserGUID = value; }
    }

    public static string Enate_Authentication_Login
    {
        get
        {
            return $"{configuration["Enate:Authentication_Login"]}";
        }
    }
    public static string Enate_Authentication_SetLiveMode
    {
        get
        {
            return $"{configuration["Enate:Authentication_SetLiveMode"]}";
        }
    }
    public static string Enate_Authentication_Logout
    {
        get
        {
            return $"{configuration["Enate:Authentication_Logout"]}";
        }
    }
    public static string Enate_Case_Create
    {
        get
        {
            return $"{configuration["Enate:Case_Create"]}";
        }
    }
    public static string Enate_ProcessContext_GetCompanies
    {
        get
        {
            return $"{configuration["Enate:ProcessContext_GetCompanies"]}";
        }
    }
    public static string Enate_ProcessContext_GetContracts
    {
        get
        {
            return $"{configuration["Enate:ProcessContext_GetContracts"]}";
        }
    }
    public static string Enate_ProcessContext_GetServices
    {
        get
        {
            return $"{configuration["Enate:ProcessContext_GetServices"]}";
        }
    }
    public static string Enate_ProcessContext_GetProcesses
    {
        get
        {
            return $"{configuration["Enate:ProcessContext_GetProcesses"]}";
        }
    }
    public static string Enate_CaseAttribute_GetVersions
    {
        get
        {
            return $"{configuration["Enate:CaseAttribute_GetVersions"]}";
        }
    }
    public static string Enate_CaseAttribute_Get
    {
        get
        {
            return $"{configuration["Enate:CaseAttribute_Get"]}";
        }
    }
    public static string Enate_Case_Get
    {
        get
        {
            return $"{configuration["Enate:Case_Get"]}";
        }
    }
    public static string Enate_Case_Update
    {
        get
        {
            return $"{configuration["Enate:Case_Update"]}";
        }
    }
    public static string Enate_Work_GetMoreWork
    {
        get
        {
            return $"{configuration["Enate:Work_GetMoreWork"]}";
        }
    }

    public static string Enate_Packet_GetContexts
    {
        get
        {
            return $"{configuration["Enate:Packet_GetContexts"]}";
        }
    }

    public static string Enate_Packet_Assign
    {
        get
        {
            return $"{configuration["Enate:Packet_Assign"]}";
        }
    }
    public static string Enate_Action_SetInProgress
    {
        get
        {
            return $"{configuration["Enate:Action_SetInProgress"]}";
        }
    }

    public static string Enate_Action_ResolveSuccessfully
    {
        get
        {
            return $"{configuration["Enate:Action_ResolveSuccessfully"]}";
        }
    }

    public static object Enate_Action_Get
    {
        get
        {
            return $"{configuration["Enate:Action_Get"]}";
        }
    }
    public static string Enate_Case_SetToDo
    {
        get
        {
            return $"{configuration["Enate:Case_SetToDo"]}";
        }
    }

    public static string Enate_Action_ResolveUnSuccessfully
    {
        get
        {
            return $"{configuration["Enate:Action_ResolveUnSuccessfully"]}";
        }
    }

    public static string ReseteNatePassword
    {
        get
        {
            return $"{configuration["Enate:UserManagement_ChangeUserPassword"]}";
        }
    }
    #endregion


    private static void LoadKeyValuePairs()
    {
        KeyValuePairs = new KeyValuePairs(PACEConnectionString, useCache: true);
    }


    private static void LoadKeywordGenerators()
    {
        KeywordGeneratorFactory = new KeyGenFactory(PACEConnectionString);
        KeywordGeneratorForEntities = KeywordGeneratorFactory.KeyGenForEntities;
        KeywordGeneratorForIndividuals = KeywordGeneratorFactory.KeyGenForIndividuals;
    }


    private static void LoadFinScanSearch()
    {
        FinScanRequestFactory = new FinScanRequestFactory(
                                    FinScanSearchAPI_OrganizationName,
                                    FinScanSearchAPI_UserName,
                                    FinScanSearchAPI_Password,
                                    FinScanSearchAPI_ApplicationId);

        FinScanSearch = new FinScanSearchAPIWithRetries(FinScanSearchAPI_URL, LoggerInfo.DefaultLogAction);
    }

    public static long SaveFileToDB(string filePath, string fileType, string entityId = "", int entityTypeId = 12)
    {
        long ID = 0;

        try
        {
            byte[] fileData = File.ReadAllBytes(filePath);


            object ret = EYSql.ExecuteScalar(PACE.Program.PACEConnectionString, CommandType.Text,
                  @"Insert Into WF_Attachments (FileName, Type, Content, UploadedDate, EntityID, EntityTypeID) 
                        Values (@a_sFileName, @a_sType, @a_sContent, getutcdate(), @a_sEntityId, @a_sEntityType);
                        Select @@Identity",
                  new SqlParameter("@a_sFileName", Path.GetFileName(filePath)),
                  new SqlParameter("@a_sType", fileType),
                  new SqlParameter("@a_sContent", fileData),
                  new SqlParameter("@a_sEntityType", entityTypeId.ToString()),
                  new SqlParameter("@a_sEntityId", entityId));

            long.TryParse(ret.ToString(), out ID);
        }
        catch (Exception ex)
        {
            UserSessionService.LogException(ex);
        }

        return ID;
    }
    //For modifying attachments in pace
    public static void UpdateFileToDB(string filePath, long Attachmentid)
    {
        try
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            object ret = EYSql.ExecuteScalar(Program.PACEConnectionString, CommandType.Text,
                  @"UPDATE WF_Attachments SET Content = @a_sContent, UploadedDate = GETUTCDATE() WHERE Attachmentid = @a_sAttachmentid",
                  new SqlParameter("@a_sContent", fileData),
                  new SqlParameter("@a_sAttachmentid", Attachmentid));
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
    }
    public static string GetConnectionString(string name, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString(name);
        if (connectionString.Contains("initial catalog") || connectionString.Contains("database"))
        {
            return connectionString;
        }
        else
        {
            return Utilities.Cryptography.Decrypt3DES(connectionString);
        }
    }

    public static string GetPACEConnectionString(AppConfigure config)
    {
        string PACEConnectionString = GetConnectionString("PACEAsyncDatabase", configuration);
        bool PACEAlive = false;

        string SQL = @" SELECT * FROM CAU_Settings";
        string SQL2 = $" SELECT * INTO #ConnectionTest_{System.Environment.MachineName} FROM CAU_Settings";

        System.Data.DataTable dt = EYSql.ExecuteDataset(PACEConnectionString, CommandType.Text, SQL).Tables[0];
        if (dt.Rows.Count > 0)
        {
            //testing write
            try
            {
                EYSql.ExecuteNonQuery(PACEConnectionString, CommandType.Text, SQL2);
                PACEAlive = true;
                return PACEConnectionString;
            }
            catch (Exception e)
            {
                Log.Error("Connection error to PACE DR database", e.Message);
                PACEAlive = false;
            }
            dt = null;
        }

        PACEConnectionString = GetConnectionString("PACEDatabase", configuration);
        System.Data.DataTable dt2 = EYSql.ExecuteDataset(PACEConnectionString, CommandType.Text, SQL).Tables[0];
        if (dt2.Rows.Count > 0)
        {
            try
            {
                EYSql.ExecuteNonQuery(PACEConnectionString, CommandType.Text, SQL2);
                PACEAlive = true;
                return PACEConnectionString;
            }
            catch (Exception e)
            {
                Log.Error("Connection error to PACE DR database", e.Message);
                PACEAlive = false;
            }
            dt2 = null;
        }

        return null;
    }
}

#pragma warning restore CA1827 // Do not use Count() or LongCount() when Any() can be used
#pragma warning restore CS0219 // Variable is assigned but its value is never used
#pragma warning restore IDE0059 // Unnecessary assignment of a value
#pragma warning restore IDE0028 // Simplify collection initialization
#pragma warning restore CA1806 // Do not ignore method results
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
#pragma warning restore IDE0063 // Use simple 'using' statement
#pragma warning restore IDE0057 // Use range operator
#pragma warning restore CA2211 // Non-constant fields should not be visible
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0090 // Use 'new(...)'
