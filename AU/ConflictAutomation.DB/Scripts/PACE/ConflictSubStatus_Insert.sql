-- Insert new substatuses for AU

IF NOT EXISTS (SELECT * from WF_ConflictCheckSubStatus WHERE ShortVersion = 'Research in progress')
	INSERT INTO [dbo].[WF_ConflictCheckSubStatus] VALUES ('Research in progress', 'Please contact Conflicts Resource ( EY_RMS_Conflicts_Resource.GID@ey.net ) for any query related to status of the check.')
GO

IF NOT EXISTS (SELECT * from WF_ConflictCheckSubStatus WHERE ShortVersion = 'Check pre-screened')
	INSERT INTO [dbo].[WF_ConflictCheckSubStatus] VALUES ('Check pre-screened', 'Please contact Conflicts Resource ( EY_RMS_Conflicts_Resource.GID@ey.net ) for any query related to status of the check.')
GO

IF NOT EXISTS (SELECT * from WF_ConflictCheckSubStatus WHERE ShortVersion = 'Research completed')
	INSERT INTO [dbo].[WF_ConflictCheckSubStatus] VALUES ('Research completed', 'Please contact Conflicts Resource ( EY_RMS_Conflicts_Resource.GID@ey.net ) for any query related to status of the check.')
GO
