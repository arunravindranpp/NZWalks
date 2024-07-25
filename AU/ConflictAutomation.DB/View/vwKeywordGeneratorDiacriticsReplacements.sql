-- vwKeywordGeneratorDiacriticsReplacements.sql
-- 2024-01-18 11:20 UTC

USE PACETechRefresh_UAT
GO

IF OBJECT_ID('dbo.vwKeywordGeneratorDiacriticsReplacements', 'V') IS NOT NULL
BEGIN
    DROP VIEW dbo.vwKeywordGeneratorDiacriticsReplacements
END
GO

CREATE VIEW dbo.vwKeywordGeneratorDiacriticsReplacements AS 
	SELECT	FromValue, ToValue_Step_1, ToValue_Step_2, ToValue_Step_3, ToValue_Step_4
	FROM	dbo.KeywordGeneratorDiacriticsReplacements
GO


-- END OF THIS FILE
