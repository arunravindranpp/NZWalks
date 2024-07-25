using AutoMapper;
using ConflictAutomation.Constants;
using ConflictAutomation.Extensions;
using ConflictAutomation.Models;
using ConflictAutomation.Models.FinScan.SubClasses.enums;
using ConflictAutomation.Models.ResearchSummaryEngine;
using ConflictAutomation.Services.ConclusionChecking;
using ConflictAutomation.Services.KeyGen;
using ConflictAutomation.Services.KeyGen.Enums;
using ConflictAutomation.Services.PreScreening;
using ConflictAutomation.Services.Sorting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using PACE;
using PACE.Domain.Models;
using PACE.Domain.Services;
using RestSharp;
using Serilog;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using static PACE.Domain.Models.ConflictResearchModel;

namespace ConflictAutomation.Services;

#pragma warning disable IDE0075 // Simplify conditional expression
#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0063 // Use simple 'using' statement
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
#pragma warning disable IDE0017 // Simplify object initialization
#pragma warning disable IDE0305 // Simplify collection initialization
#pragma warning disable CA2211 // Non-constant fields should not be visible
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0074 // Use compound assignment
#pragma warning disable IDE0270 // Use coalesce expression
#pragma warning disable CA1827 // Do not use Count() or LongCount() when Any() can be used
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable CA1860 // Avoid using 'Enumerable.Any()' extension method
#pragma warning disable CA1861 // Avoid constant arrays as arguments
#pragma warning disable IDE0037 // Use inferred member name

public class ConflictService
{
    private readonly AppConfigure _configuration;
    // private readonly ServiceProvider _serviceProvider;  // Warning CS0169: Field is never used
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private readonly IMapper _mapper;
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
    private KeyGen.KeyGen _keywordGenerator;

    public static SearchTypeEnum IndividualOrEntity { get; private set; }
    public static IWebHostEnvironment _env;

