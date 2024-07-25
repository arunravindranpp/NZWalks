-- vwKeywordGeneratorForEntitiesPrefixRemovals.sql
-- 2024-01-09 19:2 UTC

USE PACETechRefresh_UAT
GO

IF OBJECT_ID('dbo.vwKeywordGeneratorForEntitiesPrefixRemovals', 'V') IS NOT NULL
BEGIN
    DROP VIEW dbo.vwKeywordGeneratorForEntitiesPrefixRemovals
END
GO

CREATE VIEW dbo.vwKeywordGeneratorForEntitiesPrefixRemovals AS 
	SELECT FromValue FROM dbo.KeywordGeneratorPrefixRemovals WHERE KeywordGeneratorType = 'E'
GO


-- END OF THIS FILE
