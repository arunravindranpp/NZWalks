using ConflictAutomation.Constants;
using Serilog;
using System.Data;
using System.Data.SqlClient;

namespace ConflictAutomation.Services;

public class ConflictCheckAttachmentUtility(string connectionString)
{
    private readonly string _connectionString = connectionString;

    public long InsertAttachment(long conflictCheckID, string filePath, 
        string fileType = CAUConstants.ATTACHMENTS_FOR_ASSESSMENT_TEAM, 
        int entityTypeId = CAUConstants.ENTITY_TYPE_ID_CONFLICT_CHECK)
    {
        long newID = 0;

        string fileName = Path.GetFileName(filePath);
        byte[] fileContents = File.ReadAllBytes(filePath);

        try
        {
            object ret = EYSql.ExecuteScalar(_connectionString, CommandType.Text,
                                    "INSERT INTO WF_Attachments " + 
                                    "(FileName, Type, Content, UploadedDate, EntityID, EntityTypeID) " + 
                                    "VALUES (@fileName, @type, @fileContents, GETUTCDATE(), @conflictCheckID, @entityTypeID); " + 
                                    "SELECT @@IDENTITY",
                                        new SqlParameter("@fileName", fileName),
                                        new SqlParameter("@type", fileType),
                                        new SqlParameter("@fileContents", fileContents),
                                        new SqlParameter("@conflictCheckID", conflictCheckID),
                                        new SqlParameter("@entityTypeID", entityTypeId));

            long.TryParse(ret.ToString(), out newID);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred when executing method ConflictCheckAttachmentUtility.InsertAttachment() - Message: {ex}");
            LoggerInfo.LogException(ex);
        }

        return newID;
    }


    public void UpdateFileEntityToDB(long attachmentID, long entityID, int entityTypeID,string GUI, string description = null)
    {
        try
        {

            EYSql.ExecuteNonQuery(_connectionString, CommandType.Text,
                  @"UPDATE WF_ATTACHMENTS SET EntityId=IsNull(EntityId, @EntityId), EntityTypeId=@EntityTypeID, UploadedDate=getutcdate(), AttachedBy = @AttachedBy, Description = @Description WHERE AttachmentId=@AttachmentId",
                  new SqlParameter("@EntityId", entityID),
                  new SqlParameter("@EntityTypeId", entityTypeID),
                  new SqlParameter("@AttachmentId", attachmentID),
                  new SqlParameter("@AttachedBy", GUI),
                  new SqlParameter("@Description", description));
        }
        catch (Exception ex)
        {
            LoggerInfo.LogException(ex, "UpdateFileEntityToDB");
        }
    }


    public void RemoveExistingAttachmentsExceptForResearchTemplate(long conflictCheckID)
    {
        if (conflictCheckID < 1)
        {
            return;
        }

        try
        {
            // ***** Never remove Research Templates with results of previous processings
            string sql = "DELETE FROM WF_Attachments WHERE (EntityID = @conflictCheckID) " + 
                         "   AND (FileName NOT LIKE 'Research Template_%')";

            EYSql.ExecuteScalar(_connectionString, CommandType.Text, sql,
                                new SqlParameter("@conflictCheckID", conflictCheckID));
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred when executing method ConflictCheckAttachmentUtility.RemoveExistingAttachmentsExceptForResearchTemplate() - Message: {ex}");
            LoggerInfo.LogException(ex);
        }
    }
}
