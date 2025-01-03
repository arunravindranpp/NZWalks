﻿CREATE TABLE [dbo].[CAU_ProcessLog]
(
	LogID BIGINT IDENTITY(1,1) PRIMARY KEY,
	ConflictCheckID BIGINT NOT NULL,
	Enviornment NVARCHAR(10) NULL,
	ProcessStart NVARCHAR(MAX) NULL,
	PACEExtractionEnd NVARCHAR(MAX) NULL,
	AUUnitGridStart NVARCHAR(MAX) NULL,
	EntityCount NVARCHAR(MAX) NULL,
	EntitiesList NVARCHAR(MAX) NULL,
	KeyGenStart NVARCHAR(MAX) NULL,
	KeyGenCount NVARCHAR(MAX) NULL,
	Keywords NVARCHAR(MAX) NULL,
	GISStart NVARCHAR(MAX) NULL,
	MercuryStart NVARCHAR(MAX) NULL,
	CRRStart NVARCHAR(MAX) NULL,
	FinscanStart NVARCHAR(MAX) NULL,
	SPLStart NVARCHAR(MAX) NULL,
	ProcessEnd NVARCHAR(MAX) NULL,
	IsErrored BIT
)
GO