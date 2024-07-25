
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   PROCEDURE [dbo].[sp_REP_Conflict_Checker_getData] (
       @Client NVARCHAR(2000),
       @Engagement NVARCHAR(2000),
       @GTACStatusFolderName NVARCHAR(2000),
       @EngagementCountry NVARCHAR(2000),
       @OpenDateFrom DATE,
       @OpenDateTo DATE
       )
AS
BEGIN
       /* 
     * Proc# -773 -- for use in testing
     * Test using:
          exec [Reporting].[sp_REP_Conflict_Checker_getData] @LoadLogId = -773;  -- use a negative number to avoid conflict with real logging. 

          select * from Logging.ProgressLog where LoadLogId = -773; 
     */
	  --Tejal - As we are going to use all Countries so removing Country clause 

	 --EXEC [dbo].[sp_REP_Conflict_Checker_getData] 'Car (shanghai)', '', '', '', '2019-05-22', '2024-05-22'

--EXEC [dbo].[sp_REP_Conflict_Checker_getData] '619846346', '', '', '', '2019-05-22', '2024-05-22'

--EXEC [dbo].[sp_REP_Conflict_Checker_getData] 'Platinum Equity', '', '', '', '2019-05-22', '2024-05-22' --1076

--EXEC [dbo].[sp_REP_Conflict_Checker_getData] '837601723', '', '', '', '2019-05-22', '2024-05-22' --453
	
       SET NOCOUNT ON;
 
       DECLARE @CountOfInsertedRows INT = NULL,
              @CountOfUpdatedRows INT = NULL,
              @CountOfDeletedRows INT = NULL,
              @AdditionalInfo NVARCHAR(max),
              @errorMsg NVARCHAR(max),
              @errorNumber INT;
 
       EXEC dbo.Log_ProcedureCall @LoadLogId = 0,
              @ProcedureObjectID = @@PROCID,
              @ActionType = N'START';
 
       BEGIN TRY
 
          IF @Client = '' AND @Engagement = '' AND @GTACStatusFolderName = ''
       BEGIN
       SELECT      * 
       FROM     dbo.vwMercury_Client_Engagement
       WHERE  --EngagementCountry in (SELECT Parameter FROM dbo.Splitlist(@EngagementCountry,',')) AND 
                      EngagementCreationDate BETWEEN @OpenDateFrom AND @OpenDateTo
       ORDER BY      [NER] DESC
       END
 
          IF @Client <> '' AND @Engagement = '' AND @GTACStatusFolderName = '' 
       BEGIN
       SELECT      * 
       FROM     dbo.vwMercury_Client_Engagement
       WHERE       --  EngagementCountry in (SELECT Parameter FROM dbo.Splitlist(@EngagementCountry,',')) AND
                      EngagementCreationDate BETWEEN @OpenDateFrom AND @OpenDateTo
                                    AND ( ClientID like '%' + @Client + '%'  or Client like '%' + @Client + '%' or DunsName like '%' + @Client + '%' or DunsNumber like '%' + @Client + '%'  )
       ORDER BY      [NER] DESC
 
       END
 
          IF @Client = '' AND @Engagement <> '' AND @GTACStatusFolderName = ''
       BEGIN
       SELECT      * 
       FROM     dbo.vwMercury_Client_Engagement
       WHERE      --   EngagementCountry in (SELECT Parameter FROM dbo.Splitlist(@EngagementCountry,',')) AND
                      EngagementCreationDate BETWEEN @OpenDateFrom AND @OpenDateTo
                                    AND ( EngagementID like '%' + @Engagement + '%' or Engagement like '%' + @Engagement + '%' )
       ORDER BY      [NER] DESC
 
       END
 
          IF @Client = '' AND @Engagement = '' AND @GTACStatusFolderName <> ''
       BEGIN
       SELECT      * 
       FROM     dbo.vwMercury_Client_Engagement
       WHERE     --    EngagementCountry in (SELECT Parameter FROM dbo.Splitlist(@EngagementCountry,',')) AND
                      EngagementCreationDate BETWEEN @OpenDateFrom AND @OpenDateTo
                                    AND ( GTACStatusFolderName like '%' + @GTACStatusFolderName + '%' )
       ORDER BY      [NER] DESC
 
       END
 
          IF @Client <> '' AND @Engagement = '' AND @GTACStatusFolderName <> ''
       BEGIN
       SELECT      * 
       FROM     dbo.vwMercury_Client_Engagement
       WHERE    --     EngagementCountry in (SELECT Parameter FROM dbo.Splitlist(@EngagementCountry,',')) AND
                      EngagementCreationDate BETWEEN @OpenDateFrom AND @OpenDateTo
                                    AND ( ClientID like '%' + @Client + '%'  or Client like '%' + @Client + '%' or DunsName like '%' + @Client + '%' or DunsNumber like '%' + @Client + '%'  )
                                    AND ( GTACStatusFolderName like '%' + @GTACStatusFolderName + '%' )
       ORDER BY      [NER] DESC
 
       END
 
          IF @Client = '' AND @Engagement <> '' AND @GTACStatusFolderName <> ''
       BEGIN
       SELECT      * 
       FROM     dbo.vwMercury_Client_Engagement
       WHERE    --     EngagementCountry in (SELECT Parameter FROM dbo.Splitlist(@EngagementCountry,',')) AND
                      EngagementCreationDate BETWEEN @OpenDateFrom AND @OpenDateTo
                                    AND ( EngagementID like '%' + @Engagement + '%' or Engagement like '%' + @Engagement + '%' )
                                    AND ( GTACStatusFolderName like '%' + @GTACStatusFolderName + '%' )
       ORDER BY      [NER] DESC
 
       END
 
          IF @Client <> '' AND @Engagement <> '' AND @GTACStatusFolderName = ''
       BEGIN
       SELECT      * 
       FROM     dbo.vwMercury_Client_Engagement
       WHERE    --     EngagementCountry in (SELECT Parameter FROM dbo.Splitlist(@EngagementCountry,',')) AND
                      EngagementCreationDate BETWEEN @OpenDateFrom AND @OpenDateTo
                                    AND ( ClientID like '%' + @Client + '%'  or Client like '%' + @Client + '%' or DunsName like '%' + @Client + '%' or DunsNumber like '%' + @Client + '%'  )
                                    AND ( EngagementID like '%' + @Engagement + '%' or Engagement like '%' + @Engagement + '%' )
       ORDER BY      [NER] DESC
 
       END
 
          IF @Client <> '' AND @Engagement <> '' AND @GTACStatusFolderName <> ''
       BEGIN
       SELECT      * 
       FROM     dbo.vwMercury_Client_Engagement
       WHERE    --     EngagementCountry in (SELECT Parameter FROM dbo.Splitlist(@EngagementCountry,',')) AND
                      EngagementCreationDate BETWEEN @OpenDateFrom AND @OpenDateTo
                                    AND ( ClientID like '%' + @Client + '%'  or Client like '%' + @Client + '%' or DunsName like '%' + @Client + '%' or DunsNumber like '%' + @Client + '%'  )
                                    AND ( EngagementID like '%' + @Engagement + '%' or Engagement like '%' + @Engagement + '%' )
                                    AND ( GTACStatusFolderName like '%' + @GTACStatusFolderName + '%' )
       ORDER BY      [NER] DESC
       END
       END TRY
 
       BEGIN CATCH
              SELECT @errorMsg = error_message(),
                      @errorNumber = error_number();
 
              IF (@@TRANCOUNT > 0)
              BEGIN
                      ROLLBACK TRANSACTION;
              END;
 
              EXEC dbo.Log_ProcedureCall @LoadLogId = 0,
                      @ProcedureObjectID = @@PROCID,
                      @ActionType = N'ERROR',
                      @ErrorNumber = @errorNumber,
                      @ErrorMessage = @errorMsg;
 
              -- the SQL error numbers are not in the right range for throw, so add 100000
              SELECT @errorNumber = 100000 + @errorNumber;
 
              throw @errorNumber,
                      @errorMsg,
                      16;
       END CATCH
 
       EXEC dbo.Log_ProcedureCall @LoadLogId = 0,
              @ProcedureObjectID = @@PROCID,
              @ActionType = N'END',
              @CountOfInsertedRows = @CountOfInsertedRows,
              @CountOfUpdatedRows = @CountOfUpdatedRows,
              @CountOfDeletedRows = @CountOfDeletedRows;
END
