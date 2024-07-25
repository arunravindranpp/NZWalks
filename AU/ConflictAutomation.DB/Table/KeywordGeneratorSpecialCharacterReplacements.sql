-- KeywordGeneratorSpecialCharacterReplacements.sql
-- 2024-07-03 20:50 UTC

USE PACETechRefresh_UAT
-- USE PACE4
GO


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


IF OBJECT_ID('dbo.KeywordGeneratorSpecialCharacterReplacements', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.KeywordGeneratorSpecialCharacterReplacements
END
GO


CREATE TABLE dbo.KeywordGeneratorSpecialCharacterReplacements (
	ID                   INT     IDENTITY(1,1) NOT NULL,
	KeywordGeneratorType VARCHAR(3)            NOT NULL,
	FromValue            VARCHAR(255)          NOT NULL,
	ToValue              VARCHAR(255)          NOT NULL,
	
	CONSTRAINT PK_KeywordGeneratorSpecialCharacterReplacements PRIMARY KEY CLUSTERED 
		(ID ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY], 
	
	CONSTRAINT chk_SpecialCharacterReplacementsKeywordGeneratorType CHECK (KeywordGeneratorType IN ('E', 'I'))    -- 'E' = Entity    'I' = Individual
) ON [PRIMARY]
GO


INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '‐', '-')  -- From ASC 8208 = UNICODE \u2010 to ASC 45 = UNICODE \u002D (regular dash)
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '–', '-')  -- From ASC 8211 = UNICODE \u2013 to ASC 45 = UNICODE \u002D (regular dash)
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '!', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '@', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '#', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '$', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '%', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '^', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '*', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '_', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '+', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '=', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '|', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '\', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', ':', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', ';', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '<', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '>', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '?', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '~', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '/', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '"', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '“', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '”', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '.', '')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '''', '')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '`', '')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('E', '´', '')

INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '‐', '-')  -- From ASC 8208 = UNICODE \u2010 to ASC 45 = UNICODE \u002D (regular dash)
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '–', '-')  -- From ASC 8211 = UNICODE \u2013 to ASC 45 = UNICODE \u002D (regular dash)
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '!', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '@', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '#', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '$', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '%', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '^', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '*', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '_', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '+', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '=', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '|', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '\', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', ':', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', ';', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '<', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '>', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '?', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '~', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '/', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '"', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '“', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '”', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '(', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', ')', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '[', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', ']', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '{', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '}', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '.', ' ')
INSERT dbo.KeywordGeneratorSpecialCharacterReplacements(KeywordGeneratorType, FromValue, ToValue) VALUES('I', '&', ' ')

GO


-- END OF THIS FILE
