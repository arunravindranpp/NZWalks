-- vwKeywordGeneratorForEntitiesGeoLocationNames.sql
-- 2024-01-09 19:52 UTC

USE PACETechRefresh_UAT
GO

IF OBJECT_ID('dbo.vwKeywordGeneratorForEntitiesGeoLocationNames', 'V') IS NOT NULL
BEGIN
    DROP VIEW dbo.vwKeywordGeneratorForEntitiesGeoLocationNames
END
GO

CREATE VIEW dbo.vwKeywordGeneratorForEntitiesGeoLocationNames AS 
	SELECT FromValue FROM dbo.KeywordGeneratorGeoLocationNames WHERE KeywordGeneratorType = 'E'
GO


-- END OF THIS FILE
