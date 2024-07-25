-- vwKeywordGeneratorForEntitiesSpecialCharacterReplacements.sql
-- 2024-01-09 18:50 UTC

USE PACETechRefresh_UAT
GO

IF OBJECT_ID('dbo.vwKeywordGeneratorForEntitiesSpecialCharacterReplacements', 'V') IS NOT NULL
BEGIN
    DROP VIEW dbo.vwKeywordGeneratorForEntitiesSpecialCharacterReplacements
END
GO

CREATE VIEW dbo.vwKeywordGeneratorForEntitiesSpecialCharacterReplacements AS 
	SELECT FromValue, ToValue FROM dbo.KeywordGeneratorSpecialCharacterReplacements WHERE KeywordGeneratorType = 'E'
GO


-- END OF THIS FILE
