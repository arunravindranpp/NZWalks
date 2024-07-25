-- KeywordGeneratorPrefixRemovals.sql
-- 2024-01-09 19:33 UTC

USE PACETechRefresh_UAT
GO


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


IF OBJECT_ID('dbo.KeywordGeneratorPrefixRemovals', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.KeywordGeneratorPrefixRemovals
END
GO


CREATE TABLE dbo.KeywordGeneratorPrefixRemovals (
	ID                   INT     IDENTITY(1,1) NOT NULL,
	KeywordGeneratorType VARCHAR(3)            NOT NULL,
	FromValue            VARCHAR(255)          NOT NULL	
	
	CONSTRAINT PK_KeywordGeneratorPrefixRemovals PRIMARY KEY CLUSTERED 
		(ID ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY], 
	
	CONSTRAINT chk_PrefixRemovalsKeywordGeneratorType CHECK (KeywordGeneratorType IN ('E', 'I'))    -- 'E' = Entity    'I' = Individual
) ON [PRIMARY]
GO

INSERT dbo.KeywordGeneratorPrefixRemovals (KeywordGeneratorType, FromValue) VALUES('E', 'do not use')
INSERT dbo.KeywordGeneratorPrefixRemovals (KeywordGeneratorType, FromValue) VALUES('E', 'inactive')
INSERT dbo.KeywordGeneratorPrefixRemovals (KeywordGeneratorType, FromValue) VALUES('E', 'invalid')
INSERT dbo.KeywordGeneratorPrefixRemovals (KeywordGeneratorType, FromValue) VALUES('E', 'liquidated')
INSERT dbo.KeywordGeneratorPrefixRemovals (KeywordGeneratorType, FromValue) VALUES('E', 'llc')
INSERT dbo.KeywordGeneratorPrefixRemovals (KeywordGeneratorType, FromValue) VALUES('E', 'merged')
INSERT dbo.KeywordGeneratorPrefixRemovals (KeywordGeneratorType, FromValue) VALUES('E', 'p.t.')
INSERT dbo.KeywordGeneratorPrefixRemovals (KeywordGeneratorType, FromValue) VALUES('E', 'p.t')
INSERT dbo.KeywordGeneratorPrefixRemovals (KeywordGeneratorType, FromValue) VALUES('E', 'private')
INSERT dbo.KeywordGeneratorPrefixRemovals (KeywordGeneratorType, FromValue) VALUES('E', 'pt.')
INSERT dbo.KeywordGeneratorPrefixRemovals (KeywordGeneratorType, FromValue) VALUES('E', 'pt')
INSERT dbo.KeywordGeneratorPrefixRemovals (KeywordGeneratorType, FromValue) VALUES('E', 'the')
GO


-- END OF THIS FILE
