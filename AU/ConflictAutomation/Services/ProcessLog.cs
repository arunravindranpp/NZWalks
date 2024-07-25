using ConflictAutomation.Extensions;
using ConflictAutomation.Models;
using System.Data;
using System.Data.SqlClient;

namespace ConflictAutomation.Services
{
    public class ProcessLog
    {
        private readonly AppConfigure _configuration;

        public ProcessLog(AppConfigure configuration)
        {
            _configuration = configuration;
        }
        public long StartLog(long ConflictCheckID, string ProcessStart, string Enviornment)
        {
            string sourceConn = _configuration.ConnectionString.ToString();
            var sqlConnection = new SqlConnection(sourceConn);

            sqlConnection.Open();

            var sqlQuery = @"INSERT INTO CAU_ProcessLog 
                                (ConflictCheckID, Enviornment, ProcessStart)          
                                VALUES (@ConflictCheckID, @Enviornment, @ProcessStart) 

                            SELECT SCOPE_IDENTITY()";

            var command = new SqlCommand(sqlQuery, sqlConnection);
            command.Parameters.AddWithValue("@ConflictCheckID", ConflictCheckID);
            command.Parameters.AddWithValue("@ProcessStart", ProcessStart);
            command.Parameters.AddWithValue("@Enviornment", Enviornment);

            var logId = Convert.ToInt64(command.ExecuteScalar());
            return logId;
        }
        // string PACEExtractionEnd, string AUUnitGridStart, string EntityCount, string EntitiesList, string KeyGenStart, 
        //string KeyGenCount, string Keywords, string GISStart, string MercuryStart, string CRRStart, string FinscanStart, string SPLStart, bool isErrored)
        public static void UpdateProcessLog(long ProcessLogID, ProcessedChecks ProcessedLog)
        {
            string processEnd = DateTime.Now.TimestampWithTimezoneFromLocal("India Standard Time", "", "yyyy-MM-ddTHH:mm:ss"); ;

            ProcessedLog.ProcessEnd = processEnd;

            SqlParameter[] parms = { 
                             new SqlParameter("@a_PACEExtractionEnd", ProcessedLog.PACEExtractionEnd)                  
                           , new SqlParameter("@a_AUUnitGridStart", ProcessedLog.AUUnitGridStart)
                           , new SqlParameter("@a_EntityCount", ProcessedLog.EntityCount)
                           , new SqlParameter("@a_EntitiesList", ProcessedLog.EntitiesList)
                           , new SqlParameter("@a_KeyGenStart", ProcessedLog.KeyGenStart)
                           , new SqlParameter("@a_KeyGenCount", ProcessedLog.KeyGenCount)
                           , new SqlParameter("@a_Keywords", ProcessedLog.Keywords)
                           , new SqlParameter("@a_GISStart", ProcessedLog.GISStart)
                           , new SqlParameter("@a_MercuryStart", ProcessedLog.MercuryStart)
                           , new SqlParameter("@a_CRRStart",    ProcessedLog.CRRStart)
                           , new SqlParameter("@a_FinscanStart", ProcessedLog.FinscanStart)
                           , new SqlParameter("@a_SPLStart", ProcessedLog.SPLStart)
                           , new SqlParameter("@a_processEnd", processEnd)                          
                           , new SqlParameter("@a_isErrored", ProcessedLog.IsErrored)
                           , new SqlParameter("@a_processLogID", ProcessLogID)
                };

            string SQL = @"UPDATE CAU_ProcessLog SET PACEExtractionEnd = @a_PACEExtractionEnd, 
                            AUUnitGridStart = @a_AUUnitGridStart, EntityCount = @a_EntityCount, EntitiesList = @a_EntitiesList, 
                            KeyGenStart = @a_KeyGenStart, KeyGenCount = @a_KeyGenCount, 
                            Keywords = @a_Keywords, GISStart = @a_GISStart, MercuryStart = @a_MercuryStart, CRRStart = @a_CRRStart, FinscanStart = @a_FinscanStart, 
                            SPLStart = @a_SPLStart, processEnd = @a_processEnd, isErrored = @a_isErrored
                            WHERE [LogID] = @a_processLogID ";

            DataSet dsLog = PACE.EYSql.ExecuteDataset(Program.PACEConnectionString, CommandType.Text, SQL, parms);
        }
    }
}

