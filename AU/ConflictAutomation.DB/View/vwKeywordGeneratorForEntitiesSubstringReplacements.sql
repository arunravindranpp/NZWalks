-- vwKeywordGeneratorForEntitiesSubstringReplacements.sql
-- 2024-01-09 17:52 UTC

USE PACETechRefresh_UAT
GO

IF OBJECT_ID('dbo.vwKeywordGeneratorForEntitiesSubstringReplacements', 'V') IS NOT NULL
BEGIN
    DROP VIEW dbo.vwKeywordGeneratorForEntitiesSubstringReplacements
END
GO

CREATE VIEW dbo.vwKeywordGeneratorForEntitiesSubstringReplacements AS 
	SELECT FromValue, ToValue FROM dbo.KeywordGeneratorSubstringReplacements WHERE KeywordGeneratorType = 'E'
GO


-- END OF THIS FILE
