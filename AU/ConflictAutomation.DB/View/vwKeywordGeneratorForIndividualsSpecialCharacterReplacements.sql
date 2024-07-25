-- vwKeywordGeneratorForIndividualsSpecialCharacterReplacements.sql
-- 2024-01-09 20:44 UTC

USE PACETechRefresh_UAT
GO

IF OBJECT_ID('dbo.vwKeywordGeneratorForIndividualsSpecialCharacterReplacements', 'V') IS NOT NULL
BEGIN
    DROP VIEW dbo.vwKeywordGeneratorForIndividualsSpecialCharacterReplacements
END
GO

CREATE VIEW dbo.vwKeywordGeneratorForIndividualsSpecialCharacterReplacements AS 
	SELECT FromValue, ToValue FROM dbo.KeywordGeneratorSpecialCharacterReplacements WHERE KeywordGeneratorType = 'I'
GO


-- END OF THIS FILE
