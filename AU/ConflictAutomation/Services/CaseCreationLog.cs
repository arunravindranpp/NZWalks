using ConflictAutomation.Extensions;
using ConflictAutomation.Models;
using System.Data;
using System.Data.SqlClient;

namespace ConflictAutomation.Services
{
    public class CaseCreationLog
    {
        private readonly AppConfigure _configuration;

        public CaseCreationLog(AppConfigure configuration)
        {
            _configuration = configuration;
        }
        public long CaseCreationStartLog(long ConflictCheckID, DateTime ConflictInitiatedDate, long BatchID)
        {
            try
            {
                string sourceConn = _configuration.ConnectionString.ToString();
                var sqlConnection = new SqlConnection(sourceConn);

                sqlConnection.Open();               

                var sqlQuery = @"INSERT INTO CAU_CaseCreationLog 
                                (BatchID, ConflictCheckID, ConflictInitiatedDate, StartTime)          
                                VALUES (@BatchID, @ConflictCheckID, @ConflictInitiatedDate, @StartTime) 

                            SELECT SCOPE_IDENTITY()";

                var command = new SqlCommand(sqlQuery, sqlConnection);
                command.Parameters.AddWithValue("@BatchID", BatchID);
                command.Parameters.AddWithValue("@ConflictCheckID", ConflictCheckID);
                command.Parameters.AddWithValue("@ConflictInitiatedDate", ConflictInitiatedDate);
                command.Parameters.AddWithValue("@StartTime", DateTime.Now.TimestampWithTimezoneFromLocal("India Standard Time", "", "yyyy-MM-ddTHH:mm:ss"));

                var logId = Convert.ToInt64(command.ExecuteScalar());
                return logId;
            }
            catch (Exception ex)
            {
                LoggerInfo.LogException(ex, "CaseCreationStartLog - " + ConflictCheckID.ToString());
                return 0;
            }
        }
      
        public void UpdateCaseCreationLog(long ID, CaseCreationLogModel CaseCreationLog)
        {
            try
            {
                SqlParameter[] parms = {
                             new SqlParameter("@a_Country", CaseCreationLog.Country)
                           , new SqlParameter("@a_Region", CaseCreationLog.Region)
                           , new SqlParameter("@a_SLName", CaseCreationLog.SLName)
                           , new SqlParameter("@a_CheckCategory", CaseCreationLog.CheckCategory)
                           , new SqlParameter("@a_NoOfEntities", CaseCreationLog.NoOfEntities)
                           , new SqlParameter("@a_EndTime", DateTime.Now.TimestampWithTimezoneFromLocal("India Standard Time", "", "yyyy-MM-ddTHH:mm:ss"))
                           , new SqlParameter("@a_isErrored", CaseCreationLog.IsErrored)
                           , new SqlParameter("@a_LogID", ID)
                };

                string SQL = @"UPDATE CAU_CaseCreationLog  SET Country = @a_Country, 
                            Region = @a_Region, SLName = @a_SLName, CheckCategory = @a_CheckCategory, 
                            NoOfEntities = @a_NoOfEntities, 
                            EndTime = @a_EndTime, isErrored = @a_isErrored
                            WHERE [ID] = @a_LogID ";

                EYSql.ExecuteDataset(Program.PACEConnectionString, CommandType.Text, SQL, parms);
            }
            catch (Exception ex)
            {
                LoggerInfo.LogException(ex, "UpdateCaseCreationLog - LOGID= " + ID.ToString());               
            }
        }
    }
}

