-- vwKeywordGeneratorForIndividualsSubstringReplacements.sql
-- 2024-01-09 20:17 UTC

USE PACETechRefresh_UAT
GO

IF OBJECT_ID('dbo.vwKeywordGeneratorForIndividualsSubstringReplacements', 'V') IS NOT NULL
BEGIN
    DROP VIEW dbo.vwKeywordGeneratorForIndividualsSubstringReplacements
END
GO

CREATE VIEW dbo.vwKeywordGeneratorForIndividualsSubstringReplacements AS 
	SELECT FromValue, ToValue FROM dbo.KeywordGeneratorSubstringReplacements WHERE KeywordGeneratorType = 'I'
GO


-- END OF THIS FILE
