-- FN_GET_GCO_TEAM_BY_COUNTRY_NAME.sql
-- 2024-06-18-18-07UTC

USE PACETechRefresh_UAT
-- USE PACE4
GO

CREATE OR ALTER FUNCTION [dbo].[FN_GET_GCO_TEAM_BY_COUNTRY_NAME]
(	
	@COUNTRY_DESCRIPTION	NVARCHAR(180)
)
RETURNS NVARCHAR(255)
AS
BEGIN
	DECLARE @RESULT	NVARCHAR(255)
	
	-- 1st Try: look for the exact country name in CAU_SanctionsContact.WFAR_Country
	SELECT TOP 1 @RESULT = GCOContact FROM CAU_SanctionsContact WHERE WFAR_Country = @COUNTRY_DESCRIPTION
	IF(ISNULL(@RESULT, '') <> '') BEGIN
		RETURN ISNULL(@RESULT, '')
	END

	-- 2nd Try: look for the country name as a substring inside CAU_SanctionsContact.WFAR_Country
	SELECT TOP 1 @RESULT = GCOContact FROM CAU_SanctionsContact WHERE WFAR_Country LIKE '%' + @COUNTRY_DESCRIPTION + '%'
	IF(ISNULL(@RESULT, '') <> '') BEGIN
		RETURN ISNULL(@RESULT, '')
	END
	   
	-- 3rd Try: look for 'All countries' in CAU_SanctionsContact.Country for the corresponding Area/Region
	
		-- 3.1. Get the @COUNTRY_CODE
		DECLARE @COUNTRY_CODE NVARCHAR(18)
		SELECT TOP 1 @COUNTRY_CODE = COUNTRY_CODE FROM vwCOUNTRY_LIST WHERE COUNTRY_DESCRIPTION = @COUNTRY_DESCRIPTION
		IF(ISNULL(@COUNTRY_CODE, '') = '') BEGIN
			RETURN ''
		END

		-- 3.2. Get the @AREA and @REGION
		DECLARE @AREA NVARCHAR(100)
		DECLARE @REGION NVARCHAR(100)
		SELECT TOP 1 @AREA = Area, @REGION = Region FROM WF_AreaRegion WHERE CountryCode = @COUNTRY_CODE AND Area NOT LIKE '%NOT AVAIL%' AND Region NOT LIKE '%FSO%'
		IF((ISNULL(@AREA, '') = '') OR (ISNULL(@REGION, '') = '')) BEGIN
			RETURN ''
		END

		-- 3.3. Get the GCOContact corresponding to "All countries" in the corresponding @AREA and @REGION
		SELECT @RESULT = GCOContact FROM CAU_SanctionsContact WHERE WFAR_Area = @AREA AND WFAR_Region = @REGION AND WFAR_Country = 'All countries'
		
		RETURN ISNULL(@RESULT, '')
	END


-- END OF THIS FILE
