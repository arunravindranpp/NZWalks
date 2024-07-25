-- KeywordGeneratorDiacriticsReplacements.sql
-- 2024-07-02 18:40 UTC

USE PACETechRefresh_UAT
-- USE PACE4


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


IF OBJECT_ID('dbo.KeywordGeneratorDiacriticsReplacements', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.KeywordGeneratorDiacriticsReplacements
END
GO


CREATE TABLE dbo.KeywordGeneratorDiacriticsReplacements (
	ID                   INT     IDENTITY(1,1) NOT NULL,
	FromValue            VARCHAR(255)          NOT NULL,
	ToValue_Step_1       VARCHAR(255)          NOT NULL,
	ToValue_Step_2       VARCHAR(255)          NOT NULL,
	ToValue_Step_3       VARCHAR(255)          NOT NULL,
	ToValue_Step_4       VARCHAR(255)          NOT NULL

	
	CONSTRAINT PK_KeywordGeneratorDiacriticsReplacements PRIMARY KEY CLUSTERED 
		(ID ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY], 
		
) ON [PRIMARY]
GO


INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00C1', '\u00C1', 'A', 'A', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00E1', '\u00E1', 'a', 'a', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00C0', '\u00C0', 'A', 'A', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00E0', '\u00E0', 'a', 'a', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00C2', '\u00C2', 'A', 'A', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00E2', '\u00E2', 'a', 'a', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00C4', '\u00C4', 'A', 'ae', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00E4', '\u00E4', 'a', 'ae', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00C3', '\u00C3', 'A', 'A', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00E3', '\u00E3', 'a', 'a', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00C5', '\u00C5', 'A', 'aa', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00E5', '\u00E5', 'a', 'aa', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u1EA6', '\u1EA6', 'A', 'A', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u1EA7', '\u1EA7', 'a', 'a', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u1EA2', '\u1EA2', 'A', 'A', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u1EA3', '\u1EA3', 'a', 'a', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u1EAC', '\u1EAC', 'A', 'A', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u1EAD', '\u1EAD', 'a', 'a', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00C6', '\u00C6', 'AE', 'AE', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00E6', '\u00E6', 'ae', 'ae', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u010C', '\u010C', 'C', 'C', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u010D', '\u010D', 'c', 'c', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00C7', '\u00C7', 'C', 'C', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00E7', '\u00E7', 'c', 'c', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u0110', '\u0110', 'D', 'D', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u0111', '\u0111', 'd', 'd', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00C9', '\u00C9', 'E', 'E', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00E9', '\u00E9', 'e', 'e', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00C8', '\u00C8', 'E', 'E', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00E8', '\u00E8', 'e', 'e', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00CA', '\u00CA', 'E', 'E', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00EA', '\u00EA', 'e', 'e', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00CB', '\u00CB', 'E', 'ee', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00EB', '\u00EB', 'e', 'ee', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u011A', '\u011A', 'E', 'E', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u011B', '\u011B', 'e', 'e', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u0118', '\u0118', 'E', 'E', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u0119', '\u0119', 'e', 'e', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u1EC2', '\u1EC2', 'E', 'E', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u1EC3', '\u1EC3', 'e', 'e', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u0120', '\u0120', 'G', 'G', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u0121', '\u0121', 'g', 'g', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u011E', '\u011E', 'G', 'G', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u011F', '\u011F', 'g', 'g', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u0126', '\u0126', 'H', 'H', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u0127', '\u0127', 'h', 'h', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00CD', '\u00CD', 'I', 'I', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00ED', '\u00ED', 'i', 'i', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00CC', '\u00CC', 'I', 'I', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00EC', '\u00EC', 'i', 'i', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00CE', '\u00CE', 'I', 'I', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00EE', '\u00EE', 'i', 'i', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00CF', '\u00CF', 'I', 'ie', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00EF', '\u00EF', 'i', 'ie', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u0128', '\u0128', 'I', 'I', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u0129', '\u0129', 'i', 'i', '')

INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u012A', '\u012A', 'I', 'I', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u012B', '\u012B', 'i', 'i', '')

INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u0141', '\u0141', 'L', 'L', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u0142', '\u0142', 'l', 'l', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u0143', '\u0143', 'N', 'N', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u0144', '\u0144', 'n', 'n', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00D1', '\u00D1', 'N', 'ny', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00F1', '\u00F1', 'n', 'ny', '')

INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u0145', '\u0145', 'N', 'N', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u0146', '\u0146', 'n', 'n', '')

INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00D3', '\u00D3', 'O', 'O', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00F3', '\u00F3', 'o', 'o', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00D2', '\u00D2', 'O', 'O', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00F2', '\u00F2', 'o', 'o', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00D4', '\u00D4', 'O', 'O', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00F4', '\u00F4', 'o', 'o', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00D6', '\u00D6', 'O', 'OE', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00F6', '\u00F6', 'o', 'oe', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u014C', '\u014C', 'O', 'O', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u014D', '\u014D', 'o', 'o', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00D5', '\u00D5', 'O', 'O', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00F5', '\u00F5', 'o', 'o', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u1ED2', '\u1ED2', 'O', 'O', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u1ED3', '\u1ED3', 'o', 'o', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00D8', '\u00D8', 'O', 'OE', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00F8', '\u00F8', 'o', 'oe', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u1ED4', '\u1ED4', 'O', 'O', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u1ED5', '\u1ED5', 'o', 'o', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u1EE2', '\u1EE2', 'O', 'O', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u1EE3', '\u1EE3', 'o', 'o', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u0152', '\u0152', 'OE', 'OE', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u0153', '\u0153', 'oe', 'oe', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u015A', '\u015A', 'S', 'S', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u015B', '\u015B', 's', 's', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u0160', '\u0160', 'S', 'S', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u0161', '\u0161', 's', 's', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u015E', '\u015E', 'S', 'S', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u015F', '\u015F', 's', 's', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00DF', '\u00DF', 's', 'ss', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00DA', '\u00DA', 'U', 'U', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00FA', '\u00FA', 'u', 'u', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00D9', '\u00D9', 'U', 'U', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00F9', '\u00F9', 'u', 'u', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00DC', '\u00DC', 'U', 'UE', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00FC', '\u00FC', 'u', 'ue', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u016E', '\u016E', 'U', 'UU', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u016F', '\u016F', 'u', 'uu', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u01AF', '\u01AF', 'U', 'U', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u01B0', '\u01B0', 'u', 'u', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u1EE8', '\u1EE8', 'U', 'U', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u1EE9', '\u1EE9', 'u', 'u', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00DD', '\u00DD', 'Y', 'Y', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00FD', '\u00FD', 'y', 'y', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u1EF2', '\u1EF2', 'Y', 'Y', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u1EF3', '\u1EF3', 'y', 'y', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u0178', '\u0178', 'Y', 'Y', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u00FF', '\u00FF', 'y', 'y', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u017B', '\u017B', 'Z', 'Z', '')
INSERT dbo.KeywordGeneratorDiacriticsReplacements (FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4) VALUES('\u017C', '\u017C', 'z', 'z', '')


GO


-- END OF THIS FILE