    //var summary = new CheckerQueue();
    //List<ResearchSummary> GISExtraResearch = new List<ResearchSummary>();
    public ConflictService(AppConfigure configuration)
    {
        _configuration = configuration;
        var configMap = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ColumnLookup, ColumnFilter>();
        });
        _mapper = new Mapper(configMap);
    }


    public ColumnFilter MapObjects(ColumnLookup sourceObject)
    {
        return _mapper.Map<ColumnFilter>(sourceObject);
    }


    public ProcessedChecks ProcessCheck(long conflictCheckID, long ProcessLogID, string ProcessStart, ConflictCheck conflictCheck, ServiceProvider serviceProvider, bool Rework = false, bool MultiEntity = false)
    {
        var ProcessedLog = new ProcessedChecks();
        try
        {
            int AUUnitRowindex = 0;
            var summary = new CheckerQueue();
            List<ResearchSummary> GISExtraResearch = new List<ResearchSummary>();
            List<ResearchSummary> MercuryExtraResearch = new List<ResearchSummary>();
            List<ResearchSummary> AdditionalResearch = new List<ResearchSummary>();
            List<ResearchSummary> MasterAdditionalResearch = new List<ResearchSummary>();
            List<ResearchSummary> GISGUP = new List<ResearchSummary>();
            List<ResearchSummary> CERGUP = new List<ResearchSummary>();
            List<CheckerQueue> lsCheckerQueue = new List<CheckerQueue>();
            List<string> AdditionalPartiesInherited = new List<string>();
            List<long> additionalPartyIDs = new List<long>();
            var gisList = new List<SearchEntitiesResponseItemViewModel>();
            QuestionnaireService questionnaireService = new QuestionnaireService();
            string destinationFilePath = "";
            lsCheckerQueue.Add(new CheckerQueue() { ConflictCheckID = conflictCheckID });
            var filename = ConflictCheckMasterWorkbookFileName(conflictCheckID, Rework);
            string destinationPath = _configuration.destinationFilePath + _configuration.Environment + @"\" + conflictCheckID.ToString() + @"\";
            string ReworkPath = _configuration.ReworkFilePath + _configuration.Environment + @"\";// + @"Research Template_" + conflictCheckID.ToString() + ".xlsm";
            destinationFilePath = Path.Combine(destinationPath, filename);
            ProcessedLog.IsErrored = false;

            if (Directory.Exists(destinationPath))
            {
                try
                {
                    Directory.Delete(destinationPath, true);
                }
                catch
                {
                    //Ignored
                }
            }
            //may not be need to delete directory but keeping for now

            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            try
            {
                if (!Rework)
                    File.Copy(_configuration.sourceFilePath, destinationFilePath, true);
                else
                {
                    Log.Information($"Checking rework path {ReworkPath}");
                    DirectoryInfo Reworkfolder = new DirectoryInfo(ReworkPath);
                    if (Reworkfolder.Exists) // else: Invalid folder!
                    {
                        Log.Information("Rework folder exists");
                        FileInfo[] files = Reworkfolder.GetFiles($"Research Template_{conflictCheckID}*.xlsm");
                        if (files.Count<FileInfo>() == 1)
                        {

                            FileInfo file = files.First<FileInfo>();
                            Log.Information($"Found file {file.Name}");
                            //foreach (FileInfo file in files)
                            //{
                            Log.Information($"Copy file from {file.FullName} to {destinationFilePath}");
                            File.Copy(file.FullName, destinationFilePath, true);
                            //}
                        }
                        else
                        {
                            Log.Information($"Found {files.Count<FileInfo>()} files for check id {conflictCheckID} in rework folder");
                            throw new Exception($"Found {files.Count<FileInfo>()} files for check id {conflictCheckID} in rework folder");
                        }
                    }
                    else  // Invalid folder
                    {
                        string errorMsg = $"Rework folder not found: {ReworkPath}";
                        Exception ex = new Exception(errorMsg);
                        LoggerInfo.DefaultLogAction(ex, errorMsg);
                        throw ex;
                    }

                }
            }
            catch (Exception ex)
            {
                LoggerInfo.LogException(ex);
                ProcessedLog.IsErrored = true;
                ProcessedLog.ErrorMessage = conflictCheckID + ": " + ex.Message;
                return ProcessedLog;
            }
            HashSet<string> IGNORE_ENTITIES = new HashSet<string>() { "na", "n/a" };

            summary = GetSummary(conflictCheckID);
            questionnaireService.GetQuestionnaire(conflictCheck.Assessment?.QuestionnaireID, summary);
            var regionalEntity = ExcelOperations.ReadRegionalData(_configuration.RegionalLocalPath);
            var sPLEntity = ExcelOperations.ReadSPLData(_configuration.SPLLocalPath);
            ExcelOperations.ReadOnshoreData(_configuration.onshoreContactLocalPath, summary);
            if (summary.ConflictCheckType == "Secondary")
            {
                AdditionalPartiesInherited.AddRange(conflictCheck.Assessment?.AdditionalParties
                    .Where(data => data.EntityIsInherited)
                    .SelectMany(data => new[] { data.EntityName, data.GUPEntityName }));
            }
            else if (summary.ConflictCheckType == "Additional Counterparty")
            {
                string query = @"SELECT AdditionalPartyID FROM WF_AdditionalParties WHERE ConflictCheckID =" + conflictCheckID;
                DataTable dtAdditional = EYSql.ExecuteDataset(_configuration.ConnectionString, CommandType.Text, query).Tables[0];
                additionalPartyIDs = dtAdditional.AsEnumerable()
                                                           .Select(row => row.Field<long>("AdditionalPartyID"))
                                                           .ToList();
                summary.researchSummary = summary.researchSummary.Where(i => additionalPartyIDs.Contains(i.AdditionalPartyID)
                                                                            || i.Role == "Main Client").ToList();
            }
            summary.researchSummary = DistinctEntity(summary.researchSummary);
            summary.researchSummary = DistinctEntityForEmptyUIDsAndLocation(summary.researchSummary);
            summary.researchSummary = DistinctEntityForEmptyUIDs(summary.researchSummary);
            List<ResearchSummary> matchedGUPWithAPG = IdentifyDupes(summary.researchSummary.Where(i => !i.Role.StartsWith("GUP of")).ToList(), summary.researchSummary.Where(i => i.Role.StartsWith("GUP of")).ToList(), "GIS");
            RemoveAPGGUPDupes(matchedGUPWithAPG, summary.researchSummary);

            if (!Rework)
            {
                var questionService = serviceProvider.GetService<IProcessQuestionnaireService>();
                List<ApprovalRungQuestion> processQuestionnaire = null;
                bool completedCheck = false;
                if (!summary.IsPursuit())
                    processQuestionnaire = questionService.GetApprovalRungQuestions(conflictCheck.Assessment.QuestionnaireID, "CONCH");
                if (summary.pursuitCheckPerformed)
                {
                    completedCheck = SummaryService.ProcessCompletedCheck(_configuration, conflictCheckID, conflictCheck, serviceProvider, summary, processQuestionnaire, destinationFilePath, destinationPath);
                }
                if (Convert.ToBoolean(conflictCheck.Resubmitted) && !completedCheck)
                {
                    var assessmentService = serviceProvider.GetService<IAssessmentService>();
                    var result = assessmentService
                        .GetConflictCheckAuditTrail(conflictCheckID, "Assessment Fields", conflictCheck, Program.US)
                        .Where(i => i.OldValue == "Complete")
                        .ToList();
                    if (result.Any())
                    {
                        completedCheck = SummaryService.ProcessResubmittedCheck(_configuration, conflictCheckID, conflictCheck, serviceProvider, summary, destinationFilePath, destinationPath, processQuestionnaire);
                    }
                }
                if (!completedCheck)
                {
                    ExcelOperations.SaveSummaryTab(conflictCheck, summary, destinationFilePath);
                    PreScreeningOperations.WritePrescreeningTab(_configuration, conflictCheckID, conflictCheck, summary.questionnaireSummary, summary.questionnaireAdditionalParties, processQuestionnaire, destinationFilePath);
                }
                Console.WriteLine("Summary Completed");
                Log.Information("SaveSummaryTab completed for:" + conflictCheckID);
                Console.WriteLine("AU Unit Grid Completed");
                ExcelOperations.SaveNuancesTab(conflictCheck?.Region, conflictCheck?.CountryName, regionalEntity, destinationFilePath);
                ExcelOperations.SaveBotUnitTab(summary, GISExtraResearch, destinationFilePath, true, out AUUnitRowindex, false, Rework);
            }
            if (summary.researchSummary.Count > 25)
                MultiEntity = true;

            if (Rework)
            {
                //Mark which research summary need to rework
                ExcelOperations.MarkRework(destinationFilePath, summary, this);
            }
            GetGUP(summary.researchSummary, summary, "GIS");
            GetGUP(summary.researchSummary, summary, "CER");

            if (!Rework)
            {
                ExcelOperations.SaveBotUnitTab(summary, GISExtraResearch, destinationFilePath, true, out AUUnitRowindex, generateLegends: MultiEntity, Rework);
                Log.Information("SaveBotUnitTab completed for:" + conflictCheckID);

                //After save let's modify the Role, otherwise it will have wrong worksheeno
                foreach (var ge in summary.researchSummary)
                {
                    foreach (var gup in matchedGUPWithAPG)
                    {
                        if (ge.EntityWithoutLegalExt == gup.EntityWithoutLegalExt)
                        {
                            if (!ge.Role.Contains(gup.Role))
                                ge.Role += ", " + gup.Role;
                            if (ge.SourceSystem.Contains("PACE APG") && !ge.SourceSystem.Contains("GIS") && gup.SourceSystem.Contains("GIS"))
                                ge.SourceSystem += ", " + "GIS";
                            if (ge.SourceSystem.Contains("PACE APG") && !ge.SourceSystem.Contains("CER") && gup.SourceSystem.Contains("CER"))
                                ge.SourceSystem += ", " + "CER";

                            if (string.IsNullOrEmpty(ge.AdditionalComments))
                                ge.AdditionalComments = gup.AdditionalComments;
                            else if (!ge.AdditionalComments.Contains(gup.AdditionalComments))
                                ge.AdditionalComments += ", " + gup.AdditionalComments;
                        }
                    }
                }
            }
            ProcessedLog.PACEExtractionEnd = DateTime.UtcNow.ToString();
            ProcessedLog.EntityCount = summary.researchSummary.Count.ToString();

            if (MultiEntity)
                goto SKIP_RESEARCH_FORMULTI_ENTITIES;
            List<ResearchSummary> worksheetList = new List<ResearchSummary>();

            for (int i = 0; i < summary.researchSummary.Count; i++)
            {
                //----------------------------------------------------------------
                ResearchSummary rs = summary.researchSummary[i];
                string dunsNumber = rs.DUNSNumber;      // Inform the DUNS Number
                string gisId = rs.GISID;                // Inform the GIS Id
                string paceApgLocation = rs.Country;    // Inform the PACE APG Location
                bool isInherited = false;
                if (ProcessedLog.EntitiesList != null && ProcessedLog.EntitiesList.Length > 0)
                    ProcessedLog.EntitiesList += " | " + rs.EntityName.ToString();
                else
                    ProcessedLog.EntitiesList = rs.EntityName.ToString();
                ProcessedLog.KeyGenStart = DateTime.UtcNow.ToString();

                _keywordGenerator = GetKeywordGenerator(rs.EntityName, dunsNumber, gisId, paceApgLocation);
                List<string> keywordsList = _keywordGenerator.GenerateKey(rs.EntityName);
                List<string> keywordsListForFinScanSearch = ComputeKeywordsForFinScanSearch(rs.EntityName);

                if (ProcessedLog.KeyGenCount != null && ProcessedLog.KeyGenCount.Length > 0)
                    ProcessedLog.KeyGenCount += " | " + keywordsList.Count.ToString();
                else
                    ProcessedLog.KeyGenCount = keywordsList.Count.ToString();

                if (String.IsNullOrEmpty(rs.GISID))
                    rs.GISID = "";
                if (String.IsNullOrEmpty(rs.MDMID))
                    rs.MDMID = "";
                if (String.IsNullOrEmpty(rs.DUNSNumber))
                    rs.DUNSNumber = "";

                rs.EntityWithoutLegalExt = DropLegalExtension(rs.EntityName, rs.Type);

                if (rs.IsClientSide)
                {
                    rs.IsCERResearch = false;
                    rs.IsCRRResearch = false;
                }
                if (!rs.PerformResearch)
                {
                    rs.IsGISResearch = false;
                    rs.IsCERResearch = false;
                    rs.IsCRRResearch = false;
                    rs.IsFinscanResearch = false;
                    rs.IsSPLResearch = false;
                    rs.GIS = "X";
                    rs.Mercury = "X";
                    rs.Finscan = "X";
                    rs.SPL = "X";
                    rs.CRR = "X";
                }
                isInherited = AdditionalPartiesInherited.Any(name => string.Equals(name, rs.EntityName, StringComparison.OrdinalIgnoreCase));
                if (isInherited && !rs.Role.Contains("GUP of Main Client"))
                {
                    rs.GIS = rs.Mercury = rs.CRR = rs.Finscan = rs.SPL = "I";
                    continue;
                }
                Console.WriteLine($"\nWorksheet Creating: {rs.WorksheetNo} - {rs.EntityName}");
                try
                {
                    if (rs.WorksheetNo.StartsWith("04"))
                    {
                        if (((Rework && rs.Rework) || (!Rework)) && rs.PerformResearch
                    && (!string.IsNullOrEmpty(rs.EntityName) && !IGNORE_ENTITIES.Contains(rs.EntityName.ToLower())))
                        {
                            rs.IsCRRResearch = rs.IsFinscanResearch = false; //save time during debug.
                            ExcelOperations.SaveUnitGridTab(conflictCheck, summary, sPLEntity, rs, summary.researchSummary,
                                destinationFilePath, destinationPath, keywordsList, _configuration, ProcessedLog,
                                GISExtraResearch, MercuryExtraResearch, summary.IsUKI,
                                summary.IsCRRGUP, keywordsListForFinScanSearch);
                            worksheetList.Add(rs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerInfo.LogException(ex, "ProcessCheck" + conflictCheckID.ToString());
                    LoggerInfo.WriteEventLog(ex);
                    Console.WriteLine("Exception triggered");
                    //Do nothing
                    ProcessedLog.IsErrored = true;
                    ProcessedLog.ErrorMessage = conflictCheckID + ": ProcessCheck:" + ex.Message;
                    return ProcessedLog;
                }

                if (Program.FinScanDebug)
                {
                    break;
                }
            }

        ADDITIONAL_RESEARCH:
            //ProcessConclusion GIS and CER additional entities
            if (GISExtraResearch.Count > 0 || MercuryExtraResearch.Count > 0)
            {
                foreach (var i in GISExtraResearch.OrderBy(j => j.WorksheetNo))
                {
                    //CER Missing Info
                    List<ResearchSummary> updatePACEAPGFromCER = new List<ResearchSummary>();

                    if (!String.IsNullOrEmpty(i.DUNSNumber))
                    {
                        ConflictService cs = new ConflictService(_configuration);
                        updatePACEAPGFromCER = cs.GetCERMissingInfo(i.DUNSNumber, "DUNS", i);
                    }
                    else if (!String.IsNullOrEmpty(i.MDMID))
                    {
                        updatePACEAPGFromCER = GetCERMissingInfo(i.MDMID, "MDM", i);
                    }
                    if (updatePACEAPGFromCER.Count > 0)
                    {
                        if (String.IsNullOrEmpty(i.MDMID) && updatePACEAPGFromCER[0].MDMID != null)
                            i.MDMID = updatePACEAPGFromCER[0].MDMID;
                        if (String.IsNullOrEmpty(i.Country) && updatePACEAPGFromCER[0].Country != null)
                            i.Country = updatePACEAPGFromCER[0].Country;
                        if (String.IsNullOrEmpty(i.DUNSNumber) && updatePACEAPGFromCER[0].DUNSNumber != null)
                            i.DUNSNumber = updatePACEAPGFromCER[0].DUNSNumber;
                        i.SourceSystem += ", CER";
                    }

                    if (i.EntityWithoutLegalExt == null)
                    {
                        KeyGenFactory keyGenFactory = Program.KeywordGeneratorFactory;
                        i.Type = keyGenFactory.GetSubjectType(
                                       i.EntityName,
                                       i.DUNSNumber,
                                       i.GISID,
                                       i.Country).ToString();
                        i.EntityWithoutLegalExt = DropLegalExtension(i.EntityName, i.Type);
                    }

                }

                foreach (var i in MercuryExtraResearch.OrderBy(j => j.WorksheetNo))
                {
                    //CER Missing info
                    List<ResearchSummary> updatePACEAPGFromGIS = new List<ResearchSummary>();
                    if (!String.IsNullOrEmpty(i.GISID))
                        updatePACEAPGFromGIS = GetGISMissingInfo(i.GISID, "GISID", i);
                    else if (!String.IsNullOrEmpty(i.DUNSNumber))
                    {
                        updatePACEAPGFromGIS = GetGISMissingInfo(i.DUNSNumber, "DUNS", i);
                    }
                    else if (!String.IsNullOrEmpty(i.MDMID))
                    {
                        updatePACEAPGFromGIS = GetGISMissingInfo(i.MDMID, "MDM", i);
                    }

                    if (updatePACEAPGFromGIS.Count > 0)
                    {
                        if (String.IsNullOrEmpty(i.MDMID) && updatePACEAPGFromGIS[0].MDMID != null)
                            i.MDMID = updatePACEAPGFromGIS[0].MDMID;
                        if (String.IsNullOrEmpty(i.Country) && updatePACEAPGFromGIS[0].Country != null)
                            i.Country = updatePACEAPGFromGIS[0].Country;
                        if (String.IsNullOrEmpty(i.GISID) && updatePACEAPGFromGIS[0].GISID != null)
                            i.GISID = updatePACEAPGFromGIS[0].GISID;
                        if (String.IsNullOrEmpty(i.DUNSNumber) && updatePACEAPGFromGIS[0].DUNSNumber != null)
                            i.DUNSNumber = updatePACEAPGFromGIS[0].DUNSNumber;
                        if (String.IsNullOrEmpty(i.CountryCode) && updatePACEAPGFromGIS[0].CountryCode != null)
                            i.CountryCode = updatePACEAPGFromGIS[0].CountryCode;
                        i.SourceSystem += ", GIS";
                    }
                    if (i.EntityWithoutLegalExt == null)
                    {
                        KeyGenFactory keyGenFactory = Program.KeywordGeneratorFactory;
                        i.Type = keyGenFactory.GetSubjectType(
                                       i.EntityName,
                                       i.DUNSNumber,
                                       i.GISID,
                                       i.Country).ToString();
                        i.EntityWithoutLegalExt = DropLegalExtension(i.EntityName, i.Type);
                    }
                }

                List<ResearchSummary> matchedGISAdditionalWithRS = IdentifyDupes(summary.researchSummary, GISExtraResearch, "GIS");

                foreach (ResearchSummary i in matchedGISAdditionalWithRS)
                {
                    if (i.ParentalRelationship != null)
                        i.Role = "GIS " + "N/A - Additional Research Unit (" + i.ParentalRelationship + ")";
                    else
                        i.Role = "GIS " + "N/A - Additional Research Unit";
                }

                foreach (var ge in summary.researchSummary)
                {
                    var test = matchedGISAdditionalWithRS.Where(x => x.EntityWithoutLegalExt.Equals(ge.EntityWithoutLegalExt)).ToList();
                    if (matchedGISAdditionalWithRS.Where(x => x.EntityWithoutLegalExt.Equals(ge.EntityWithoutLegalExt) &&
                                          (x.Role != null && !x.Role.Contains(ge.Role))).ToList().Count > 0)
                    {
                        ge.Role += ", " + matchedGISAdditionalWithRS.Where(x => x.EntityWithoutLegalExt.ToLower().Equals(ge.EntityWithoutLegalExt.ToLower()))
                            .Select(x => x.Role.ToString()).ToList().FirstOrDefault();
                    }
                }

                RemoveDupes(summary.researchSummary, GISExtraResearch, "GIS");

                RemoveDupes(summary.researchSummary, MercuryExtraResearch);

                //remove entities if they are part of GISExtra
                var matchedCERExtraWithGIS = (from c in MercuryExtraResearch
                                              where !!GISExtraResearch.Any(y => y.EntityName.ToLower() == c.EntityName.ToLower()
                                               && y.Country == c.Country && y.GISID == c.GISID && y.DUNSNumber == c.DUNSNumber)
                                              select c.EntityName);

                foreach (var ge in GISExtraResearch)
                {
                    if (matchedCERExtraWithGIS.Contains(ge.EntityName))
                    {
                        if (ge.SourceSystem == "GIS")
                            ge.SourceSystem += ", CER";
                    }
                }

                RemoveDupes(GISExtraResearch, MercuryExtraResearch);

                RemoveDupes(MasterAdditionalResearch, GISExtraResearch, "GIS");

                RemoveDupes(MasterAdditionalResearch, MercuryExtraResearch);

                GISExtraResearch = GISExtraResearch.DistinctBy(d => new { d.EntityName, d.DUNSNumber, d.GISID, d.Country }).ToList();
                MercuryExtraResearch = MercuryExtraResearch.DistinctBy(d => new { d.EntityName, d.DUNSNumber, d.GISID, d.Country }).ToList();

                int inc = 01;
                string ws = string.Empty;
                foreach (var i in GISExtraResearch.OrderBy(j => j.WorksheetNo))
                {
                    if (i.Role != null && i.Role.Contains("Additional Research Unit"))
                        i.IsAdditionalLoop = true;
                    // KeyGenFactory keyGenFactory = new KeyGenFactory(_configuration.ConnectionString);
                    KeyGenFactory keyGenFactory = Program.KeywordGeneratorFactory;
                    string modifiedWorksheet = string.Empty;
                    if (i.IsAdditionalLoop)
                        modifiedWorksheet = i.WorksheetNo.StrLeftBack(".").Trim();
                    else
                    {
                        // modifiedWorksheet = i.WorksheetNo.Split('.')[0];

                        if (i.WorksheetNo.StrRightBack(".").Contains("A"))
                            modifiedWorksheet = i.WorksheetNo.StrLeftBack(".A").Trim();
                        else
                            modifiedWorksheet = i.WorksheetNo;

                    }

                    //  string j = modifiedWorksheet.StrLeftBack(".").Trim();
                    if (modifiedWorksheet == ws)
                        inc++;
                    else
                        inc = 01;

                    if (i.IsAdditionalLoop)
                        ws = i.WorksheetNo.StrLeftBack(".").Trim();
                    else
                    {
                        if (i.WorksheetNo.StrRightBack(".").Contains("A"))
                            ws = i.WorksheetNo.StrLeftBack(".A").Trim();
                        else
                            ws = i.WorksheetNo;
                    }


                    i.SourceSystem = "GIS";

                    if (i.ParentalRelationship != null)
                    {
                        i.Role = "GIS" + "N/A - Additional Research Unit (" + i.ParentalRelationship + ")";
                        i.WorksheetNo = ws + ".A" + string.Format("{0:00}", inc);
                    }
                    else
                    {
                        if (i.Role != null && i.Role.StartsWith("GIS GUP of"))
                        {
                            //    var count = GISExtraResearch.Where(i => i.WorksheetNo == ws).Count();
                            //string orgworksheetNo = i.WorksheetNo;
                            //var t = GISExtraResearch.Where(i => i.WorksheetNo == orgworksheetNo).ToList();
                            if (GISExtraResearch.Where(i => i.WorksheetNo == ws).Count() > 1)
                                inc--;

                            i.WorksheetNo = ws + ".G" + string.Format("{0:00}", 01);


                        }
                        else
                        {
                            i.Role = "GIS" + "N/A - Additional Research Unit";
                            i.WorksheetNo = ws + ".A" + string.Format("{0:00}", inc);
                        }
                    }


                    i.Type = ExcelOperations.GetSubjectType(i);

                    // Carlos: Code block needed for Issues #1022641 and #1022538
                    if ((!i.Role.Contains("Significant")) && (!summary.IsPursuit()))
                    {
                        i.IsFinscanResearch = true;
                    }
                    // -------------------------------------------
                }
                int minc = 01;
                string mws = string.Empty;
                foreach (var i in MercuryExtraResearch.OrderBy(j => j.WorksheetNo))
                {
                    if (i.Role != null && i.Role.Contains("Additional Research Unit"))
                        i.IsAdditionalLoop = true;
                    // KeyGenFactory keyGenFactory = new KeyGenFactory(_configuration.ConnectionString);
                    KeyGenFactory keyGenFactory = Program.KeywordGeneratorFactory;
                    string modifiedMercuryWorksheet = string.Empty;
                    if (i.IsAdditionalLoop)
                        modifiedMercuryWorksheet = i.WorksheetNo.StrLeftBack(".").Trim();
                    else
                    {
                        //  modifiedMercuryWorksheet = i.WorksheetNo.Split('.')[0];

                        if (i.WorksheetNo.StrRightBack(".").Contains("D"))
                            modifiedMercuryWorksheet = i.WorksheetNo.StrLeftBack(".D").Trim();
                        else
                            modifiedMercuryWorksheet = i.WorksheetNo;
                    }

                    //  string j = modifiedWorksheet.StrLeftBack(".").Trim();
                    if (modifiedMercuryWorksheet == mws)
                        minc++;
                    else
                        minc = 01;

                    if (i.IsAdditionalLoop)
                        mws = i.WorksheetNo.StrLeftBack(".").Trim();
                    else
                    {
                        if (i.WorksheetNo.StrRightBack(".").Contains("D"))
                            mws = i.WorksheetNo.StrLeftBack(".D").Trim();
                        else
                            mws = i.WorksheetNo;
                    }

                    //if (i.IsAdditionalLoop)
                    //    mws = i.WorksheetNo.StrLeftBack(".").Trim();
                    //else
                    //    mws = i.WorksheetNo.Split('.')[0];

                    if (!i.Role.StartsWith("CER GUP of"))
                        i.Role = "CER" + "N/A - Additional Research Unit";
                    i.SourceSystem = "CER";
                    if (i.Role.StartsWith("CER GUP of"))
                    {
                        i.WorksheetNo = mws + ".C" + string.Format("{0:00}", 01);
                    }
                    else
                    {
                        //if (i.IsAdditionalLoop)
                        //    i.WorksheetNo = mws + ".D" + string.Format("{0:00}", minc);
                        //else
                        i.WorksheetNo = mws + ".D" + string.Format("{0:00}", minc);
                    }
                    i.Type = ExcelOperations.GetSubjectType(i);
                }
                //Combine them
                AdditionalResearch.AddRange(GISExtraResearch);
                AdditionalResearch.AddRange(MercuryExtraResearch);

                int GISExtraResearchCount = int.Parse(GISExtraResearch.Count.ToString());
                int MercuryExtraResearchCount = int.Parse(MercuryExtraResearch.Count.ToString());
                //Once we added GIS and Mercury ExtraResearch Let's empty them so that we can go in loop
                GISExtraResearch.Clear();
                MercuryExtraResearch.Clear();
                //Get GUP
                GetGUP(AdditionalResearch, summary, "GIS");
                GetGUP(AdditionalResearch, summary, "CER");
                CheckForAdditionalEntities(conflictCheck, summary, sPLEntity, destinationFilePath, destinationPath,
                     ProcessedLog, AdditionalResearch, GISExtraResearch, MercuryExtraResearch, worksheetList, summary.IsUKI, conflictCheckID, summary.IsCRRGUP);

                MasterAdditionalResearch.AddRange(AdditionalResearch);
                AdditionalResearch.Clear();

                ProcessedLog.EntityCount = (int.Parse(ProcessedLog.EntityCount) + int.Parse(MasterAdditionalResearch.Count.ToString())).ToString();
            }
            if (GISExtraResearch.Count > 0 || MercuryExtraResearch.Count > 0)
                goto ADDITIONAL_RESEARCH;

            Log.Information("AdditionalResearch:" + MasterAdditionalResearch.Count);
            Console.WriteLine("\nAdditionalResearch:" + MasterAdditionalResearch.Count);
            ExcelOperations.SaveBotUnitTab(summary, MasterAdditionalResearch, destinationFilePath, true, out AUUnitRowindex, false, Rework);
            if (!Rework)
                ExcelOperations.ClearBotUnitTab(summary, MasterAdditionalResearch, destinationFilePath);
            ExcelOperations.SaveBotUnitTab(summary, MasterAdditionalResearch, destinationFilePath, false, out AUUnitRowindex, generateLegends: true, Rework);

            SortingOperations.SortWorksheets(destinationFilePath);
            var researchSummaryGrid = ExcelOperations.SaveResearchSummaryTab(destinationFilePath);

            // #971644 - Consolidation 5b - Conclude Non-Client Side Entity Checks
            // #1019320 - Consolidation 5c - Conclude Client-Side Entity Checks
            ProcessConclusion(conflictCheckID, worksheetList, destinationFilePath, researchSummaryGrid, summary);

        SKIP_RESEARCH_FORMULTI_ENTITIES:
            AttachMasterWorkbookToPACEConflictCheck(conflictCheckID, destinationFilePath);

            if (_configuration.SaveToFileShare)
            {
                AttachMasterWorkbookToFileShare(conflictCheckID, destinationFilePath, _configuration.FileSharePath);
            }

            ProcessLog.UpdateProcessLog(ProcessLogID, ProcessedLog);

            //commenting this one as I don't want to show elasped time. 
            //TimeSpan span = Convert.ToDateTime(ProcessedLog.ProcessEnd).Subtract(Convert.ToDateTime(ProcessStart));
            //string ElapsedTime = string.Format("Days: " + span.Days + " Hours: " + span.Hours + " Minutes: " + span.Minutes + " Seconds: " + span.Seconds);

            //ExcelOperations.SaveSummaryTab(conflictCheck, summary, destinationFilePath, ElapsedTime);

            //if (Directory.Exists(ReworkPath)) //Delete original file from input directory
            //{
            //    try
            //    {
            //        Directory.Delete(ReworkPath, true);
            //    }
            //    catch
            //    {
            //        //Ignored
            //    }
            //}
            DirectoryInfo folder = new DirectoryInfo(ReworkPath);
            if (folder.Exists) // else: Invalid folder!
            {
                FileInfo[] files = folder.GetFiles($"Research Template_{conflictCheckID}*.xlsm");

                foreach (FileInfo file in files)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            return ProcessedLog;
        }
        catch (Exception ex)
        {
            var str = $"{ex.Message};{ex.InnerException}";
            Console.WriteLine("error processlog :" + str);
            ProcessedLog.IsErrored = true;
            ProcessedLog.ErrorMessage += ex.Message + "\n";
            ProcessLog.UpdateProcessLog(ProcessLogID, ProcessedLog);
            LoggerInfo.LogException(ex, "FinalProcessCheck - " + conflictCheckID.ToString());
            //throw;
            return ProcessedLog;
        }
    }


    private List<string> ComputeKeywordsForFinScanSearch(string individualName)
    {
        List<string> results = [];
        switch (IndividualOrEntity)
        {
            case SearchTypeEnum.Organization:
                results.AddRange(
                    ((KeyGenForEntities)_keywordGenerator).GenerateKeyForFinScanSearch(individualName));
                break;

            case SearchTypeEnum.Individual:
                results.AddRange(
                    ((KeyGenForIndividuals)_keywordGenerator).GenerateKeyForFinScanSearch(individualName));
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(IndividualOrEntity),
                    "Invalid value for property ConflictService.IndividualOrEntity. It is neither SearchTypeEnum.Organization(1) nor SearchTypeEnum.Individual(0).");
        }
        return results;
    }


    private static string ConflictCheckMasterWorkbookFileName(long conflictCheckID, bool isRework) =>
        $"Research Template_{conflictCheckID}" +
        $"_{DateTime.UtcNow.ShortTimestampFromUtc()}" +
        //$"{(isRework ? "_Rework" : string.Empty)}" + //Beth doesn't want it.
        ".xlsm";


    private void AttachMasterWorkbookToPACEConflictCheck(long conflictCheckID, string destinationFilePath)
    {
        if (!Program.UpdateCAUOutputInPACE)
        {
            return;
        }

        string msgAttachingMasterWorkbook = $"Inserting '{Path.GetFileName(destinationFilePath)}' " +
                                            $"into WF_Attachments under ConflictCheckID {conflictCheckID}...";
        Console.Write($"\n{msgAttachingMasterWorkbook}");

        ConflictCheckAttachmentUtility conflictCheckAttachmentUtility = new(_configuration.ConnectionString);
        Thread.Sleep(3000);
        long newAttachmentID = conflictCheckAttachmentUtility.InsertAttachment(conflictCheckID, destinationFilePath,
            CAUConstants.ATTACHMENTS_FOR_CONSOLIDATED_RESEARCH_RESULTS);
        conflictCheckAttachmentUtility.UpdateFileEntityToDB(newAttachmentID,
            conflictCheckID, CAUConstants.ENTITY_TYPE_ID_CONFLICT_CHECK, Program.US.GUI,
            CAUConstants.ATTACHMENTS_FOR_CONSOLIDATED_RESEARCH_RESULTS);

        Console.Write($"{msgAttachingMasterWorkbook} done");
    }
    private void AttachMasterWorkbookToFileShare(long conflictCheckID, string filePath, string networkPath)
    {
        try
        {
            var todaysdate = DateTime.Now;
            string formattedDate = todaysdate.ToString("MMM d yyyy");
            string folderPath = Path.Combine(networkPath, formattedDate, conflictCheckID.ToString());
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string fileName = Path.GetFileName(filePath);
            string destinationFilePath = Path.Combine(folderPath, fileName);
            File.Copy(filePath, destinationFilePath, overwrite: true);
            Console.WriteLine($"File successfully copied to {destinationFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            LoggerInfo.LogException(ex, conflictCheckID.ToString());
        }
    }
    private void ProcessConclusion(
        long conflictCheckID, List<ResearchSummary> worksheetList, string destinationFilePath,
        List<ResearchSummaryEntry> researchSummaryGrid, CheckerQueue summary)
    {
        ConclusionOperations conclusionOperations =
            new(worksheetList, _configuration.ConnectionString, conflictCheckID);
        conclusionOperations.ProcessConclusion(conflictCheckID, destinationFilePath,
            researchSummaryGrid, summary);
    }


    public static string RemoveSpecialCharacters(string input)
    {
        // Remove special characters, parentheses
        return new string(input.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
    }
    public static KeyGen.KeyGen GetKeywordGenerator(string involvedPartyName, string dunsNumber, string gisId, string paceApgLocation)
    {
        // The string parameters involvedPartyName, dunsNumber, gisId, paceApgLocation are passed
        // as arguments to method Program.KeywordGeneratorFactory.GetSubjectType(), 
        // which determines the Subject Type:
        //     Individual | Entity | UnableToDecide(consider Entity, by default)
        // Then, according to the Subject Type, the appropriate Keyword Generator is selected.

        KeyGen.KeyGen keywordGenerator;
        switch (Program.KeywordGeneratorFactory.GetSubjectType(
                    involvedPartyName, dunsNumber, gisId, paceApgLocation))
        {
            case SubjectTypeEnum.Individual:
                IndividualOrEntity = SearchTypeEnum.Individual;
                keywordGenerator = Program.KeywordGeneratorForIndividuals;
                break;

            case SubjectTypeEnum.Entity:
            case SubjectTypeEnum.UnableToDecide:
            default:
                IndividualOrEntity = SearchTypeEnum.Organization;
                keywordGenerator = Program.KeywordGeneratorForEntities;
                break;
        }

        return keywordGenerator;
    }

    public void CheckForAdditionalEntities(ConflictCheck conflictCheck, CheckerQueue queue, SPLEntity sPLEntity, string destinationFilePath,
       string destinationPath, ProcessedChecks ProcessedLog, List<ResearchSummary> AdditionalResearch,
       List<ResearchSummary> GISExtraResearch, List<ResearchSummary> MercuryExtraResearch,
       List<ResearchSummary> WorksheetList, bool IsUKI, long conflictCheckID, bool IsCRRGUP)
    {
        List<ResearchSummary> researchSummary = [];
        researchSummary.AddRange(AdditionalResearch);
        for (int i = 0; i < researchSummary.Count; i++)
        {
            //if (!rs.IsAdditionalResearchCompleted)
            //{
            var rs = researchSummary[i];

            string dunsNumber = rs.DUNSNumber;      // Inform the DUNS Number
            string gisId = rs.GISID;                // Inform the GIS Id
            string paceApgLocation = rs.Country;    // Inform the PACE APG Location


            if (ProcessedLog.EntitiesList != null && ProcessedLog.EntitiesList.Length > 0)
                ProcessedLog.EntitiesList += " | " + rs.EntityName.ToString();
            else
                ProcessedLog.EntitiesList = rs.EntityName.ToString();

            // rs.TurnFlagsOffWhenNoResearch();

            ProcessedLog.KeyGenStart = DateTime.UtcNow.ToString();

            _keywordGenerator = GetKeywordGenerator(rs.EntityName, dunsNumber, gisId, paceApgLocation);
            List<string> keywordsList = _keywordGenerator.GenerateKey(rs.EntityName);
            List<string> keywordsListForFinScanSearch = ComputeKeywordsForFinScanSearch(rs.EntityName);

            if (ProcessedLog.KeyGenCount != null && ProcessedLog.KeyGenCount.Length > 0)
                ProcessedLog.KeyGenCount += " | " + keywordsList.Count.ToString();
            else
                ProcessedLog.KeyGenCount = keywordsList.Count.ToString();

            Console.WriteLine($"\nWorksheet Creating Additional: {rs.WorksheetNo} - {rs.EntityName}");
            ExcelOperations.SaveUnitGridTab(conflictCheck, queue, sPLEntity, rs, researchSummary,
                destinationFilePath, destinationPath, keywordsList, _configuration, ProcessedLog,
                GISExtraResearch, MercuryExtraResearch, IsUKI,
                IsCRRGUP, keywordsListForFinScanSearch);
            WorksheetList.Add(rs);
            Console.WriteLine("Worksheet Finished:" + rs.WorksheetNo);
            ProcessedLog.IsErrored = false;

            //    rs.IsAdditionalResearchCompleted = true;
            //}
        }
    }


    public CheckerQueue GetSummary(long checkid)
    {
        var result = new CheckerQueue();
        try
        {
            var sqlQuery = string.Format("EXEC CAU_Summary '{0}'", checkid);
            var dataSet = EYSql.ExecuteDataset(_configuration.ConnectionString, CommandType.Text, sqlQuery);
            var dtPrescreening = dataSet.Tables.Count > 0 ? dataSet.Tables[0] : null;
            var dtResearchSummary = dataSet.Tables.Count > 1 ? dataSet.Tables[1] : null;
            var dtQuestionnaireSummary = dataSet.Tables.Count > 2 ? dataSet.Tables[2] : null;
            var cauDbQuery = new CauDbQuery(_configuration.ConnectionString);
            var subServiceLineCode = cauDbQuery.GetSubServiceLineCode(checkid);
            if (dtPrescreening?.Rows.Count > 0)
            {
                var dr = dtPrescreening.Rows[0];
                result = new CheckerQueue
                {
                    ConflictCheckID = dr.GetLong("ConflictCheckID"),
                    ConflictCheckType = dr.GetString("ConflictCheckType"),
                    AssessmentID = dr.GetLong("AssessmentID"),
                    ClientName = dr.GetString("ClientName"),
                    Region = dr.GetString("Region"),
                    CountryName = dr.GetString("CountryName"),
                    EngagementName = dr.GetString("EngagementName"),
                    SubServiceLine = dr.GetString("SubServiceLineName"),
                    SubServiceLineCode = subServiceLineCode,
                    Confidential = dr.GetString("Confidential"),
                    Services = dr.GetString("Services"),
                    EngagementDesc = dr.GetString("ServicesDetailedDescription"),
                    IsUKI = dr.GetBool("IsUKI"),
                    IsCRRGUP = _configuration.IsGUPLaw ? dr.GetBool("IsCRRGUP") : false
                };

                if (dtResearchSummary?.Rows.Count > 0)
                {
                    // KeyGenFactory keyGenFactory = new KeyGenFactory(_configuration.ConnectionString);
                    KeyGenFactory keyGenFactory = Program.KeywordGeneratorFactory;
                    int orderNo = 1;
                    result.researchSummary = dtResearchSummary.AsEnumerable()
                        .Select(drResearch => new ResearchSummary
                        {
                            OrderNo = orderNo++,
                            EntityName = drResearch.GetString("EntityName"),
                            Role = drResearch.GetString("Role"),
                            Country = drResearch.GetString("Country"),
                            DUNSNumber = drResearch.GetString("DUNSNumber"),
                            GISID = drResearch.GetString("GISID"),
                            MDMID = drResearch.GetString("MDMID"),
                            AdditionalPartyID = drResearch.GetLong("AdditionalPartyID"),
                            AdditionalComments = drResearch.GetString("AdditionalComments"),
                            // For Carlos' tests only:
                            // IsFinscanResearch = (result.ConflictCheckID == 1412701) || drResearch.GetBool("IsFinscan"), 
                            IsFinscanResearch = Convert.ToBoolean(drResearch.GetInt("IsFinscan")),  // User Story #1012887: Skip FinScan search for Main Clients
                            IsClientSide = Convert.ToBoolean(drResearch.GetInt("IsClientSide")),
                            PerformResearch = Convert.ToBoolean(drResearch.GetInt("PerformResearch")),
                            CountryCode = drResearch.GetString("CountryCode"),
                            Type = keyGenFactory.GetSubjectType(
                                        drResearch.GetString("EntityName"),
                                        drResearch.GetString("DUNSNumber"),
                                        drResearch.GetString("GISID"),
                                        drResearch.GetString("Country")).ToString()
                        })
                        .ToList();
                    result.researchSummary.ForEach(x => x.EntityWithoutLegalExt = DropLegalExtension(x.EntityName, x.Type));

                    foreach (var rs in result.researchSummary.ToList())
                    {
                        //GIS Missing info
                        List<ResearchSummary> updatePACEAPGFromGIS = new List<ResearchSummary>();
                        if (!String.IsNullOrEmpty(rs.GISID))
                            updatePACEAPGFromGIS = GetGISMissingInfo(rs.GISID, "GISID", rs);
                        else if (!String.IsNullOrEmpty(rs.DUNSNumber))
                        {
                            updatePACEAPGFromGIS = GetGISMissingInfo(rs.DUNSNumber, "DUNS", rs);
                        }
                        else if (!String.IsNullOrEmpty(rs.MDMID))
                        {
                            updatePACEAPGFromGIS = GetGISMissingInfo(rs.MDMID, "MDM", rs);
                        }

                        if (updatePACEAPGFromGIS.Count > 0)
                        {
                            if (String.IsNullOrEmpty(rs.MDMID) && !String.IsNullOrEmpty(updatePACEAPGFromGIS[0].MDMID))
                            {
                                rs.MDMID = updatePACEAPGFromGIS[0].MDMID;
                                if (!rs.SourceSystem.Contains("GIS"))
                                    rs.SourceSystem += ", GIS";
                            }
                            if (String.IsNullOrEmpty(rs.Country) && !String.IsNullOrEmpty(updatePACEAPGFromGIS[0].Country))
                            {
                                rs.Country = updatePACEAPGFromGIS[0].Country;
                                if (!rs.SourceSystem.Contains("GIS"))
                                    rs.SourceSystem += ", GIS";
                            }
                            if (String.IsNullOrEmpty(rs.GISID) && !String.IsNullOrEmpty(updatePACEAPGFromGIS[0].GISID))
                            {
                                rs.GISID = updatePACEAPGFromGIS[0].GISID;
                                if (!rs.SourceSystem.Contains("GIS"))
                                    rs.SourceSystem += ", GIS";
                            }
                            if (String.IsNullOrEmpty(rs.DUNSNumber) && !String.IsNullOrEmpty(updatePACEAPGFromGIS[0].DUNSNumber))
                            {
                                rs.DUNSNumber = updatePACEAPGFromGIS[0].DUNSNumber;
                                if (!rs.SourceSystem.Contains("GIS"))
                                    rs.SourceSystem += ", GIS";
                            }
                            if (String.IsNullOrEmpty(rs.CountryCode) && !String.IsNullOrEmpty(updatePACEAPGFromGIS[0].CountryCode))
                            {
                                rs.CountryCode = updatePACEAPGFromGIS[0].CountryCode;
                                if (!rs.SourceSystem.Contains("GIS"))
                                    rs.SourceSystem += ", GIS";
                            }
                        }
                        //CER Missing Info
                        List<ResearchSummary> updatePACEAPGFromCER = new List<ResearchSummary>();

                        if (!String.IsNullOrEmpty(rs.DUNSNumber))
                        {
                            updatePACEAPGFromCER = GetCERMissingInfo(rs.DUNSNumber, "DUNS", rs);
                        }
                        else if (!String.IsNullOrEmpty(rs.MDMID))
                        {
                            updatePACEAPGFromCER = GetCERMissingInfo(rs.MDMID, "MDM", rs);
                        }

                        if (updatePACEAPGFromCER.Count > 0)
                        {
                            if (String.IsNullOrEmpty(rs.MDMID) && !String.IsNullOrEmpty(updatePACEAPGFromCER[0].MDMID))
                            {
                                rs.MDMID = updatePACEAPGFromCER[0].MDMID;
                                if (!rs.SourceSystem.Contains("CER"))
                                    rs.SourceSystem += ", CER";
                            }
                            if (String.IsNullOrEmpty(rs.Country) && !String.IsNullOrEmpty(updatePACEAPGFromCER[0].Country))
                            {
                                rs.Country = updatePACEAPGFromCER[0].Country;
                                if (!rs.SourceSystem.Contains("CER"))
                                    rs.SourceSystem += ", CER";
                            }
                            if (String.IsNullOrEmpty(rs.DUNSNumber) && !String.IsNullOrEmpty(updatePACEAPGFromCER[0].DUNSNumber))
                            {
                                rs.DUNSNumber = updatePACEAPGFromCER[0].DUNSNumber;
                                if (!rs.SourceSystem.Contains("CER"))
                                    rs.SourceSystem += ", CER";
                            }
                        }

                        if (rs.Role.StartsWith("GUP of"))
                        {
                            List<ResearchSummary> updatePACEFromGIS = new List<ResearchSummary>();
                            if (!String.IsNullOrEmpty(rs.GISID))
                                updatePACEFromGIS = GetGISGUPInfo(rs.GISID, "GISID", rs);
                            else if (!String.IsNullOrEmpty(rs.DUNSNumber))
                            {
                                updatePACEFromGIS = GetGISGUPInfo(rs.DUNSNumber, "DUNS", rs);
                            }
                            else if (!String.IsNullOrEmpty(rs.MDMID))
                            {
                                updatePACEFromGIS = GetGISGUPInfo(rs.MDMID, "MDM", rs);
                            }

                            if (updatePACEFromGIS.Count > 0)
                            {
                                rs.MDMID = updatePACEFromGIS[0].MDMID;
                                rs.Country = updatePACEFromGIS[0].Country;
                                rs.CountryCode = updatePACEFromGIS[0].CountryCode;
                                if (String.IsNullOrEmpty(rs.GISID) && !String.IsNullOrEmpty(updatePACEFromGIS[0].GISID))
                                    rs.GISID = updatePACEFromGIS[0].GISID;
                                if (String.IsNullOrEmpty(rs.DUNSNumber) && !String.IsNullOrEmpty(updatePACEFromGIS[0].DUNSNumber))
                                    rs.DUNSNumber = updatePACEFromGIS[0].DUNSNumber;
                                rs.Type = updatePACEFromGIS[0].Type;
                                if (!rs.SourceSystem.Contains("GIS"))
                                    rs.SourceSystem += ", GIS";
                                //Tejal - you can not update EntityWithoutLegalExt here because it will overwrite GIS values. 
                                //   rs.EntityWithoutLegalExt = updatePACEFromGIS[0].EntityWithoutLegalExt;
                            }
                        }
                    }
                }
                if (dtQuestionnaireSummary?.Rows.Count > 0)
                {
                    result.questionnaireSummary = dtQuestionnaireSummary.AsEnumerable()
                        .Select(dtQuestionnaire => new QuestionnaireSummary
                        {
                            Title = dtQuestionnaire.GetString("Title"),
                            Answer = dtQuestionnaire.GetString("Answer"),
                            Explanation = dtQuestionnaire.GetString("Explanation"),
                            QuestionNumber = dtQuestionnaire.GetString("QuestionNumber")
                        })
                        .ToList();
                }
            }
            var pursuitCheckPerformed = result.questionnaireSummary.FirstOrDefault(i => i.Title == "Pursuit Check Performed");
            if (pursuitCheckPerformed != null)
            {
                if (!string.IsNullOrEmpty(pursuitCheckPerformed.Explanation) && string.Equals(pursuitCheckPerformed.Answer, "YES", StringComparison.OrdinalIgnoreCase))
                {
                    result.pursuitCheckPerformed = true;
                    result.pursuitCheckID = pursuitCheckPerformed.Explanation;
                }
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, "GetSummary-" + checkid.ToString());
        }
        return result;
    }


    public CEREmbeddFiles GetMercuryEntitiesData(ConflictCheck conflictcheck, ResearchSummary rs, List<string> keywordsList,
        string sEntityName, string sDUNSNumber, string destinationFilePath, bool IsUKI)
    {
        List<MercuryEntity_SpreadSheet> mdata = new List<MercuryEntity_SpreadSheet>();
        List<MercuryEntity_SpreadSheet> CERData = new List<MercuryEntity_SpreadSheet>();
        try
        {
            System.Data.DataTable dt = null;
            System.Data.DataTable dtDUNS = null;
            string KeywordSearch = string.Empty;
            string SearchMode = string.Empty;

            string sFileName = string.Empty;

            string EngagementOpenDateFrom = DateTime.Now.Date.AddYears(-5).ToString("yyyy-MM-dd");
            string EngagementOpenDateTo = DateTime.Now.Date.ToString("yyyy-MM-dd");

            if (IsUKI)
                EngagementOpenDateFrom = DateTime.Now.Date.AddYears(-7).ToString("yyyy-MM-dd");

            foreach (var keyword in keywordsList)
            {
                //  dont know why we clean the keyword. 
                //  string cleanedKeyword = GetValidName(keyword);
                string cleanedKeyword = keyword.ToString();
                string formattedQuery = string.Format("EXEC [dbo].[CAU_REP_Conflict_Checker_getData] '{0}', '', '', '', '{1}', '{2}'", cleanedKeyword.Replace("'", "''"), EngagementOpenDateFrom, EngagementOpenDateTo);

                System.Data.DataTable dtInternal = EYSql.ExecuteDataset(_configuration.ConnectionString, CommandType.Text, formattedQuery).Tables[0];

                if (dtInternal != null)
                {
                    KeywordSearch = keyword;
                    SearchMode = "Keywords";
                    sFileName = keyword;
                }

                if (dt != null)
                    dt.Merge(dtInternal);
                else
                    dt = dtInternal;
            }
            if (!String.IsNullOrEmpty(sDUNSNumber))
            {
                string formattedQuery = string.Format("EXEC [dbo].[CAU_REP_Conflict_Checker_getData] '{0}', '', '', '', '{1}', '{2}'", sDUNSNumber, EngagementOpenDateFrom, EngagementOpenDateTo);


                dtDUNS = EYSql.ExecuteDataset(_configuration.ConnectionString, CommandType.Text, formattedQuery).Tables[0];

                if (dtDUNS != null)
                {
                    KeywordSearch = KeywordSearch + "_" + sDUNSNumber;
                    SearchMode = SearchMode + "_" + "DUNS No";
                    sFileName = sFileName + "_" + sDUNSNumber;
                }
            }

            if (dt != null && dtDUNS != null)
            {
                dt.Merge(dtDUNS);
            }

            if (dt == null && dtDUNS != null)
                dt = dtDUNS;


            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    //int iBN_Fuzzy = Common.GetFuzzyPercentage(rs.EntityWithoutLegalExt, 
                    //       DropLegalExtension(dr.GetString("Client"), rs.Type));

                    //int iAN_Fuzzy = 0;

                    //int _iAN_Fuzzy = Common.GetFuzzyPercentage(rs.EntityWithoutLegalExt, DropLegalExtension(dr.GetString("DunsName"), rs.Type));
                    //if (iAN_Fuzzy < _iAN_Fuzzy) iAN_Fuzzy = _iAN_Fuzzy; //largest fuzzy match %

                    int iAN_Fuzzy = 0;
                    int iBN_Fuzzy = 0;

                    mdata.Add(new MercuryEntity_SpreadSheet()
                    {
                        CERID = dr.GetLong("CERID"),
                        EngagementOpenDate = dr.GetDateTime("EngagementCreationDate"),//Modified after error : String '' was not recognized as a valid DateTime

                        ClientID = dr.GetString("ClientID"),
                        Client = dr.GetString("Client"),
                        DunsNumber = dr.GetString("DunsNumber"),
                        DunsName = dr.GetString("DunsName"),
                        DunsLocation = dr.GetString("DunsLocation"),
                        UltimateDunsNumber = dr.GetString("AccountID"),
                        Account = dr.GetString("Account"),
                        EngagementID = dr.GetString("EngagementID"),
                        Engagement = dr.GetString("Engagement"),
                        PACEID = dr.GetString("GTACStatusFolderName"),
                        PACEStatus = dr.GetString("GTACStatus"),
                        EngagementGlobalService = dr.GetString("EngagementGlobalService"),
                        EngagementServiceLine = dr.GetString("EngagementServiceLine"),
                        EngagementSubServiceLine = dr.GetString("EngagementSubServiceLine"),
                        EngagementCountry = dr.GetString("EngagementCountry") + " / " + dr.GetString("EngagementRegion"),
                        EngagementStatus = dr.GetString("EngagementStatus"),
                        EngagementStatusEffectiveDate = string.IsNullOrEmpty(dr.GetString("EngagementStatusEffectiveDate")) ? null : Convert.ToDateTime(dr.GetString("EngagementStatusEffectiveDate")),
                        EngagementOpenDateShowcase = string.IsNullOrEmpty(dr.GetString("EngagementCreationDate")) ? null : Convert.ToDateTime(dr.GetString("EngagementCreationDate")),
                        EngagementLastTimeChargedDate = string.IsNullOrEmpty(dr.GetString("EngagementLastTimeChargedDate")) ? null : Convert.ToDateTime(dr.GetString("EngagementLastTimeChargedDate")),
                        LatestInvoiceIssuedDate = string.IsNullOrEmpty(dr.GetString("LatestInvoiceIssuedDate")) ? null : Convert.ToDateTime(dr.GetString("LatestInvoiceIssuedDate")),
                        EngagementType = dr.GetString("EngagementType"),
                        GCSP = dr.GetString("GCSP"),
                        GCSPEmail = dr.GetString("GCSPEmail"),
                        EngagementPartner = dr.GetString("EngagementPartner"),
                        EngagementPartnerEmail = dr.GetString("EngagementPartnerEmail"),
                        EngagementManager = dr.GetString("EngagementManager"),
                        TechnologyIndicatorCdJoin = dr.GetString("TechnologyIndicatorCdJoin"),
                        NER = dr.GetString("NER"),
                        ChargedHours = dr.GetString("ChargedHours"),
                        BilledFees = dr.GetString("BilledFees"),
                        CurrencyCode = dr.GetString("CurrencyCode"),
                        AccountChannel = dr.GetString("AccountChannel"),
                        SECFlag = dr.GetString("SECFlag"),
                        EngagementReportingOrg = dr.GetString("EngagementReportingOrg"),
                        EngagementOpenDateFrom = string.IsNullOrEmpty(EngagementOpenDateFrom) ? null : Convert.ToDateTime(EngagementOpenDateFrom),
                        EngagementOpenDateTo = string.IsNullOrEmpty(EngagementOpenDateTo) ? null : Convert.ToDateTime(EngagementOpenDateTo),
                        AN_Fuzzy = iAN_Fuzzy,
                        BN_Fuzzy = iBN_Fuzzy,
                        SearchMode = SearchMode,
                        KeywordUsed = KeywordSearch

                    });
                    //   CERData.AddRange(mdata);
                }
                CERData.AddRange(mdata);
            }
            if (CERData.Count > 0)
            {
                var ls = CERData.DistinctBy(i => i.CERID);

                Dictionary<string, string> keyValuePairsClients = new Dictionary<string, string>();
                Dictionary<string, string> keyValuePairsDUNSName = new Dictionary<string, string>();

                var lstClient = ls.Select(i => i.Client).Distinct().ToList();
                var lstDUNSName = ls.Select(i => i.DunsName).Distinct().ToList();

                lstClient.ToList().ForEach(x => keyValuePairsClients.Add(x, DropLegalExtension(x, rs.Type)));
                lstDUNSName.ToList().ForEach(x => keyValuePairsDUNSName.Add(x, DropLegalExtension(x, rs.Type)));

                foreach (var l in ls.ToList())
                {
                    //Client
                    string clientWithoutLE = keyValuePairsClients.FirstOrDefault(x => x.Key.Equals(l.Client)).Value.ToString();
                    l.BN_Fuzzy = Common.GetFuzzyPercentage(rs.EntityWithoutLegalExt, clientWithoutLE);
                    l.EntityWithoutLegalExt = clientWithoutLE;

                    //DUNSName
                    int iAN_FuzzyTemp = 0;
                    string dunsNameWithoutLE = keyValuePairsDUNSName.FirstOrDefault(x => x.Key.Equals(l.DunsName)).Value.ToString();
                    int _iAN_Fuzzy = Common.GetFuzzyPercentage(rs.EntityWithoutLegalExt, dunsNameWithoutLE);

                    if (iAN_FuzzyTemp < _iAN_Fuzzy) iAN_FuzzyTemp = _iAN_Fuzzy; //largest fuzzy match %
                    l.AN_Fuzzy = iAN_FuzzyTemp;
                    l.DUNSNameWithoutLegalExt = dunsNameWithoutLE;
                }

                CERData = ls.ToList();

                var removedCERIDls = ls.Select(i => new
                {
                    i.ClientID,
                    i.Client,
                    i.DunsNumber,
                    i.DunsName,
                    i.DunsLocation,
                    i.UltimateDunsNumber,
                    i.Account,
                    i.EngagementID,
                    i.Engagement,
                    i.PACEID,
                    i.PACEStatus,
                    i.EngagementGlobalService,
                    i.EngagementServiceLine,
                    i.EngagementSubServiceLine,
                    i.EngagementCountry,
                    i.EngagementStatus,
                    i.EngagementStatusEffectiveDate,
                    i.EngagementOpenDateShowcase,
                    i.EngagementLastTimeChargedDate,
                    i.LatestInvoiceIssuedDate,
                    i.EngagementType,
                    i.GCSP,
                    i.GCSPEmail,
                    i.EngagementPartner,
                    i.EngagementPartnerEmail,
                    i.EngagementManager,
                    i.TechnologyIndicatorCdJoin,
                    i.NER,
                    i.ChargedHours,
                    i.BilledFees,
                    i.CurrencyCode,
                    i.AccountChannel,
                    i.SECFlag,
                    i.EngagementReportingOrg,
                    i.EngagementOpenDateFrom,
                    i.EngagementOpenDateTo,
                    i.AN_Fuzzy,
                    i.BN_Fuzzy,
                    i.SearchMode,
                    i.KeywordUsed
                });

                if (removedCERIDls.ToList().Count > 0)
                {
                    DataSet dset = new DataSet();
                    System.Data.DataTable dtable = Common.ToDataTable(removedCERIDls.OrderByDescending(o => o.BN_Fuzzy)
                        .ThenByDescending(o => o.EngagementOpenDateShowcase).ToList());
                    dtable.TableName = "Combined Data";
                    dset.Tables.Add(dtable);

                    string _sFileName = string.IsNullOrEmpty(sFileName) ? sEntityName.Replace(" ", "_") : sFileName.Replace(" ", "_");
                    _sFileName += ".xlsx";
                    string sFilePath = Path.Combine(destinationFilePath, rs.WorksheetNo, "CER", _sFileName);
                    ExcelOperations.GenerateUnitGridTabEmbeddedFiles(sFilePath, dset, rs, 3, TabCategory.TabCEREmbedded);
                }
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, "GetMercuryEntitiesData-" + conflictcheck.ConflictCheckID.ToString());
        }

        return new CEREmbeddFiles
        {

            //EntityMatchCount = CERData.ToList().Where(i => i.AN_Fuzzy.Equals(100) || i.BN_Fuzzy.Equals(100))
            //                        .Select(i => new
            //                        {
            //                            sDUNSNumber = i.DunsNumber
            //                        }).Distinct().ToList().Count,
            EntityMatchCount = 0,
            EntityMatchRecords = CERData
        };

    }
    public static string GetValidName(string filename)
    {
        string invalidChars = new string(Path.GetInvalidFileNameChars()) + ":/?*\"<>|.'";
        string pattern = "[" + Regex.Escape(invalidChars) + "]";
        string validFileName = Regex.Replace(filename, pattern, "");
        return validFileName;
    }
    public SearchEntitiesResponseViewModel GetGISEntitiesFromGISWebAPIAsync(GISSearch gisSearch, bool PassFuzzy = false)
    {
        SearchEntitiesResponseViewModel GISResult = new SearchEntitiesResponseViewModel();
        try
        {
            string searchTerm = string.Empty;

            if (gisSearch.Enity != null)
                searchTerm = gisSearch.Enity.ToString();

            if (gisSearch.DUNSNumber != null)
                searchTerm = gisSearch.DUNSNumber.ToString();

            if (gisSearch.MDMID != null)
                searchTerm = gisSearch.MDMID.ToString();

            if (gisSearch.GISID != null)
                searchTerm = gisSearch.GISID.ToString();

            SearchEntitiesRequestViewModel requestViewModel = new SearchEntitiesRequestViewModel()
            {

                SearchTerm = searchTerm
            };

            //Find in GIS
            return GISResult = FindEntitiesFromGISWebAPIAsync(requestViewModel, gisSearch.IsIndividual, PassFuzzy);
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
            return GISResult;
        }
    }

    public static GISEmbeddFiles GetGISInfoForEmbeddedFiles(ConflictCheck ConflictCheck, ResearchSummary rs, List<SearchEntitiesResponseItemViewModel> GISResult,
        List<ResearchSummary> GISExtraResearch, string sEntityName, string destinationFilePath, AppConfigure configure, string sFileName)
    {

        List<GISEmbeddFiles> lsSERVM = new List<GISEmbeddFiles>();

        //Add GIS entities to a list
        List<SearchEntitiesResponseItemViewModel_SpreadSheet> lsSERV = new List<SearchEntitiesResponseItemViewModel_SpreadSheet>();
        string RestrictionGISIDs = string.Empty;
        foreach (SearchEntitiesResponseItemViewModel _serv in GISResult ?? Enumerable.Empty<SearchEntitiesResponseItemViewModel>())
        {
            int iBN_Fuzzy = Common.GetFuzzyPercentage(rs.EntityWithoutLegalExt, DropLegalExtension(_serv.EntityName, rs.Type));

            int iAN_Fuzzy = 0;

            if (_serv.Aliases.Count > 0)
            {
                foreach (string sAlias in _serv.Aliases)
                {
                    int _iAN_Fuzzy = Common.GetFuzzyPercentage(rs.EntityWithoutLegalExt, DropLegalExtension(sAlias, rs.Type));
                    if (iAN_Fuzzy < _iAN_Fuzzy) iAN_Fuzzy = _iAN_Fuzzy; //largest fuzzy match %
                }
            }

            string MDMID = _serv.MDMID.ToString();

            if (MDMID == "0")
                MDMID = "";

            RestrictionGISIDs += "," + _serv.GisId.ToString();

            lsSERV.Add(new SearchEntitiesResponseItemViewModel_SpreadSheet()
            {

                GISID = _serv.GisId,
                UltimateParent = (_serv.GisId.ToString() == _serv.GupGisId) ? (_serv.HasIndirectParents ? "Y/L" : "Y") : (_serv.HasIndirectParents ? "N/L" : "N"),
                BN_Fuzzy = iBN_Fuzzy,
                AN_Fuzzy = iAN_Fuzzy,
                IdentificationNumber = @$"GIS ID: {_serv.GisId}, DUNS: {_serv.DUNSNumber}, MDM ID: {MDMID}, GFIS ID: {_serv.GFISClientID}, Mercury ID: {MDMID}, National ID: {_serv.NationalID}",
                BusinessName = _serv.EntityName,
                Alias = string.Join(", ", _serv.Aliases),
                GUPESDID = _serv.GupGisId,
                UltimateBusinessName = _serv.GUPName,
                //Street_City = _serv.City,
                City = _serv.City,
                //State_Country = _serv.CountryName,
                StateProvinceName = _serv.StateProvinceName,
                //  MarkupCodes = GetMarkupCodes(_serv, configure.GISConnectionString),
                GISP = _serv.Gisp,
                Country = _serv.CountryName,
                DUNS = _serv.DUNSNumber,
                MDMID = MDMID,
                CountryCode = _serv.CountryCode

            });
        }
        if (!string.IsNullOrEmpty(RestrictionGISIDs))
            RestrictionGISIDs = RestrictionGISIDs.Remove(0, 1);

        List<Restrictions> restrictions = new List<Restrictions>();
        restrictions = GetRestrictions(RestrictionGISIDs, configure.GISConnectionString);

        foreach (var item in lsSERV)
        {
            if (restrictions.Where(r => r.GISID == item.GISID).ToList().Count > 0)
                item.MarkupCodes = restrictions.FirstOrDefault(m => m.GISID == item.GISID).RestrictionName;
        }
        //Generate excel file
        if (lsSERV.Count > 0)
        {
            lsSERV = lsSERV.OrderByDescending(o => o.BN_Fuzzy).ToList();

            DataSet ds = new DataSet();
            System.Data.DataTable dt = Common.ToDataTable(lsSERV);
            dt.TableName = "Combined Data";
            dt.Columns.Remove("CountryCode");
            ds.Tables.Add(dt);

            string _sFileName = string.IsNullOrEmpty(sFileName) ? sEntityName.Replace(" ", "_") : sFileName.Replace(" ", "_");
            _sFileName += ".xlsx";
            string sFilePath = Path.Combine(destinationFilePath, rs.WorksheetNo, "GIS", _sFileName);
            ExcelOperations.GenerateUnitGridTabEmbeddedFiles(sFilePath, ds, rs, 3, TabCategory.TabGISEmbedded);
        }

        bool isGISIDmatch = false;
        bool isGISIDTurquoise = lsSERV.ToList().Where(i => rs.GISID != null && i.GISID.ToString().Equals(rs.GISID.ToString())).ToList().Count == 1 ? true : false;

        if (string.IsNullOrEmpty(rs.GISID))
            isGISIDmatch = false;
        else
            isGISIDmatch = lsSERV.ToList().Where(i => i.GISID.ToString().Equals(rs.GISID.ToString())).ToList().Count == 1 ? true : false;

        //Based on new rule if we find multiple matches with ANFuzzy OR BNFuzzy then match country
        List<SearchEntitiesResponseItemViewModel_SpreadSheet> ReturnEntityMatchRecords = lsSERV.ToList().Where(i => i.AN_Fuzzy.Equals(100) || i.BN_Fuzzy.Equals(100)).ToList();
        if (ReturnEntityMatchRecords.Count >= 1 && !isGISIDTurquoise)
        {
            if (!string.IsNullOrEmpty(rs.Country))
            {
                if (ReturnEntityMatchRecords.Where(i => i.CountryCode.Equals(rs.CountryCode)).ToList().Count == 1)
                {
                    ReturnEntityMatchRecords = ReturnEntityMatchRecords.Where(i => i.CountryCode.Equals(rs.CountryCode)).ToList();
                }
                //as per Shashank (6/12/24) and //[3:16 PM] Manisha Viswanathan
                //Also, if there are multiple 100 % name matches but with no UID match, then I don't think we would want AU to do any thing with those multiple
                //matches as we don't yet know which one is the correct match.That is why we flag it to humans saying 'Multiple close matches found'.
                //if multiple then don't do anything
                else
                    isGISIDmatch = true;
            }
            else
                isGISIDmatch = true;
        }
        if (ReturnEntityMatchRecords.Count == 0 && !isGISIDTurquoise)
            isGISIDmatch = true;

        if (!isGISIDmatch && string.IsNullOrEmpty(rs.GISID))
        {
            //Rule#1 to add additional GIS research          
            var anFuzzy = ReturnEntityMatchRecords.ToList().Where(i => i.AN_Fuzzy.Equals(100)).ToList();

            var missingBNFuzzy = anFuzzy.Where(i => !i.BusinessName.Equals(rs.EntityName)).ToList();

            if (GISExtraResearch.Count > 0)
            {
                if (missingBNFuzzy.ToList().Where(x => !String.IsNullOrEmpty(x.DUNS)).Count() > 0)
                {
                    missingBNFuzzy.RemoveAll(x => !!GISExtraResearch.Any(y => y.EntityWithoutLegalExt.ToLower() == DropLegalExtension(x.BusinessName, "Enity").ToLower()
                                    && y.DUNSNumber == x.DUNS));
                }
                else if (missingBNFuzzy.ToList().Where(x => !String.IsNullOrEmpty(x.GISID.ToString())).Count() > 0)
                {
                    missingBNFuzzy.RemoveAll(x => !!GISExtraResearch.Any(y => y.EntityWithoutLegalExt.ToLower() == DropLegalExtension(x.BusinessName, "Enity").ToLower()
                                    && y.GISID == x.GISID.ToString()));
                }
                else if (missingBNFuzzy.ToList().Where(x => !String.IsNullOrEmpty(x.Country)).Count() > 0)
                {
                    missingBNFuzzy.RemoveAll(x => !!GISExtraResearch.Any(y => y.EntityWithoutLegalExt.ToLower() == DropLegalExtension(x.BusinessName, "Enity").ToLower()
                                    && y.Country == x.Country));
                }
            }

            //missingBNFuzzy.RemoveAll(x => !!GISExtraResearch.Any(y => y.EntityName.ToLower() == x.BusinessName.ToLower()
            //               && y.Country == x.Country && y.GISID == x.GISID.ToString() && y.DUNSNumber == x.DUNS));

            //03/29/24 - Chat with Manisha - story will be raised. 1412727 - Goldman
            //        [3:13 PM] Tejal Patel
            //100 % match with Businessname OR Alias -but then when comparing both names they do not match so then we added Alias to research further
            //[3:13 PM] Tejal Patel
            //that's why you got 04.A0, 04.A02
            //[3:15 PM] Manisha Viswanathan
            //but if there is one UID match, then we don't need to look at 100% nam matches. and for Goldman there is already one UID match
            //[3:16 PM] Tejal Patel
            //oh..that part is not coded --may be that's why
            //[3:16 PM] Manisha Viswanathan
            //Also, if there are multiple 100 % name matches but with no UID match, then I don't think we would want AU to do any thing with those multiple
            //matches as we don't yet know which one is the correct match.That is why we flag it to humans saying 'Multiple close matches found'.
            //[3:17 PM] Tejal Patel
            //oh! that's a second rule then

            //[3:30 PM] Tejal Patel
            //what if I have 1 UID match(GIS ID match) and 1 100 % name match - will I add as additional research ? --Yes
            //[3:30 PM] Tejal Patel
            //what if I have 2 UID matches(GIS ID) and 1 100 % name match - Will I add as additional match ? --Not possible
            //[3:31 PM] Tejal Patel
            //what if I have 0 UID match(GIS ID match) and 1 100 % name match - will I add as additional research ? --No
            //[3:47 PM] Manisha Viswanathan
            //Tejal Patel
            //so if 0 UID match then no need?

            //For 0 UID matches, there can be 2 possibilities. 1) UID already provided by engagement team in PACE - in that case we can consider that
            //this is the correct UID and if we don't have any matches with that, we can ignore the 100% name matches. 2) if there is no UID provided by
            //the engagement team in PACE -  In that case, if there is a single 100% entity name match we can follow this. If there are multiple 100% name matches,
            //AU need not take any further action

            //[3:47 PM] Manisha Viswanathan
            //This is just my thought.Will need to discuss with Beth and maye Shashank as well.

            if (missingBNFuzzy.Count == 1)
                GISExtraResearch.AddRange(missingBNFuzzy.Select(x => new ResearchSummary()
                {
                    EntityName = x.BusinessName,
                    DUNSNumber = x.DUNS,
                    GISID = x.GISID.ToString(),
                    MDMID = x.MDMID.ToString(),
                    Country = x.Country,
                    WorksheetNo = rs.WorksheetNo,
                    //WorksheetNo = rs.IsAdditionalLoop ? rs.WorksheetNo : rs.WorksheetNo.Split('A')[0],
                    IsClientSide = rs.IsClientSide,
                    Type = rs.Type,
                    //    Role = "GIS" + "N/A - Additional Research Unit",
                    IsGISAdditionalBNFuzzy = true,
                    IsFinscanResearch = rs.IsFinscanResearch,
                    IsCERResearch = rs.IsCERResearch,
                    IsCRRResearch = rs.IsCRRResearch,
                    IsSPLResearch = rs.IsSPLResearch,
                    IsGISResearch = rs.IsGISResearch,
                    EntityWithoutLegalExt = DropLegalExtension(x.BusinessName, rs.Type),
                    Rework = rs.Rework,
                    PerformResearch = rs.PerformResearch
                }));
        }

        return new GISEmbeddFiles
        {
            EntityMatchCount = ReturnEntityMatchRecords.Count,
            EntityMatchRecords = ReturnEntityMatchRecords
        };

    }

    public SearchEntitiesResponseViewModel FindEntitiesFromGISWebAPIAsync(SearchEntitiesRequestViewModel requestViewModel, bool IsIndividual, bool passfuzzy)
    {
        try
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                if (_configuration.IsTestMode)
                {
                    handler.UseDefaultCredentials = false;
                    var credentials = new NetworkCredential(_configuration.TestModeUserName, _configuration.TestModePassword);
                    handler.Credentials = credentials;
                    handler.Proxy = GetProxy(_configuration.TestModeUserName, _configuration.TestModePassword);
                }
                else
                {
                    handler.UseDefaultCredentials = true;
                }

                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OpenCertPolicy);

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.BaseAddress = new Uri(Program.GISOrigin);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var searchCriteria = new EntitySearchCriteria(requestViewModel.Country);
                    searchCriteria.StandardSearchParameters.SearchTerm = requestViewModel.SearchTerm;
                    searchCriteria.StandardSearchParameters.Filters[0].DefaultValue = IsIndividual.ToString().ToLower();

                    if (passfuzzy)
                    {
                        searchCriteria.StandardSearchParameters.IncludeInactive = true;
                        searchCriteria.StandardSearchParameters.IsFuzzy = true;
                    }

                    //var res = JsonConvert.SerializeObject(searchCriteria);
                    var request = new StringContent(searchCriteria.ToJson(), Encoding.UTF8, "application/json");
                    try
                    {
                        var response = client.PostAsync(Program.GISEntitySearchAPIRelativeURL, request).GetAwaiter().GetResult();

                        if (response.IsSuccessStatusCode)
                        {
                            var jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            var data = jsonResponse.FromJson<SearchEntitiesResponseViewModel>();
                            return new SearchEntitiesResponseViewModel
                            {
                                Records = data.Records.ToList(),
                                TotalRecords = data.Records.Count
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerInfo.LogException(ex, "FindEntitiesFromGISWebAPIAsync Method");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred calling the GIS Entity Search api. Message: {ex}");
            LoggerInfo.LogException(ex);
        }

        return new SearchEntitiesResponseViewModel();
    }

    public SearchEntitiesResponseViewModel FindEntitiesFromGISWebAPIAsync_RESTClient(SearchEntitiesRequestViewModel requestViewModel)//ToDo. Dilip. Delete this method.
    {
        try
        {
            string s_baseURL = Program.GISOrigin;

            RestClientOptions clientOptions = new RestClientOptions(s_baseURL);

            if (_configuration.IsTestMode)
            {
                var credentials = new NetworkCredential(_configuration.TestModeUserName, _configuration.TestModePassword);
                clientOptions.UseDefaultCredentials = false;
                clientOptions.Credentials = credentials;
                clientOptions.Proxy = GetProxy(_configuration.TestModeUserName, _configuration.TestModePassword);
            }
            else
            {
                clientOptions.UseDefaultCredentials = true;
                clientOptions.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            }

            RestClient objClient = new RestClient(clientOptions);

            RestRequest request = new RestRequest(Program.GISEntitySearchAPIRelativeURL, Method.Post);

            var searchCriteria = new EntitySearchCriteria(requestViewModel.Country);
            searchCriteria.StandardSearchParameters.SearchTerm = requestViewModel.SearchTerm;
            Console.WriteLine($"Calling GIS entity search api start");

            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            var json = JsonConvert.SerializeObject(searchCriteria,
                   new JsonSerializerSettings()
                   {
                       DateFormatString = "yyyy-MM-ddTHH:mm:ss"
                   });

            request.RequestFormat = DataFormat.Json;

            request.AddParameter("application/json", json, ParameterType.RequestBody);

            Console.WriteLine($"Calling GIS API from {s_baseURL}");
            var response = objClient.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var jsonResponse = response.Content;
                var data = jsonResponse.FromJson<SearchEntitiesResponseViewModel>();

                return new SearchEntitiesResponseViewModel
                {
                    Records = data.Records.ToList(),
                    TotalRecords = data.Records.Count
                };

            }
            else
                Console.WriteLine($"Error in requesting GIS API from {s_baseURL}. Error: {response.ErrorMessage} {response.Content}  ");

        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred calling the GIS Entity Search api. Message: {ex}");
            LoggerInfo.LogException(ex);
        }

        return new SearchEntitiesResponseViewModel();
    }

    public static WebProxy GetProxy(string TestModeUserName, string TestModePassword)
    {
        string strUseProxy = "Y";
        string strProxyServer = "amweb.ey.net";//ToDo. Dilip. Move to config if this works
        int strProxyPort = 80;//  
        string strProxyUserName = TestModeUserName;
        string strProxyPassword = TestModePassword;

        //Added proxy add-on in case is needed.
        if (strUseProxy != "N")
        {
            WebProxy pry = new WebProxy(strProxyServer, strProxyPort);
            pry.Credentials = new NetworkCredential(strProxyUserName, strProxyPassword);
            return pry;
        }
        else
            return null;

    }

    public static bool OpenCertPolicy(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
    {
        return true; // allow all certificates
    }
    public void UpdateFileshare(string SPL = "")
    {
        try
        {
            _configuration.SPLLocalPath = Path.Combine(_configuration.destinationFilePath, Path.GetFileName(_configuration.SPLPath));
            _configuration.RegionalLocalPath = Path.Combine(_configuration.destinationFilePath, Path.GetFileName(_configuration.RegionalPath));
            _configuration.onshoreContactLocalPath = Path.Combine(_configuration.destinationFilePath, Path.GetFileName(_configuration.onshoreContact));
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, "UpdateFileshare Method");
        }
    }
    public static System.Data.DataTable GetDataTableFromExcel(string path, bool hasHeader = true)
    {
        using (var pck = new ExcelPackage())
        {
            using (var stream = File.OpenRead(path))
            {
                pck.Load(stream);
            }
            var ws = pck.Workbook.Worksheets.First();
            System.Data.DataTable tbl = new();
            foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
            {
                tbl.Columns.Add(hasHeader ? firstRowCell.Text : string.Format("Column {0}", firstRowCell.Start.Column));
            }
            var startRow = hasHeader ? 2 : 1;
            for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
            {
                var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                DataRow row = tbl.Rows.Add();
                foreach (var cell in wsRow)
                {
                    row[cell.Start.Column - 1] = cell.Text;
                }
            }
            return tbl;
        }
    }


    public List<ColumnLookup> GetAllColumnLookup()
    {
        List<ColumnLookup> lstcolumnLookup = new List<ColumnLookup>();
        string connString = _configuration.ConnectionString;
        DataSet columnLookup = EYSql.ExecuteDataset(connString, CommandType.Text, "select * from RPT_ColumnsLookUp where IsActive=1 and ReportID=" + 7 + " order by Category,ColumnPrettyName");
        foreach (DataRow drA in columnLookup.Tables[0].Rows)
        {
            ColumnLookup lookup = new ColumnLookup();
            lookup.LookUpID = (long)drA["LookUpID"];
            lookup.Category = drA["Category"].ToString();
            lookup.TableName = drA["TableName"].ToString();
            lookup.ColumnName = drA["ColumnName"].ToString();
            lookup.TableAlias = drA["TableAlias"].ToString();
            lookup.ColumnPrettyName = drA["ColumnPrettyName"].ToString();
            lookup.IsActive = drA["IsActive"] != DBNull.Value ? (bool)drA["IsActive"] : false;
            lookup.IsDefaultCategory = drA["IsDefaultCategory"] != DBNull.Value ? (bool)drA["IsDefaultCategory"] : false;
            lookup.DateFormat = drA["DateFormat"].ToString();
            lstcolumnLookup.Add(lookup);
        }
        return lstcolumnLookup;
    }
    public List<ParentalRelationship> GetParentalRelationship(string GISIDs)
    {
        List<ParentalRelationship> lstRet = new List<ParentalRelationship>();
        try
        {
            string SQL = @" DROP TABLE IF EXISTS #ParentGISRelationship
            select  IRA.ChildGisId, EN.EntityName as Entity,PRA.Name , 
            STRING_AGG ( CAST(RA.[Name] AS NVARCHAR(MAX)), ',' ) WITHIN GROUP (ORDER BY RA.SecondaryAttributeOrder)  AS SecondaryRelationshipAttribute, 
                    IRA.ParentGisId 
                INTO #ParentGISRelationship     
            from 
                 ItemRelationshipAttribute IRA 
                 join      RelationshipAttribute PRA on IRA.PrimaryRelationshipAttributeId = PRA.Id    
                 join      Item I on IRA.ParentGisId = I.GisId
                 left join Entity E on IRA.ParentGisId = E.GisId

                 left join EntityName EN on IRA.ParentGisId = EN.GisID and EN.NameTypeCode = 'LEGAL'
                 Left JOIN ItemRelationshipAttributeSecondary S on IRA.Id = S.ItemRelationshipAttributeId 
                 Left JOIN [dbo].[RelationshipAttribute] RA ON s.[SecondaryRelationshipAttributeId] = RA.[Id] 
             where IRA.ChildGisId IN ( " + GISIDs + ") and IRA.ChildGisId<> IRA.ParentGisId " +
                    "  GROUP BY IRA.ChildGisId, IRA.ParentGisId,EN.EntityName,PRA.Name,IRA.Id, EN.Increment, EN.NameTypeCode" +
                    "   order by IRA.Id, EN.Increment, EN.NameTypeCode " +

            "select Distinct A.*, B.DUNSNumber, B.MDMID, C.CountryDesc from #ParentGISRelationship A JOIN Entity B on A.ParentGisId = B.GisId " +
               "  LEFT JOIN Country C on B.CountryCode = C.CountryCode ";

            System.Data.DataTable dt = EYSql.ExecuteDataset(_configuration.GISConnectionString, CommandType.Text, SQL
                   ).Tables[0];

            Log.Information($"GetParentalRelationship GIS Result count {dt.Rows.Count}");
            foreach (DataRow dr in dt.Rows)
            {
                // KeyGenFactory keyGenFactory = new KeyGenFactory(_configuration.ConnectionString);
                KeyGenFactory keyGenFactory = Program.KeywordGeneratorFactory;

                Log.Information($"GetParentalRelationship GIS Result Entity:{dr.GetString("Entity")}, GIS ID {dr.GetLong("ChildGisId")}, DUNS: {dr.GetString("DUNSNumber")}, MDM ID: {dr.GetString("MDMID")}");
                lstRet.Add(new ParentalRelationship()
                {
                    BaseGISID = GISIDs.ToString(),
                    GISID = dr.GetLong("ChildGisId"),
                    Entity = dr.GetString("Entity"),
                    RelationshipName = dr.GetString("Name"),
                    SecondaryRelationshipAttribute = dr.GetString("SecondaryRelationshipAttribute"),
                    // RelationshipName = "Control", //dr.GetString("Name"),
                    // SecondaryRelationshipAttribute = "PE Investment", // dr.GetString("SecondaryRelationshipAttribute"),
                    ParentGISID = dr.GetLong("ParentGisId"),
                    ParentDUNSNumber = dr.GetString("DUNSNumber"),
                    ParentMDMID = dr.GetString("MDMID"),
                    Country = dr.GetString("CountryDesc"),
                    Type = keyGenFactory.GetSubjectType(
                                         dr.GetString("Entity"),
                                         dr.GetString("DUNSNumber"),
                                         dr.GetString("ChildGisId"),
                                         dr.GetString("CountryDesc")).ToString()
                });
            }

            lstRet.ForEach(dr => dr.EntityWithoutLegalExt = DropLegalExtension(dr.Entity, dr.Type));
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, "GetParentalRelationship Method ");
            Log.Error(ex, $"GetParentalRelationship Method GISIDs: {GISIDs}");
        }
        return lstRet;
    }

    public List<ParentalRelationship> GetParentalRelationship03A01G01(string GISIDs)
    {
        List<ParentalRelationship> lstRet = new List<ParentalRelationship>();
        try
        {
            string parentGISIDs = GISIDs;
            string iSQL = @"         declare @ChildGisId BIGINT 
                                 
                    declare @ParentGISID table (GRP NVARCHAR(10),ParentGISID BIGINT)
                    INSERT INTO  @ParentGISID 
                    select distinct 'GIS', IRA.ParentGisId from ItemRelationshipAttribute IRA WHERE IRA.ChildGisId IN ( " + GISIDs + ")     " +

                 " INSERT INTO  @ParentGISID select distinct 'GIS',IRA.ParentGisId from ItemRelationshipAttribute IRA JOIN @ParentGISID B ON IRA.ChildGisId = B.ParentGISID " +

                 " INSERT INTO  @ParentGISID select distinct 'GIS', IRA.ParentGisId from ItemRelationshipAttribute IRA JOIN @ParentGISID B ON IRA.ChildGisId = B.ParentGISID " +

                 "   select distinct GRP,  STRING_AGG(ParentGISID, ',') as GISID from  @ParentGISID group by GRP";

            System.Data.DataTable dtt = EYSql.ExecuteDataset(_configuration.GISConnectionString, CommandType.Text, iSQL).Tables[0];

            foreach (DataRow drr in dtt.Rows)
            {
                parentGISIDs = drr.GetString("GISID");
            }

            string SQL = @" DROP TABLE IF EXISTS #ParentGISRelationship
select  IRA.ChildGisId, EN.EntityName as Entity,PRA.Name , 
 STRING_AGG ( CAST(RA.[Name] AS NVARCHAR(MAX)), ',' ) WITHIN GROUP (ORDER BY RA.SecondaryAttributeOrder)  AS SecondaryRelationshipAttribute, 
         IRA.ParentGisId 
     INTO #ParentGISRelationship     
 from 
      ItemRelationshipAttribute IRA 
      join      RelationshipAttribute PRA on IRA.PrimaryRelationshipAttributeId = PRA.Id    
      join      Item I on IRA.ParentGisId = I.GisId
      left join Entity E on IRA.ParentGisId = E.GisId

      left join EntityName EN on IRA.ParentGisId = EN.GisID and EN.NameTypeCode = 'LEGAL'
      Left JOIN ItemRelationshipAttributeSecondary S on IRA.Id = S.ItemRelationshipAttributeId 
      Left JOIN [dbo].[RelationshipAttribute] RA ON s.[SecondaryRelationshipAttributeId] = RA.[Id] 
  where IRA.ChildGisId IN ( " + parentGISIDs + ") and IRA.ChildGisId<> IRA.ParentGisId " +
                     "  GROUP BY IRA.ChildGisId, IRA.ParentGisId,EN.EntityName,PRA.Name,IRA.Id, EN.Increment, EN.NameTypeCode" +
                     "   order by IRA.Id, EN.Increment, EN.NameTypeCode " +

            "select Distinct A.*, B.DUNSNumber, B.MDMID, C.CountryDesc from #ParentGISRelationship A JOIN Entity B on A.ParentGisId = B.GisId " +
                "  LEFT JOIN Country C on B.CountryCode = C.CountryCode ";

            System.Data.DataTable dt = EYSql.ExecuteDataset(_configuration.GISConnectionString, CommandType.Text, SQL
                   ).Tables[0];

            Log.Information($"GetParentalRelationship GIS Result count {dt.Rows.Count}");
            foreach (DataRow dr in dt.Rows)
            {
                // KeyGenFactory keyGenFactory = new KeyGenFactory(_configuration.ConnectionString);
                KeyGenFactory keyGenFactory = Program.KeywordGeneratorFactory;

                Log.Information($"GetParentalRelationship GIS Result Entity:{dr.GetString("Entity")}, GIS ID {dr.GetLong("ChildGisId")}, DUNS: {dr.GetString("DUNSNumber")}, MDM ID: {dr.GetString("MDMID")}");
                lstRet.Add(new ParentalRelationship()
                {
                    BaseGISID = GISIDs.ToString(),
                    GISID = dr.GetLong("ChildGisId"),
                    Entity = dr.GetString("Entity"),
                    //RelationshipName = dr.GetString("Name"),
                    //SecondaryRelationshipAttribute = dr.GetString("SecondaryRelationshipAttribute"),
                    RelationshipName = "Control", //dr.GetString("Name"),
                    SecondaryRelationshipAttribute = "PE Investment", // dr.GetString("SecondaryRelationshipAttribute"),
                    ParentGISID = dr.GetLong("ParentGisId"),
                    ParentDUNSNumber = dr.GetString("DUNSNumber"),
                    ParentMDMID = dr.GetString("MDMID"),
                    Country = dr.GetString("CountryDesc"),
                    Type = keyGenFactory.GetSubjectType(
                                         dr.GetString("Entity"),
                                         dr.GetString("DUNSNumber"),
                                         dr.GetString("ChildGisId"),
                                         dr.GetString("CountryDesc")).ToString()
                });
            }

            lstRet.ForEach(dr => dr.EntityWithoutLegalExt = DropLegalExtension(dr.Entity, dr.Type));
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, "GetParentalRelationship Method ");
            Log.Error(ex, $"GetParentalRelationship Method GISIDs: {GISIDs}");
        }
        return lstRet;
    }
    public List<UnitGrid_GISEntity> GetNewGISFields(long GISID)
    {
        List<UnitGrid_GISEntity> unit = new List<UnitGrid_GISEntity>();

        try
        {
            string SQL = @" --declare @GISID BIGINT
                  -- SET @GISID = 38190093

                  DROP TABLE IF EXISTS #CAU_GISInfo
                  CREATE TABLE #CAU_GISInfo(GISID BIGINT, MDMID NVARCHAR(100), GFISID NVARCHAR(100), MDMGFISID NVARCHAR(100), PIE BIT, PieAffiliate BIT, Subsidiaries  BIT, G360 BIT DEFAULT 0)

                 INSERT INTO #CAU_GISInfo(GISID, MDMID, GFISID)
                 SELECT GISID, ISNULL(MDMID, '0' ), ISNULL(GFISClientID,'0') FROM Entity WHERE GISID = @GISID

                 UPDATE #CAU_GISInfo
                 SET MDMGFISID = MDMID + ' / ' + GFISID
                 FROM #CAU_GISInfo WHERE MDMID <> '0' AND GFISID <> '0'

                 UPDATE #CAU_GISInfo
                 SET MDMGFISID = MDMID 
                 FROM #CAU_GISInfo WHERE MDMID <> '0' AND GFISID = '0'

                 UPDATE #CAU_GISInfo
                 SET MDMGFISID = GFISID 
                 FROM #CAU_GISInfo WHERE MDMID = '0' AND GFISID <> '0'

                  UPDATE A
                  SET A.PIE = ISNULL(B.PIE,0), A.PieAffiliate = ISNULL(B.PieAffiliate,0 )
                  FROM #CAU_GISInfo A JOIN RestrictionImpactAggregate B ON A.GISID = B.GisId

                  DROP TABLE IF EXISTS #Subsidiaries
                  SELECT  @GISID AS GISID, COUNT(*) as SubsidiariesCount INTO #Subsidiaries 
                  FROM ItemRelationshipAttribute 
                  WHERE ParentGisid =  @GISID
                  AND ChildGisId != ParentGisId
                  AND PrimaryRelationshipAttributeId NOT IN (15,17,18,33,37)

                  UPDATE A
                  SET A.Subsidiaries = CASE WHEN SubsidiariesCount > 0 THEN 1 ELSE 0 END
                  FROM #CAU_GISInfo A JOIN #Subsidiaries B ON A.GISID = B.GISID

                 DROP TABLE IF EXISTS #G360
            SELECT GISID, RestrictionId INTO #G360 FROM RestrictionImpact WHERE RestrictionId =43 AND GISID = @GISID

            UPDATE A
            SET A.G360 = 1
            FROM #CAU_GISInfo A JOIN #G360 B ON A.GISID = B.GisId
                    WHERE A.GISID = @GISID

                  SELECT * FROM #CAU_GISInfo ";

            // string SQL = "exec usp_PACE_GetCAUGISExact ";
            System.Data.DataTable dt = EYSql.ExecuteDataset(_configuration.GISConnectionString, CommandType.Text, SQL
                            , new SqlParameter("@GISID", GISID)).Tables[0];

            Log.Information($"GetNewGISFields GIS Result count: {dt.Rows.Count}");
            foreach (DataRow dr in dt.Rows)
            {
                Log.Information($"GetNewGISFields GIS Result GIS ID {dr.GetLong("GISID")}, PIE: {dr.GetBool("PIE")}, G360: {dr.GetBool("G360")}");
                unit.Add(new UnitGrid_GISEntity()
                {
                    GISID = dr.GetLong("GISID"),
                    MDMGFISID = dr.GetString("MDMGFISID"),
                    PIE = dr.GetBool("PIE"),
                    PIEAffiliate = dr.GetBool("PieAffiliate"),
                    Subsidiaries = dr.GetBool("Subsidiaries"),
                    G360 = dr.GetBool("G360")
                });
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, "GetNewGISFields Method ");
            Log.Error(ex, $"GetNewGISFields Method GISID: {GISID} ");
        }
        return unit;
    }

    public List<ResearchSummary> GetGISGUPInfo(string searchParam, string searchTerm, ResearchSummary r)
    {
        List<ResearchSummary> lstRet = new List<ResearchSummary>();
        try
        {
            string WhereClause = string.Empty;
            if (searchTerm == "GISID")
                WhereClause = string.Format(" WHERE A.ChildGisId = N'{0}' ", searchParam);
            else if (searchTerm == "DUNS")
                WhereClause = string.Format(" WHERE B.DUNSNumber = N'{0}' ", searchParam);
            else if (searchTerm == "MDM")
                WhereClause = string.Format(" WHERE B.MDMID = N'{0}' ", searchParam);

            string SQL = @" select distinct A.GupGisId, C.EntityName, D.CountryDesc, B.DUNSNumber, B.MDMID, B.CountryCode
                from ItemRelationshipAttribute A  JOIN Entity B on A.GupGisId = B.GisId 
                JOIN EntityName C on A.GupGisId= C.GisId AND NameTypeCode = 'LEGAL'
                LEFT JOIN  Country D on D.CountryCode = B.CountryCode
               " + WhereClause;

            System.Data.DataTable dt = EYSql.ExecuteDataset(_configuration.GISConnectionString, CommandType.Text, SQL).Tables[0];

            Log.Information($"GetGISGUPInfo GIS Result count: {dt.Rows.Count}");
            foreach (DataRow dr in dt.Rows)
            {

                Log.Information($"GetGISGUPInfo GISId: {dr.GetString("GupGisId")}, EntityName: {dr.GetString("EntityName")}, DUNS: {dr.GetString("DUNSNumber")}, CountryCode: {dr.GetString("CountryCode")}");
                lstRet.Add(new ResearchSummary()
                {
                    GISID = dr.GetString("GupGisId"),
                    EntityName = dr.GetString("EntityName"),
                    DUNSNumber = dr.GetString("DUNSNumber"),
                    MDMID = dr.GetString("MDMID"),
                    Country = dr.GetString("CountryDesc").Replace("United States", "United States of America"),
                    CountryCode = dr.GetString("CountryCode"),
                    Role = "GIS GUP of " + r.Role,
                    SourceSystem = "GIS",
                    WorksheetNo = r.WorksheetNo,
                    IsClientSide = r.IsClientSide,
                    IsCERResearch = r.IsCERResearch,
                    IsCRRResearch = r.IsCRRResearch,
                    IsFinscanResearch = r.IsFinscanResearch,
                    IsGISResearch = r.IsGISResearch,
                    IsSPLResearch = r.IsSPLResearch,
                    PerformResearch = r.PerformResearch,
                    Rework = r.Rework
                });
            }

            KeyGenFactory keyGenFactory = Program.KeywordGeneratorFactory;
            lstRet.ForEach(r => r.Type = keyGenFactory.GetSubjectType(
                                r.EntityName,
                                r.DUNSNumber,
                                r.GISID,
                                r.Country).ToString());

            lstRet.ForEach(x => x.EntityWithoutLegalExt = DropLegalExtension(x.EntityName, x.Type));
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, " GetGISGUPInfo");
            Log.Error(ex, $" GetGISGUPInfo searchParam:{searchParam}, searchTerm: {searchTerm}");
        }
        return lstRet;
    }
    public List<ResearchSummary> GetCERGUPInfo(string sDUNSNumber, ResearchSummary r)
    {
        List<ResearchSummary> lstRet = new List<ResearchSummary>();
        try
        {
            if (!r.IsClientSide && r.PerformResearch)
            {
                string SQL = @" DROP TABLE IF EXISTS #CERGUP
                    CREATE TABLE #CERGUP(DUNSNumber NVARCHAR(50), Client NVARCHAR(MAX), MDMID NVARCHAR(50), DUNSLocation NVARCHAR(MAX))
                    INSERT INTO #CERGUP(DUNSNumber, Client, MDMID)
                    select distinct B.DUNSNumber, B.Client, CASE WHEN Source <> 'GFIS' AND ClientID LIKE '1%' THEN ClientID ELSE '' END AS MDMID
                                                FROM vwCAU_Mercury_Client_Engagement B 
                                                WHERE B.DunsNumber = @DUNSNumber and B.AccountID = @DUNSNumber

                    DECLARE @count INT
                    SELECT @count = count(*) FROM  #CERGUP

                    IF(@count > 1)
                    DELETE FROM #CERGUP WHERE ISNULL(MDMID,'') = ''

                    UPDATE A
                    SET DUNSLocation  = B.DUNSLocation
                    FROM #CERGUP A JOIN vwCAU_Mercury_Client_Engagement B ON A.DUNSNumber = B.DunsNumber

                    SELECT * FROM #CERGUP";

                System.Data.DataTable dt = EYSql.ExecuteDataset(_configuration.ConnectionString, CommandType.Text, SQL
                      , new SqlParameter("@DUNSNumber", sDUNSNumber)).Tables[0];

                Log.Information($"GetCERGUPInfo CER Result count: {dt.Rows.Count}");
                foreach (DataRow dr in dt.Rows)
                {

                    Log.Information($"GetCERGUPInfo CER Result, DUNS: {dr.GetString("DUNSNumber")}, EntityName: {dr.GetString("Client")}, Country: {dr.GetString("DUNSLocation")}");
                    lstRet.Add(new ResearchSummary()
                    {
                        DUNSNumber = dr.GetString("DUNSNumber"),
                        EntityName = dr.GetString("Client"),
                        MDMID = dr.GetString("MDMID"),
                        Country = dr.GetString("DUNSLocation"),
                        Role = "CER GUP of " + r.Role,
                        SourceSystem = "CER",
                        WorksheetNo = r.WorksheetNo,
                        IsClientSide = r.IsClientSide,
                        IsCERResearch = r.IsCERResearch,
                        IsCRRResearch = r.IsCRRResearch,
                        IsFinscanResearch = r.IsFinscanResearch,
                        IsGISResearch = r.IsGISResearch,
                        IsSPLResearch = r.IsSPLResearch,
                        PerformResearch = r.PerformResearch,
                        Rework = r.Rework

                    });
                }

                KeyGenFactory keyGenFactory = Program.KeywordGeneratorFactory;
                lstRet.ForEach(r => r.Type = keyGenFactory.GetSubjectType(
                                    r.EntityName,
                                    r.DUNSNumber,
                                    r.GISID,
                                    r.Country).ToString());

                lstRet.ForEach(x => x.EntityWithoutLegalExt = DropLegalExtension(x.EntityName, x.Type));
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, "GetCERGUPInfo Method");
            Log.Error(ex, $"GetCERGUPInfo Method DUNS: {sDUNSNumber}");
        }
        return lstRet;
    }

    public List<ResearchSummary> GetGISMissingInfo(string searchParam, string searchTerm, ResearchSummary r)
    {
        List<ResearchSummary> lstRet = new List<ResearchSummary>();
        try
        {
            string WhereClause = string.Empty;
            if (searchTerm == "GISID")
                WhereClause = string.Format(" WHERE E.GisId = N'{0}' ", searchParam);
            else if (searchTerm == "DUNS")
                WhereClause = string.Format(" WHERE E.DUNSNumber = N'{0}'", searchParam);
            else if (searchTerm == "MDM")
                WhereClause = string.Format(" WHERE E.MDMID = N'{0}'", searchParam);

            string SQL = @" select distinct GisId, DUNSNumber, MDMID, C.CountryDesc, E.CountryCode from Entity E 
                    LEFT join Country C on E.CountryCode = C.CountryCode " + WhereClause;

            System.Data.DataTable dt = EYSql.ExecuteDataset(_configuration.GISConnectionString, CommandType.Text, SQL).Tables[0];

            Log.Information($"GetGISMissingInfo GIS Result count: {dt.Rows.Count}");
            foreach (DataRow dr in dt.Rows)
            {

                Log.Information($"GetGISMissingInfo GIS Id: {dr.GetString("GisId")}, DUNS: {dr.GetString("DUNSNumber")}, CountryCode: {dr.GetString("CountryCode")}");
                lstRet.Add(new ResearchSummary()
                {
                    GISID = dr.GetString("GisId"),
                    DUNSNumber = dr.GetString("DUNSNumber"),
                    MDMID = dr.GetString("MDMID"),
                    Country = dr.GetString("CountryDesc").Replace("United States", "United States of America"),
                    CountryCode = dr.GetString("CountryCode")
                });
            }

        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, " GetGISMissingInfo Method");
            Log.Error(ex, $" GetGISMissingInfo Method searchTerm:{searchTerm}, searchParam:{searchParam}");
        }
        return lstRet;
    }
    public List<ResearchSummary> GetCERMissingInfo(string searchParam, string searchTerm, ResearchSummary r)
    {
        List<ResearchSummary> lstRet = new List<ResearchSummary>();
        try
        {
            if (!r.IsClientSide && r.PerformResearch)
            {
                string WhereClause = string.Empty;

                if (searchTerm == "DUNS")
                    WhereClause = string.Format(" WHERE E.DUNSNumber = N'{0}'", searchParam);
                else if (searchTerm == "MDM")
                    WhereClause = string.Format(" WHERE E.ClientID = N'{0}'", searchParam);

                string SQL = @"  select distinct E.DUNSNumber, CASE WHEN Source <> 'GFIS' AND ClientID LIKE '1%' THEN ClientID ELSE '' END AS MDMID, DunsLocation
                             FROM vwCAU_Mercury_Client_Engagement E  " + WhereClause;

                System.Data.DataTable dt = EYSql.ExecuteDataset(_configuration.ConnectionString, CommandType.Text, SQL).Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    lstRet.Add(new ResearchSummary()
                    {
                        DUNSNumber = dr.GetString("DUNSNumber"),
                        MDMID = dr.GetString("MDMID"),
                        Country = dr.GetString("DunsLocation")
                    });
                }
            }

        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, " GetCERMissingInfo Method");
        }
        return lstRet;
    }
    public CRRResults GetCRRData(string keywords, string destinationPath, bool IsCRRGUP)
    {
        CRRResults cRRResults = new CRRResults();
        var searchFilter = IsCRRGUP ? "GUP" : "Entity";
        try
        {
            Directory.CreateDirectory(destinationPath);
            Log.Information($"GetCRRData Keywords: {keywords}, searchFilter: {searchFilter}");

            var crrEntityJson = GetConflictResearchData(0, true, keywords, searchFilter);
            ConflictResearchService CC = new ConflictResearchService(_configuration.configurationRoot, _env, _configuration.ConnectionString);
            try
            {
                cRRResults.crrDataset = CC.SaveCRReport(crrEntityJson, null, out string fileName, destinationPath);
                cRRResults.fileName = fileName;
                return cRRResults;
            }
            catch (Exception ex)
            {
                cRRResults.isError = true;
                LoggerInfo.LogException(ex);
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, "GetCRRData Method");
        }
        return cRRResults;
    }
    public JObject GetConflictResearchData(long RequestID, bool fetchMasterTemplate = false, string sSearchTerm = "", string searchFilter = "")
    {
        string sql = string.Empty;
        if (fetchMasterTemplate)
        {
            sql = "select top 1 * from CRR_MasterTemplate Order by RequestID desc";
        }
        else
        {
            sql = @"select * from RPT_Requests Where RequestID = " + RequestID;
        }

        DataSet ds = EYSql.ExecuteDataset(_configuration.ConnectionString, CommandType.Text, sql);

        if (ds.Tables[0].Rows.Count > 0)
        {

            var filter = JsonConvert.DeserializeObject<List<ConflictAdditionalFilter>>(ds.Tables[0].Rows[0]["FilterSelectionJSON"].ToString());
            var columns = JsonConvert.DeserializeObject<List<ColumnLookup>>(ds.Tables[0].Rows[0]["ColumnFilterJSON"].ToString());
            var commonFilter = ds.Tables[0].Rows[0]["Requiredsets"] == DBNull.Value ? JsonConvert.DeserializeObject<ConflictCommonFilter>(string.Empty) : JsonConvert.DeserializeObject<ConflictCommonFilter>(ds.Tables[0].Rows[0]["Requiredsets"].ToString());
            var request = JsonConvert.DeserializeObject<RPTRequests>(ds.Tables[0].Rows[0]["RequestJSON"].ToString());
            //var PrimarySearchTerm = fetchMasterTemplate ? string.Empty : ds.Tables[0].Rows[0]["FilterQuery"].ToString();
            //var PrimarySearchFilter = ds.Tables[0].Rows[0]["QuestionsFilterQuery"].ToString();
            var requestID = ds.Tables[0].Rows[0]["RequestID"].ToString();
            var _lookbackFilterOperator = filter.FirstOrDefault(f => f.FilterName == "CRR lookback period");
            var LookbackFilterOperator = _lookbackFilterOperator == null ? string.Empty : _lookbackFilterOperator.FilterOperator;

            //Filter out inactive columns name
            columns = GetActiveColumns(columns, (int)ReportName.ConflictResearchRedesign);
            var crrFilter = filter.FirstOrDefault(f => f.FilterName == "CRR lookback period");//Fix to Issue 932141: CRR report: lookback dates in saved reports
            if (crrFilter != null)
            {
                crrFilter.FilterValue2 = Convert.ToDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd'T'23:59:59.999'Z'"); //Set end date to today.
                crrFilter.FilterValue1 = Convert.ToDateTime(crrFilter.FilterValue2).AddYears(-5).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"); //Set beginning date -5 years.
            }

            if (fetchMasterTemplate)
            {
                if (sSearchTerm != "")
                {
                    var searchEntityFilter = filter.FirstOrDefault(f => f.FilterType == "Primary Filter");
                    if (searchEntityFilter != null)
                    {
                        searchEntityFilter.FilterValue1 = sSearchTerm;
                    }
                    commonFilter.TabsMode = "One tab";
                }
            }

            return JObject.FromObject(new
            {
                Result = filter,
                ColumnFilter = columns,
                CommonFilter = commonFilter,
                RPTRequest = request,
                PrimarySearchTerm = sSearchTerm,
                PrimarySearchFilter = searchFilter,
                RequestID = requestID,
                RunOption = "run",
                LookbackFilterOperator = LookbackFilterOperator
            });
        }
        else
            return null;
    }
    public List<ColumnLookup> GetActiveColumns(List<ColumnLookup> savedColumns, int ReportID)
    {
        var currentColumns = GetAllColumnLookup();

        var result = from s in savedColumns
                     join c in currentColumns
                     on s.ColumnName equals c.ColumnName
                     where c.IsActive = true
                     select s;

        return result.Distinct().ToList();
    }
    public SanctionsContactEntity GetSanctionsContact(string region, string country)
    {
        if (string.IsNullOrEmpty(country))
        {
            return new SanctionsContactEntity();
        }
        List<SanctionsContactEntity> listRegion = new List<SanctionsContactEntity>();
        string getTemplateQuery = $"select Id,Area,Region,Country,GCOContact,RMContact from CAU_SanctionsContact where Region like '%{region}%'";
        try
        {
            using (var reader = EYSql.ExecuteReader(_configuration.ConnectionString, CommandType.Text, getTemplateQuery, null))
            {
                while (reader.Read())
                {
                    listRegion.Add(new SanctionsContactEntity
                    {
                        Id = reader.GetInt32("Id"),
                        Area = reader.GetString("Area"),
                        Region = reader.GetString("Region"),
                        Country = reader.GetString("Country"),
                        GCOContact = reader.GetString("GCOContact"),
                        RMContact = reader.GetString("RMContact")
                    });
                }
            }
            var countryData = listRegion
                             .Where(i => i.Country.ToUpper() == country.ToUpper())
                             .FirstOrDefault();
            if (countryData == null)
            {
                countryData = listRegion
                             .Where(i => i.Country.ToUpper() == "ALL COUNTRIES")
                             .FirstOrDefault();
            }
            return countryData;

        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, "GetSanctionsContact Method");
        }
        return new SanctionsContactEntity();
    }
    public ConclusionConditionsEntity GetConclusionConditions(int cases)
    {
        ConclusionConditionsEntity conclusionConditions = new ConclusionConditionsEntity();
        string getTemplateQuery = $"select Id,Cases,Condition1, Condition2,ConflictConclusion,DisclaimerIndependence,RationaleInstructions from CAU_ConclusionConditions where Cases ={cases}";
        try
        {
            using (var reader = EYSql.ExecuteReader(_configuration.ConnectionString, CommandType.Text, getTemplateQuery, null))
            {
                while (reader.Read())
                {
                    conclusionConditions.Id = reader.GetInt32("Id");
                    conclusionConditions.Cases = reader.GetInt32("Cases");
                    conclusionConditions.Condition1 = reader.GetString("Condition1");
                    conclusionConditions.Condition1 = reader.GetString("Condition1");
                    conclusionConditions.ConflictConclusion = reader.GetString("ConflictConclusion");
                    conclusionConditions.DisclaimerIndependence = reader.GetString("DisclaimerIndependence");
                    conclusionConditions.RationaleInstructions = reader.GetString("RationaleInstructions");
                }
            }
            return conclusionConditions;
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex);
        }
        return new ConclusionConditionsEntity();
    }

    public void GetGUP(List<ResearchSummary> rs, CheckerQueue queue, string searchSystem)
    {
        List<ResearchSummary> GISGUP = new List<ResearchSummary>();
        List<ResearchSummary> CERGUP = new List<ResearchSummary>();

        if (searchSystem == "GIS")
        {
            foreach (ResearchSummary r in rs)
            {
                if (r.PerformResearch)
                {
                    List<ResearchSummary> GISGUPTemp = new List<ResearchSummary>();

                    if (!String.IsNullOrEmpty(r.GISID))
                        GISGUPTemp.AddRange(GetGISGUPInfo(r.GISID, "GISID", r));
                    else if (!String.IsNullOrEmpty(r.DUNSNumber))
                        GISGUPTemp.AddRange(GetGISGUPInfo(r.DUNSNumber, "DUNS", r));
                    else if (!String.IsNullOrEmpty(r.MDMID))
                        GISGUPTemp.AddRange(GetGISGUPInfo(r.MDMID, "MDM", r));

                    RemoveDupes(GISGUP, GISGUPTemp, "GIS");
                    //if(GISGUPTemp.Where(i=> !i.EntityName.Equals(r.EntityName, StringComparison.OrdinalIgnoreCase)).Any())
                    //    GISGUP.AddRange(GISGUPTemp); Bug 1022054
                    bool shouldAddGUP = !GISGUPTemp.Any(g => rs.Any(r => r.EntityName.Equals(g.EntityName, StringComparison.OrdinalIgnoreCase)));
                    if (shouldAddGUP)
                    {
                        GISGUP.AddRange(GISGUPTemp);
                    }
                }

            }

            if (GISGUP != null)
            {
                // List<ResearchSummary> matchedGISGUPWithRS = IdentifyDupes(queue.researchSummary, GISGUP, "IgnoreCountry");
                List<ResearchSummary> matchedGISGUPWithRS = IdentifyDupes(queue.researchSummary, GISGUP, "GIS");

                List<string> matchedGISGUPWithRSEntity = matchedGISGUPWithRS.Select(x => x.EntityName).ToList();

                //  List<string> matchedGISGUPWithRSEntity = IdentifyDupes1(queue.researchSummary, GISGUP);

                foreach (var ge in queue.researchSummary)
                {
                    if (matchedGISGUPWithRSEntity.ToList().Contains(ge.EntityName))
                    {
                        if (ge.SourceSystem == "PACE APG" && !ge.SourceSystem.Contains("GIS"))
                            ge.SourceSystem += ", GIS";
                        //if (string.IsNullOrEmpty(ge.DUNSNumber))
                        //    ge.DUNSNumber = matchedGISGUPWithRS.Where(x => x.EntityWithoutLegalExt.Equals(ge.EntityWithoutLegalExt))
                        //    .Select(x => x.DUNSNumber).ToString();
                        //if (string.IsNullOrEmpty(ge.GISID))
                        //    ge.GISID = matchedGISGUPWithRS.Where(x => x.EntityWithoutLegalExt.Equals(ge.EntityWithoutLegalExt))
                        //    .Select(x => x.GISID).ToString();
                        //if (string.IsNullOrEmpty(ge.MDMID))
                        //    ge.MDMID = matchedGISGUPWithRS.Where(x => x.EntityWithoutLegalExt.Equals(ge.EntityWithoutLegalExt))
                        //    .Select(x => x.MDMID).ToString();
                    }
                }

                //   RemoveDupes(queue.researchSummary, GISGUP, "IgnoreCountry");

                RemoveDupes(queue.researchSummary, GISGUP, "GIS");
                RemoveDupes(rs, GISGUP, "GIS");

                //GISGUP.RemoveAll(x => !!queue.researchSummary.Any(y => y.EntityName.ToLower() == x.EntityName.ToLower() && y.GISID == x.GISID && y.DUNSNumber == x.DUNSNumber));

                //GISGUP.RemoveAll(x => !!rs.Any(y => y.EntityName.ToLower() == x.EntityName.ToLower() && y.GISID == x.GISID && y.DUNSNumber == x.DUNSNumber));
            }

            rs.AddRange(GISGUP);
        }
        //CER GUP fill up
        if (searchSystem == "CER")
        {
            foreach (ResearchSummary r in rs)
            {
                if (r.PerformResearch)
                {
                    if (!String.IsNullOrEmpty(r.DUNSNumber))
                    {
                        var tempCERGUP = GetCERGUPInfo(r.DUNSNumber, r);
                        if (tempCERGUP.Count > 1)
                        {
                            if (r.ClientRelationshipSummary == null)
                            {
                                r.ClientRelationshipSummary = new();
                            }
                            r.ClientRelationshipSummary.CERDesc = "MULTIPLE GUPS HAVE BEEN IDENTIFIED IN CER FOR WHICH AU HAS TAKEN NO ACTION. PLEASE ACTION ACCORDINGLY.";
                        }
                        else
                            CERGUP.AddRange(GetCERGUPInfo(r.DUNSNumber, r));
                    }
                }
            }

            if (CERGUP != null)
            {
                List<ResearchSummary> matchedCEREntitiesRS = IdentifyDupes(queue.researchSummary, CERGUP);

                List<string> matchedCEREntities = matchedCEREntitiesRS.Select(x => x.EntityName).ToList();

                //(from c in CERGUP where !!queue.researchSummary.Any(y => y.EntityName.ToLower() == c.EntityName.ToLower() && y.DUNSNumber == c.DUNSNumber) select c.EntityName);

                foreach (var r in queue.researchSummary)
                {
                    if (matchedCEREntities.Contains(r.EntityName))
                    {
                        if (r.SourceSystem == "GIS" && !r.SourceSystem.Contains("CER"))
                            r.SourceSystem = "GIS, CER";
                    }
                }

                RemoveDupes(queue.researchSummary, CERGUP);

                RemoveDupes(rs, CERGUP);

                //CERGUP.RemoveAll(x => !!queue.researchSummary.Any(y => y.EntityName.ToLower() == x.EntityName.ToLower() && y.DUNSNumber == x.DUNSNumber));

                //GISGUP.RemoveAll(x => !!rs.Any(y => y.EntityName.ToLower() == x.EntityName.ToLower() && y.DUNSNumber == x.DUNSNumber));
            }

            rs.AddRange(CERGUP);
        }

        int iWorksheet = 5;
        int rowCER = 01;
        string wsCER = string.Empty;

        int rowGIS = 01;
        string wsGIS = string.Empty;

        foreach (ResearchSummary r in rs.OrderBy(i => i.WorksheetNo))
        {
            if (r.Role.Contains("CER GUP"))
            {
                ++iWorksheet;
                //  ++row;

                r.BotUnitRowNo = iWorksheet;
                if (r.Role.Contains("CER GUP"))
                {
                    if (r.WorksheetNo == wsCER)
                        rowCER++;
                    else
                        rowCER = 01;
                    wsCER = r.WorksheetNo;

                    //    r.WorksheetNo = r.WorksheetNo.Replace(".P01", "").Replace(".C01", "") + ".C" + string.Format("{0:00}", rowCER);
                    if (!r.WorksheetNo.EndsWith(".C" + string.Format("{0:00}", rowCER)))
                        r.WorksheetNo = r.WorksheetNo + ".C" + string.Format("{0:00}", rowCER);

                    r.TurnFlagsOffWhenNoResearch();
                    if (r.Role.Contains("GUP of Main Client"))
                        r.IsFinscanResearch = true;
                }
            }
            if (r.Role.StartsWith("GIS GUP"))
            {
                ++iWorksheet;

                r.BotUnitRowNo = iWorksheet;
                if (r.Role.Contains("GIS GUP"))
                {
                    if (r.WorksheetNo == wsGIS)
                        rowGIS++;
                    else
                        rowGIS = 01;
                    wsGIS = r.WorksheetNo;

                    //    r.WorksheetNo = r.WorksheetNo.Replace(".P01", "").Replace(".G01", "") + ".G" + string.Format("{0:00}", rowGIS);
                    if (!r.WorksheetNo.EndsWith(".G" + string.Format("{0:00}", rowGIS)))
                        r.WorksheetNo = r.WorksheetNo + ".G" + string.Format("{0:00}", rowGIS);

                    if (((r.Role.Contains("Consolidated") || r.Role.Contains("Control")) && r.IsClientSide) || r.IsGISAdditionalBNFuzzy)
                    {
                        r.IsCERResearch = false;
                        r.IsCRRResearch = false;
                    }

                    if (r.Role.Contains("Significant"))
                    {
                        r.IsCERResearch = false;
                        r.IsCRRResearch = false;
                        r.IsGISResearch = false;
                        r.IsFinscanResearch = false;
                    }

                    r.TurnFlagsOffWhenNoResearch();
                    if (r.Role.Contains("GUP of Main Client"))
                        r.IsFinscanResearch = true;

                    // Carlos: Code block needed for Issues #1022641 and #1022538
                    if ((!r.Role.Contains("Significant")) && (!queue.IsPursuit()))
                    {
                        r.IsFinscanResearch = true;
                    }
                    // -------------------------------------------
                }
            }
        }
    }
    public void RemoveDupes(List<ResearchSummary> MainGrid, List<ResearchSummary> NewGrid, string source = "")
    {
        try
        {
            if (NewGrid.Count > 0)
            {
                if (NewGrid.ToList().Where(x => !String.IsNullOrEmpty(x.DUNSNumber)).Count() > 0)
                {
                    NewGrid.RemoveAll(x => !!MainGrid.Any(y => y.EntityWithoutLegalExt.ToLower() == x.EntityWithoutLegalExt.ToLower()
                                    && y.DUNSNumber == x.DUNSNumber && !string.IsNullOrEmpty(y.DUNSNumber) && !string.IsNullOrEmpty(x.DUNSNumber)));
                }
                else if (NewGrid.ToList().Where(x => !String.IsNullOrEmpty(x.GISID)).Count() > 0)
                {
                    NewGrid.RemoveAll(x => !!MainGrid.Any(y => y.EntityWithoutLegalExt.ToLower() == x.EntityWithoutLegalExt.ToLower()
                                    && y.GISID == x.GISID && !string.IsNullOrEmpty(y.GISID) && !string.IsNullOrEmpty(x.GISID)));
                }
                else if (NewGrid.ToList().Where(x => !String.IsNullOrEmpty(x.MDMID)).Count() > 0)
                {
                    NewGrid.RemoveAll(x => !!MainGrid.Any(y => y.EntityWithoutLegalExt.ToLower() == x.EntityWithoutLegalExt.ToLower()
                                    && y.MDMID == x.MDMID && !string.IsNullOrEmpty(y.MDMID) && !string.IsNullOrEmpty(x.MDMID)));
                }
                else if (NewGrid.ToList().Where(x => !String.IsNullOrEmpty(x.Country)).Count() > 0)
                {
                    if (source == "GIS")
                        NewGrid.RemoveAll(x => !!MainGrid.Any(y => y.EntityWithoutLegalExt.ToLower() == x.EntityWithoutLegalExt.ToLower()
                                    && y.CountryCode.Equals(x.CountryCode) && !string.IsNullOrEmpty(y.CountryCode) && !string.IsNullOrEmpty(x.CountryCode)));
                    else if (source == "IgnoreCountry")
                        NewGrid.RemoveAll(x => !!MainGrid.Any(y => y.EntityWithoutLegalExt.ToLower() == x.EntityWithoutLegalExt.ToLower()
                                    && !string.IsNullOrEmpty(x.CountryCode)));
                    else
                        NewGrid.RemoveAll(x => !!MainGrid.Any(y => y.EntityWithoutLegalExt.ToLower() == x.EntityWithoutLegalExt.ToLower()
                                    && y.Country.Equals(x.Country) && !string.IsNullOrEmpty(y.Country) && !string.IsNullOrEmpty(x.Country)));
                }
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, "RemoveDupes");
        }
    }
    public List<ResearchSummary> IdentifyDupes(List<ResearchSummary> MainGrid, List<ResearchSummary> NewGrid, string source = "")
    {
        List<ResearchSummary> matchedEntities = new List<ResearchSummary>();
        try
        {
            if (NewGrid.Count > 0)
            {
                if (NewGrid.ToList().Where(x => !String.IsNullOrEmpty(x.DUNSNumber)).Count() > 0)// && MainGrid.ToList().Where(m => !String.IsNullOrEmpty(m.DUNSNumber)).Count() > 0)
                {
                    matchedEntities.AddRange(from x in NewGrid
                                             where !!MainGrid.Any(y => y.EntityWithoutLegalExt.ToLower() == x.EntityWithoutLegalExt.ToLower()
                                       && y.DUNSNumber == x.DUNSNumber && !String.IsNullOrEmpty(y.DUNSNumber) && !String.IsNullOrEmpty(x.DUNSNumber))
                                             select x);
                }
                else if (NewGrid.ToList().Where(x => !String.IsNullOrEmpty(x.GISID)).Count() > 0) // && MainGrid.ToList().Where(m => !String.IsNullOrEmpty(m.GISID)).Count() > 0)
                {
                    matchedEntities.AddRange(from x in NewGrid
                                             where !!MainGrid.Any(y => y.EntityWithoutLegalExt.ToLower() == x.EntityWithoutLegalExt.ToLower()
                                          && y.GISID == x.GISID && !String.IsNullOrEmpty(y.GISID) && !String.IsNullOrEmpty(x.GISID))
                                             select x);
                }
                else if (NewGrid.ToList().Where(x => !String.IsNullOrEmpty(x.MDMID)).Count() > 0) // && MainGrid.ToList().Where(m => !String.IsNullOrEmpty(m.MDMID)).Count() > 0)
                {
                    matchedEntities.AddRange(from x in NewGrid
                                             where !!MainGrid.Any(y => y.EntityWithoutLegalExt.ToLower() == x.EntityWithoutLegalExt.ToLower()
                                          && y.MDMID == x.MDMID && !String.IsNullOrEmpty(y.MDMID) && !String.IsNullOrEmpty(x.MDMID))
                                             select x);
                }
                else if (NewGrid.ToList().Where(x => !String.IsNullOrEmpty(x.Country)).Count() > 0) // && MainGrid.ToList().Where(m => !String.IsNullOrEmpty(m.Country)).Count() > 0)
                {
                    if (source == "GIS")
                        matchedEntities.AddRange(from x in NewGrid
                                                 where !!MainGrid.Any(y => y.EntityWithoutLegalExt.ToLower() == x.EntityWithoutLegalExt.ToLower()
                                              && y.CountryCode.Equals(x.CountryCode) && !String.IsNullOrEmpty(y.CountryCode) && !String.IsNullOrEmpty(x.CountryCode))
                                                 select x);
                    else if (source == "IgnoreCountry")
                        matchedEntities.AddRange(from x in NewGrid
                                                 where !!MainGrid.Any(y => y.EntityWithoutLegalExt.ToLower() == x.EntityWithoutLegalExt.ToLower()
                                                   && !String.IsNullOrEmpty(x.CountryCode))
                                                 select x);
                    else
                        matchedEntities.AddRange(from x in NewGrid
                                                 where !!MainGrid.Any(y => y.EntityWithoutLegalExt.ToLower() == x.EntityWithoutLegalExt.ToLower()
                                              && y.Country.Equals(x.Country) && !String.IsNullOrEmpty(y.Country) && !String.IsNullOrEmpty(x.Country))
                                                 select x);
                }
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, "IdentifyDupes");
        }

        return matchedEntities;

    }
    public void RemoveAPGGUPDupes(List<ResearchSummary> MainGrid, List<ResearchSummary> NewGrid)
    {
        try
        {
            if (NewGrid.Count > 0)
            {
                if (NewGrid.ToList().Where(x => !String.IsNullOrEmpty(x.DUNSNumber)).Count() > 0)
                {
                    NewGrid.RemoveAll(x => !!MainGrid.Any(y => y.EntityName.ToLower() == x.EntityName.ToLower()
                        && y.DUNSNumber == x.DUNSNumber && !string.IsNullOrEmpty(y.DUNSNumber) && !string.IsNullOrEmpty(x.DUNSNumber)
                        && y.Role == x.Role));
                }
                else if (NewGrid.ToList().Where(x => !String.IsNullOrEmpty(x.GISID)).Count() > 0)
                {
                    NewGrid.RemoveAll(x => !!MainGrid.Any(y => y.EntityName.ToLower() == x.EntityName.ToLower()
                                    && y.GISID == x.GISID && !string.IsNullOrEmpty(y.GISID) && !string.IsNullOrEmpty(x.GISID)
                                    && y.Role == x.Role));
                }
                else if (NewGrid.ToList().Where(x => !String.IsNullOrEmpty(x.MDMID)).Count() > 0)
                {
                    NewGrid.RemoveAll(x => !!MainGrid.Any(y => y.EntityName.ToLower() == x.EntityName.ToLower()
                                    && y.MDMID == x.MDMID && !string.IsNullOrEmpty(y.MDMID) && !string.IsNullOrEmpty(x.MDMID)
                                    && y.Role == x.Role));
                }
                else if (NewGrid.ToList().Where(x => !String.IsNullOrEmpty(x.CountryCode)).Count() > 0)
                {
                    NewGrid.RemoveAll(x => !!MainGrid.Any(y => y.EntityName.ToLower() == x.EntityName.ToLower()
                         && y.CountryCode.Equals(x.CountryCode) && !string.IsNullOrEmpty(y.CountryCode) && !string.IsNullOrEmpty(x.CountryCode)
                         && y.Role.Equals(x.Role)));
                }
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, "RemoveAPGGUPDupes");
        }
    }

    public List<ResearchSummary> DistinctEntity(List<ResearchSummary> MainGrid)
    {
        // Group by DUNSNumber and EntityName, taking the first record from each group and concatenating Roles
        var distinctByDuns = MainGrid
            .Where(r => !string.IsNullOrEmpty(r.DUNSNumber))
            .GroupBy(r => new { r.DUNSNumber, EntityWithoutLegalExt = r.EntityWithoutLegalExt.ToLowerInvariant() })
            .Where(g => g.Count() > 1)  // Only select groups with more than one element
            .Select(g =>
            {
                var first = g.First();
                first.Role = string.Join(",", g.Select(x => x.Role).Distinct());
                first.PerformResearch = g.Any(x => x.PerformResearch);
                first.IsClientSide = g.All(x => x.IsClientSide);
                first.AdditionalComments = string.Join(", ", g.Select(x => x.AdditionalComments).Where(comment => !string.IsNullOrEmpty(comment)).Distinct());
                return first;
            })
            .ToList();

        var dunsList = MainGrid
            .Where(r => !string.IsNullOrEmpty(r.DUNSNumber))
            .GroupBy(r => new { r.DUNSNumber, EntityWithoutLegalExt = r.EntityWithoutLegalExt.ToLowerInvariant() })
            .Where(g => g.Count() > 1)
            .SelectMany(g => g)
            .ToList();

        // Exclude records already grouped by DUNSNumber and EntityName
        var remaining = MainGrid
            .Except(dunsList)
            .ToList();

        // Group by GISID and EntityName, taking the first record from each group and concatenating Roles
        var distinctByGis = remaining
            .Where(r => !string.IsNullOrEmpty(r.GISID))
            .GroupBy(r => new { r.GISID, EntityWithoutLegalExt = r.EntityWithoutLegalExt.ToLowerInvariant() })
            .Where(g => g.Count() > 1)
            .Select(g =>
            {
                var first = g.First();
                first.Role = string.Join(",", g.Select(x => x.Role).Distinct());
                first.PerformResearch = g.Any(x => x.PerformResearch);
                first.IsClientSide = g.All(x => x.IsClientSide);
                first.AdditionalComments = string.Join(", ", g.Select(x => x.AdditionalComments).Where(comment => !string.IsNullOrEmpty(comment)).Distinct());
                return first;
            })
            .ToList();

        var gisList = remaining
            .Where(r => !string.IsNullOrEmpty(r.GISID))
            .GroupBy(r => new { r.GISID, EntityWithoutLegalExt = r.EntityWithoutLegalExt.ToLowerInvariant() })
            .Where(g => g.Count() > 1)
            .SelectMany(g => g)
            .ToList();


        // Exclude records already grouped by GISID and EntityName
        remaining = remaining
            .Except(gisList)
            .ToList();

        // Group by MDMID and EntityName, taking the first record from each group and concatenating Roles
        var distinctByMdm = remaining
            .Where(r => !string.IsNullOrEmpty(r.MDMID))
            .GroupBy(r => new { r.MDMID, EntityWithoutLegalExt = r.EntityWithoutLegalExt.ToLowerInvariant() })
            .Where(g => g.Count() > 1)
            .Select(g =>
            {
                var first = g.First();
                first.Role = string.Join(",", g.Select(x => x.Role).Distinct());
                first.AdditionalComments = string.Join(", ", g.Select(x => x.AdditionalComments).Where(comment => !string.IsNullOrEmpty(comment)).Distinct());
                first.PerformResearch = g.Any(x => x.PerformResearch);
                first.IsClientSide = g.All(x => x.IsClientSide);
                return first;
            })
            .ToList();
        var mdmList = remaining
            .Where(r => !string.IsNullOrEmpty(r.MDMID))
            .GroupBy(r => new { r.MDMID, EntityWithoutLegalExt = r.EntityWithoutLegalExt.ToLowerInvariant() })
            .Where(g => g.Count() > 1)
            .SelectMany(g => g)
            .ToList();
        // Exclude records already grouped by MDMID and EntityName
        remaining = remaining
            .Except(mdmList)
            .ToList();

        // Group by EntityName and Country, taking the first record from each group and concatenating Roles
        var distinctByEntityCountry = remaining
            .Where(r => !string.IsNullOrEmpty(r.Country) && string.IsNullOrEmpty(r.DUNSNumber) && string.IsNullOrEmpty(r.GISID) && string.IsNullOrEmpty(r.MDMID))
            .GroupBy(r => new { r.Country, EntityWithoutLegalExt = r.EntityWithoutLegalExt.ToLowerInvariant() })
            .Where(g => g.Count() > 1)
            .Select(g =>
            {
                var first = g.First();
                first.Role = string.Join(",", g.Select(x => x.Role).Distinct());
                first.PerformResearch = g.Any(x => x.PerformResearch);
                first.IsClientSide = g.All(x => x.IsClientSide);
                first.AdditionalComments = string.Join(", ", g.Select(x => x.AdditionalComments).Where(comment => !string.IsNullOrEmpty(comment)).Distinct());
                return first;
            })
            .ToList();
        var entityCountryList = remaining
        .Where(r => !string.IsNullOrEmpty(r.Country) && string.IsNullOrEmpty(r.DUNSNumber) && string.IsNullOrEmpty(r.GISID) && string.IsNullOrEmpty(r.MDMID))
        .GroupBy(r => new { r.Country, EntityWithoutLegalExt = r.EntityWithoutLegalExt.ToLowerInvariant() })
        .Where(g => g.Count() > 1)
        .SelectMany(g => g)
        .ToList();

        // Exclude records already grouped by Country and EntityName
        remaining = remaining
            .Except(entityCountryList)
            .ToList();

        // Combine all the records
        var distinctEntities = distinctByDuns
            .Concat(distinctByGis)
            .Concat(distinctByMdm)
            .Concat(distinctByEntityCountry)
            .Concat(remaining).
            OrderBy(i => i.OrderNo).ToList();

        return distinctEntities;
    }
    /// <summary>
    /// Where there is no DUNS ID, GIS ID or MDM ID available to match, whether the entity name (excluding legal extensions & special characters) AND location of the unit that is to be added matches the entity name AND location of another unit already in the Unit Grid. If there is a match on entity name AND location, AU will not add the unit. 
    /// </summary>
    /// <param name="MainGrid"></param>
    /// <returns></returns>
    public List<ResearchSummary> DistinctEntityForEmptyUIDs(List<ResearchSummary> MainGrid)
    {
        var mergedSummaries = new List<ResearchSummary>();
        var groupedByEntityName = MainGrid
                                  .ToLookup(r => new EntityCountryKey { EntityWithoutLegalExt = r.EntityWithoutLegalExt.ToLowerInvariant(), Country = r.Country },
                                              new EntityCountryKeyEqualityComparer())
                                  .Select(group => group.ToList())
                                  .ToList();

        foreach (var group in groupedByEntityName)
        {
            var nonEmptyIdentifiers = group
                .Where(r => !string.IsNullOrEmpty(r.GISID) || !string.IsNullOrEmpty(r.MDMID) || !string.IsNullOrEmpty(r.DUNSNumber))
                .ToList();
            var emptyIdentifiers = group
                .Where(r => string.IsNullOrEmpty(r.GISID) && string.IsNullOrEmpty(r.MDMID) && string.IsNullOrEmpty(r.DUNSNumber))
                .ToList();

            if (nonEmptyIdentifiers.Any())
            {
                // Merge roles from emptyIdentifiers into the first nonEmptyIdentifiers record
                var firstNonEmpty = nonEmptyIdentifiers.First();
                var roles = new HashSet<string>(firstNonEmpty.Role.Split(new[] { "," }, StringSplitOptions.None));
                var performResearch = firstNonEmpty.PerformResearch;
                var clientSide = firstNonEmpty.IsClientSide;
                var additionalComments = new HashSet<string>(firstNonEmpty.AdditionalComments.Split(new[] { ", " }, StringSplitOptions.None));
                foreach (var empty in emptyIdentifiers)
                {
                    var emptyRoles = empty.Role.Split(new[] { "," }, StringSplitOptions.None);
                    foreach (var role in emptyRoles)
                    {
                        if (!roles.Any(r => r.Contains(role)))
                        {
                            roles.Add(role);
                        }
                    }
                    var emptyadditionalComments = empty.AdditionalComments.Split(new[] { ", " }, StringSplitOptions.None);
                    foreach (var comments in emptyadditionalComments)
                    {
                        if (!additionalComments.Any(r => r.Contains(comments)))
                        {
                            additionalComments.Add(comments);
                        }
                    }
                    performResearch |= empty.PerformResearch;
                    clientSide &= empty.IsClientSide; // If any ClientSide is false, this will be false
                }
                firstNonEmpty.Role = string.Join(",", roles);
                firstNonEmpty.PerformResearch = performResearch;
                firstNonEmpty.IsClientSide = clientSide;
                firstNonEmpty.AdditionalComments = string.Join(", ", additionalComments.Where(comment => !string.IsNullOrEmpty(comment)));
                // Add the merged first non-empty record
                mergedSummaries.Add(firstNonEmpty);

                // Add the remaining non-empty records
                mergedSummaries.AddRange(nonEmptyIdentifiers.Skip(1));
            }
            else if (emptyIdentifiers.Any())
            {
                // Merge emptyIdentifiers as previously done
                var firstEmpty = emptyIdentifiers.First();
                var roles = emptyIdentifiers.Select(r => r.Role).Distinct();
                var additionalComments = emptyIdentifiers
                    .Select(r => r.AdditionalComments)
                    .Where(comment => !string.IsNullOrEmpty(comment))
                    .Distinct();
                firstEmpty.Role = string.Join(",", roles);
                firstEmpty.AdditionalComments = string.Join(", ", additionalComments);
                mergedSummaries.Add(firstEmpty);
            }
        }
        MainGrid = mergedSummaries.OrderBy(i => i.OrderNo).ToList();
        return MainGrid;
    }

    /// <summary>
    /// Adding this scenario for Location empty case. 
    /// </summary>
    /// <param name="MainGrid"></param>
    /// <returns></returns>
    public List<ResearchSummary> DistinctEntityForEmptyUIDsAndLocation(List<ResearchSummary> MainGrid)
    {
        var mergedSummaries = new List<ResearchSummary>();
        var groupedByEntityName = MainGrid
                                  .GroupBy(r => new EntityCountryKey { EntityWithoutLegalExt = r.EntityWithoutLegalExt.ToLowerInvariant() },
                                           new EntityCountryKeyEqualityComparer())
                                  .Select(group => group.ToList())
                                  .ToList();

        foreach (var group in groupedByEntityName)
        {
            var emptyIdentifiersWithoutUIDs = group
                .Where(r => !string.IsNullOrEmpty(r.Country) && string.IsNullOrEmpty(r.GISID) && string.IsNullOrEmpty(r.MDMID) && string.IsNullOrEmpty(r.DUNSNumber))
                .ToList();
            var emptyIdentifiersNoCountry = group
                .Where(r => string.IsNullOrEmpty(r.GISID) && string.IsNullOrEmpty(r.MDMID) && string.IsNullOrEmpty(r.DUNSNumber) && string.IsNullOrEmpty(r.Country))
                .ToList();
            var nonEmptyIdentifiersWithUIDs = group
                .Where(r => !emptyIdentifiersWithoutUIDs.Contains(r) && !emptyIdentifiersNoCountry.Contains(r))
                .ToList();
            if (emptyIdentifiersWithoutUIDs.Any())
            {
                var firstNonEmpty = emptyIdentifiersWithoutUIDs.First();
                var roles = new HashSet<string>(firstNonEmpty.Role.Split(new[] { "," }, StringSplitOptions.None));
                var performResearch = firstNonEmpty.PerformResearch;
                var clientSide = firstNonEmpty.IsClientSide;
                var additionalComments = new HashSet<string>(firstNonEmpty.AdditionalComments.Split(new[] { ", " }, StringSplitOptions.None));
                foreach (var empty in emptyIdentifiersNoCountry)
                {
                    var emptyRoles = empty.Role.Split(new[] { "," }, StringSplitOptions.None);
                    foreach (var role in emptyRoles)
                    {
                        if (!roles.Any(r => r.Contains(role)))
                        {
                            roles.Add(role);
                        }
                    }
                    var emptyadditionalComments = empty.AdditionalComments.Split(new[] { ", " }, StringSplitOptions.None);
                    foreach (var comments in emptyadditionalComments)
                    {
                        if (!additionalComments.Any(r => r.Contains(comments)))
                        {
                            additionalComments.Add(comments);
                        }
                    }
                    performResearch |= empty.PerformResearch;
                    clientSide &= empty.IsClientSide; // If any ClientSide is false, this will be false
                }
                firstNonEmpty.Role = string.Join(",", roles);
                firstNonEmpty.AdditionalComments = string.Join(", ", additionalComments.Where(comment => !string.IsNullOrEmpty(comment)));
                firstNonEmpty.PerformResearch = performResearch;
                firstNonEmpty.IsClientSide = clientSide;
                mergedSummaries.Add(firstNonEmpty);
                mergedSummaries.AddRange(emptyIdentifiersWithoutUIDs.Skip(1));
                mergedSummaries.AddRange(nonEmptyIdentifiersWithUIDs);
            }
            else
            {
                mergedSummaries.AddRange(emptyIdentifiersNoCountry);
                mergedSummaries.AddRange(nonEmptyIdentifiersWithUIDs);
            }
        }
        MainGrid = mergedSummaries.OrderBy(i => i.OrderNo).ToList();
        return MainGrid;
    }
    public static string DropLegalExtension(string Name, string Type)
    {
        string outputName = String.Empty;
        try
        {

            Name = Name.Replace("&", "and");

            if (Type == "Individual")
                outputName = Program.KeywordGeneratorForIndividuals.ReplaceCustomSubstrings(Name);
            else
                outputName = Program.KeywordGeneratorForEntities.ReplaceCustomSubstrings(Name);

            // :) Remove special chars
            outputName = KeyGen.KeyGen.RemoveSpecialCharacters(outputName);

            ////Remove space also
            outputName = outputName.Replace(" ", string.Empty);
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, "DropLegalExtension");
        }

        return outputName;
    }

    public static void PACEAPGGUPCleanUp(List<ResearchSummary> rss, List<ResearchSummary> APGGrid)
    {
        foreach (ResearchSummary rs in APGGrid)
        {
            if (rs.Role.StartsWith("GUP of"))
            {
                if (!String.IsNullOrEmpty(rs.DUNSNumber))
                {

                    if (APGGrid.Any(x => rs.EntityWithoutLegalExt.ToLower() == x.EntityWithoutLegalExt.ToLower()
                                    && rs.DUNSNumber == x.DUNSNumber && !x.Role.StartsWith("GUP of")))
                        APGGrid.Remove(rs);
                }
                else if (!String.IsNullOrEmpty(rs.GISID))
                {
                    if (APGGrid.Any(x => rs.EntityWithoutLegalExt.ToLower() == x.EntityWithoutLegalExt.ToLower()
                                    && rs.GISID == x.GISID && !x.Role.StartsWith("GUP of")))
                        APGGrid.Remove(rs);
                }
                else if (!String.IsNullOrEmpty(rs.MDMID))
                {
                    if (APGGrid.Any(x => rs.EntityWithoutLegalExt.ToLower() == x.EntityWithoutLegalExt.ToLower()
                                     && rs.MDMID == x.MDMID && !x.Role.StartsWith("GUP of")))
                        APGGrid.Remove(rs);
                }
                else if (!String.IsNullOrEmpty(rs.Country))
                {
                    if (APGGrid.Any(x => rs.EntityWithoutLegalExt.ToLower() == x.EntityWithoutLegalExt.ToLower()
                                     && rs.Country == x.Country && !x.Role.StartsWith("GUP of")))
                        APGGrid.Remove(rs);
                }
                else if (!String.IsNullOrEmpty(rs.DUNSNumber) && !String.IsNullOrEmpty(rs.MDMID) && !String.IsNullOrEmpty(rs.GISID))
                {
                    if (APGGrid.Any(x => rs.EntityWithoutLegalExt.ToLower() == x.EntityWithoutLegalExt.ToLower()
                    && rs.MDMID == x.MDMID && rs.GISID == x.GISID && rs.DUNSNumber == x.DUNSNumber && rs.Country == x.Country && !x.Role.StartsWith("GUP of")))
                        APGGrid.Remove(rs);
                }
            }
        }
    }

    public static List<Restrictions> GetRestrictions(string GISIDs, string GISConnectionString)
    {
        List<Restrictions> lsRestrictions = new List<Restrictions>();
        try
        {
            if (!string.IsNullOrEmpty(GISIDs))
            {
                string SQL = @" DROP TABLE IF EXISTS #DistinctRestriction
                SELECT DISTINCT Name as Restriction, RI.GISID INTO #DistinctRestriction
                FROM RestrictionImpact RI Join Restriction R on R.Id= RI.restrictionId
                WHERE R.Visibility <> 'admins' AND RI.gisid in ( " + GISIDs + " )    " +
                    "SELECT GISID, STRING_AGG(Restriction,', ') as RestrictionName FROM #DistinctRestriction GROUP BY GISID";

                System.Data.DataTable dt = EYSql.ExecuteDataset(GISConnectionString, CommandType.Text, SQL).Tables[0];

                Log.Information($"GetRestrictions GIS Result count: {dt.Rows.Count}");
                foreach (DataRow dr in dt.Rows)
                {
                    Log.Information($"GetRestrictions GISId: {dr.GetLong("GISID")}, RestrictionName: {dr.GetString("RestrictionName")}");
                    lsRestrictions.Add(new Restrictions()
                    {
                        GISID = dr.GetLong("GISID"),
                        RestrictionName = dr.GetString("RestrictionName")
                    });
                }
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, "GetRestrictions");
            Log.Error(ex, "GetRestrictions Error");
        }
        return lsRestrictions;
    }
    public void AddWorkToQueue(Work_GetMoreWork getMoreWork, bool startNow)
    {
        try
        {
            string strQuery = @"
                IF NOT EXISTS (SELECT 1 FROM [dbo].[CAU_WorkQueue] with (nolock) WHERE [GUID] = @GUID)
                BEGIN

                INSERT INTO [dbo].[CAU_WorkQueue" + @"]
                   ([GUID]
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
                   ,[ServerName]
                   ,[DataEntryDate])
                VALUES
                   (@GUID
                   ,@Reference
                   ,@Title
                   ,@DueDate
                   ,@Status
                   ,@ProcessType
                   ,@CustomerName
                   ,@ContractName
                   ,@ServiceName
                   ,@ProcessName
                   ,@ConflictCheckId
                   ,@ServerName
                   ,@DataEntryDate) 

                END";

            EYSql.ExecuteNonQuery(_configuration.ConnectionString, CommandType.Text, strQuery,
                new SqlParameter("@Guid", getMoreWork.GUID),
                new SqlParameter("@Reference", getMoreWork.Reference),
                new SqlParameter("@Title", getMoreWork.Title),
                new SqlParameter("@DueDate", getMoreWork.DueDate),
                new SqlParameter("@Status", getMoreWork.Status),
                new SqlParameter("@ProcessType", getMoreWork.ProcessType),
                new SqlParameter("@CustomerName", getMoreWork.CustomerName),
                new SqlParameter("@ContractName", getMoreWork.ContractName),
                new SqlParameter("@ServiceName", getMoreWork.ServiceName),
                new SqlParameter("@ProcessName", getMoreWork.ProcessName),
                new SqlParameter("@ConflictCheckId", getMoreWork.ConflictCheckId),
                new SqlParameter("@ServerName", System.Environment.MachineName),
                new SqlParameter("@DataEntryDate", DateTime.Now.TimestampWithTimezoneFromLocal("India Standard Time", "", "yyyy-MM-ddTHH:mm:ss")));

            if (startNow)
            {
                EYSql.ExecuteNonQuery(_configuration.ConnectionString, CommandType.Text, @"UPDATE dbo.CAU_WorkQueue" + @" SET IsStarted = 1, StartTime = @StartTime,[ServerName] = @ServerName  WHERE GUID = @Guid",
                new SqlParameter("@Guid", getMoreWork.GUID),
                new SqlParameter("@ServerName", System.Environment.MachineName),
                new SqlParameter("@StartTime", DateTime.Now.TimestampWithTimezoneFromLocal("India Standard Time", "", "yyyy-MM-ddTHH:mm:ss")));
            }
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, "AddWorkToQueue");
        }
    }

    public void UpdateWorkQueue(Work_GetMoreWork getMoreWork)
    {
        try
        {
            string strQuery = @"UPDATE [dbo].[CAU_WorkQueue" + @"] SET EndTime = @EndTime WHERE [GUID] = @GUID";
            EYSql.ExecuteNonQuery(_configuration.ConnectionString, CommandType.Text, strQuery, new SqlParameter("@GUID", getMoreWork.GUID),
                new SqlParameter("@EndTime", DateTime.Now.TimestampWithTimezoneFromLocal("India Standard Time", "", "yyyy-MM-ddTHH:mm:ss")));
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, "UpdateWorkQueue");
        }
    }

    public List<string> GetAPGRolesExclusion()
    {
        List<string> apgRoles = new List<string>();

        string strQuery = "SELECT RoleName FROM dbo.CAU_APGRolesToBlockResearch ";

        DataTable dt = new DataTable();
        dt = EYSql.ExecuteDataset(_configuration.ConnectionString, CommandType.Text, strQuery).Tables[0];

        foreach (DataRow dr in dt.Rows)
        {
            apgRoles.Add(dr["RoleName"].ToString());
        }

        return apgRoles;
    }

    public Work_GetMoreWork GetWorkQueueItem(string ContractName)
    {
        Work_GetMoreWork _result = new Work_GetMoreWork();
        try
        {
            //  string strQuery = @"SELECT TOP 1 [ID]
            //,[GUID]
            //,[Reference]
            //,[Title]
            //,[DueDate]
            //,[Status]
            //,[ProcessType]
            //,[CustomerName]
            //,[ContractName]
            //,[ServiceName]
            //,[ProcessName]
            //,[ConflictCheckId]
            //,[IsStarted]
            //,[IsOnHold]
            //,[StartTime]
            //,[EndTime]
            //,[DataEntryDate] FROM dbo.CAU_WorkQueue WHERE ISNULL(IsStarted, 0) = 0 and StartTime is null and ContractName LIKE '" + ContractName + "%' ORDER BY DueDate";

            string strQuery = @"BEGIN TRAN
	SELECT TOP 1 ID INTO #tmp_WorkQueue
    FROM dbo.CAU_WorkQueue" + @" WHERE ISNULL(IsStarted, 0) = 0 and ISNULL(IsOnHold, 0) = 0 AND StartTime is null and ContractName LIKE '" + ContractName + @"%' ORDER BY DueDate
	
	IF (SELECT COUNT(1) FROM #tmp_WorkQueue) = 1 BEGIN
		UPDATE dbo.CAU_WorkQueue" + @" SET IsOnHold = 1 WHERE Id = (SELECT ID FROM #tmp_WorkQueue)
		INSERT INTO dbo.CAU_WorkQueue_Log" + @" SELECT ID, '" + System.Environment.MachineName + @"' FROM #tmp_WorkQueue

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
      ,[DataEntryDate] FROM dbo.CAU_WorkQueue" + @" WHERE ID = (SELECT ID FROM #tmp_WorkQueue)

	END ELSE
        SELECT 1 FROM dbo.CAU_WorkQueue" + @" WHERE 1 =0
DROP TABLE #tmp_WorkQueue
	COMMIT TRAN

	";

            DataSet _ds = EYSql.ExecuteDataset(_configuration.ConnectionString, CommandType.Text, strQuery);

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
            }
            else
                return null;
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, "GetWorkQueueItem");
        }
        return _result;
    }

    public List<string> GetPriority(DateTime ist_now)
    {
        int _time = ist_now.Hour * 100 + ist_now.Minute;
        string strQuery = "SELECT Priority1, Priority2, Priority3 FROM dbo.CAU_PrioritySchedule with (nolock) WHERE @CurrentTime >= TimeStart and @CurrentTime < TimeEnd";

        DataSet _ds = EYSql.ExecuteDataset(_configuration.ConnectionString, CommandType.Text, strQuery, new SqlParameter("@CurrentTime", _time));

        if (_ds.Tables[0].Rows.Count > 0)
        {
            var results = _ds.Tables[0].Rows[0].ItemArray.Cast<string>().ToList();
            return results;
        }
        else
            return null;
    }
    public static void DeleteOldFiles(string path, int daysOld, List<string> excludeList)
    {
        DeleteOldFilesAndFoldersRecursive(path, daysOld, excludeList);
        Console.WriteLine("Old files and folders deleted successfully.");
    }
    private static void DeleteOldFilesAndFoldersRecursive(string path, int daysOld, List<string> excludeList)
    {
        var files = Directory.GetFiles(path);
        var directories = Directory.GetDirectories(path);
        // Delete files
        foreach (var file in files)
        {
            DateTime creationTime = File.GetCreationTime(file);
            string fileName = Path.GetFileName(file);
            if (creationTime < DateTime.Now.AddDays(-daysOld) && !excludeList.Contains(fileName))
            {
                File.Delete(file);
                Console.WriteLine($"Deleted file: {file}");
            }
        }

        // Recursively delete directories
        foreach (var directory in directories)
        {
            DeleteOldFilesAndFoldersRecursive(directory, daysOld, excludeList);
            // After deleting files in the directory, check if it's empty and delete the directory if it is
            if (Directory.GetFiles(directory).Length == 0 && Directory.GetDirectories(directory).Length == 0)
            {
                Directory.Delete(directory);
                Console.WriteLine($"Deleted directory: {directory}");
            }
        }
    }
}

#pragma warning restore IDE0037 // Use inferred member name
#pragma warning restore CA1861 // Avoid constant arrays as arguments
#pragma warning restore CA1860 // Avoid using 'Enumerable.Any()' extension method
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore CA1827 // Do not use Count() or LongCount() when Any() can be used
#pragma warning restore IDE0270 // Use coalesce expression
#pragma warning restore IDE0074 // Use compound assignment
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore CA2211 // Non-constant fields should not be visible
#pragma warning restore IDE0305 // Simplify collection initialization
#pragma warning restore IDE0017 // Simplify object initialization
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
#pragma warning restore IDE0063 // Use simple 'using' statement
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0028 // Simplify collection initialization
#pragma warning restore IDE0075 // Simplify conditional expression
