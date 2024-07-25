-- KeywordGeneratorGeoLocationNames.sql
-- 2024-01-09 19:57 UTC

USE PACETechRefresh_UAT
GO


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


IF OBJECT_ID('dbo.KeywordGeneratorGeoLocationNames', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.KeywordGeneratorGeoLocationNames
END
GO


CREATE TABLE dbo.KeywordGeneratorGeoLocationNames (
	ID                   INT     IDENTITY(1,1) NOT NULL,
	KeywordGeneratorType VARCHAR(3)            NOT NULL,
	FromValue            VARCHAR(255)          NOT NULL	
	
	CONSTRAINT PK_KeywordGeneratorGeoLocationNames PRIMARY KEY CLUSTERED 
		(ID ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY], 
	
	CONSTRAINT chk_GeoLocationNamesKeywordGeneratorType CHECK (KeywordGeneratorType IN ('E', 'I'))    -- 'E' = Entity    'I' = Individual
) ON [PRIMARY]
GO

INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'afghanistan')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'albania')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'algeria')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'andorra')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'angola')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'anguilla')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'antigua & barbuda')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'antigua')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'barbuda')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'argentina')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'armenia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'australia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'austria')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'azerbaijan')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'bahamas')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'bahrain')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'bangladesh')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'barbados')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'belarus')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'belgium')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'belize')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'benin')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'bermuda')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'bhutan')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'bolivia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'bosnia & herzegovina')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'bosnia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'herzegovina')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'botswana')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'brazil')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'brunei darussalam')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'bulgaria')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'burkina faso')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'myanmar/burma')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'burundi')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'cambodia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'cameroon')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'canada')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'cape verde')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'cayman islands')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'central african republic')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'chad')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'chile')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'china')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'colombia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'comoros')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'congo')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'costa rica')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'croatia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'cuba')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'cyprus')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'czech republic')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'democratic republic of the congo')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'denmark')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'djibouti')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'dominican republic')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'dominica')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'ecuador')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'egypt')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'el salvador')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'equatorial guinea')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'eritrea')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'estonia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'ethiopia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'fiji')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'finland')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'france')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'french guiana')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'gabon')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'gambia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'georgia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'germany')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'ghana')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'great britain')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'greece')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'grenada')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'guadeloupe')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'guatemala')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'guinea')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'guinea-bissau')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'guyana')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'haiti')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'honduras')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'hungary')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'iceland')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'india')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'indonesia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'iran')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'iraq')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'israel')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'italy')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'ivory coast (cote d''ivoire)')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'ivory coast')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'cote d''ivoire')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'jamaica')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'japan')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'jordan')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'kazakhstan')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'kenya')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'kosovo')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'kuwait')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'kyrgyz republic (kyrgyzstan)')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'kyrgyzstan')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'laos')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'latvia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'lebanon')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'lesotho')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'liberia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'libya')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'liechtenstein')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'lithuania')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'luxembourg')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'republic of macedonia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'macedonia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'madagascar')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'malawi')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'malaysia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'maldives')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'mali')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'malta')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'martinique')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'mauritania')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'mauritius')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'mayotte')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'mexico')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'moldova, republic of')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'republic of moldova')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'moldova')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'monaco')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'mongolia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'montenegro')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'montserrat')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'morocco')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'mozambique')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'namibia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'nepal')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'netherlands')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'new zealand')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'nicaragua')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'niger')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'nigeria')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'korea, democratic republic of (north korea)')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'north korea')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'korea')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'norway')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'oman')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'pacific islands')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'pakistan')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'panama')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'papua new guinea')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'paraguay')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'peru')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'philippines')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'poland')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'portugal')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'puerto rico')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'qatar')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'reunion')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'romania')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'russian federation')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'rwanda')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'saint kitts and nevis')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'saint kitts')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'st kitts')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'nevis')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'saint kitts & nevis')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'saint lucia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'saint vincent''s & grenadines')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'saint vincent')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'st vincent')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'grenadines')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'samoa')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'sao tome and principe')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'sao tome & principe')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'principe')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'sao tome')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'saudi arabia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'senegal')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'serbia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'seychelles')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'sierra leone')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'singapore')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'slovak republic (slovakia)')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'slovak republic')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'slovakia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'slovenia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'solomon islands')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'somalia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'south africa')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'korea, republic of (south korea)')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'south korea')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'south sudan')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'spain')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'sri lanka')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'sudan')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'suriname')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'swaziland')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'sweden')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'switzerland')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'syria')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'tajikistan')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'tanzania')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'thailand')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'timor leste')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'togo')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'trinidad & tobago')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'trinidad')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'tobago')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'tunisia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'turkey')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'turkmenistan')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'turks & caicos islands')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'turks')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'caicos')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'uganda')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'ukraine')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'united arab emirates')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'united states of america (usa)')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'usa')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'u.s.a.')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'united states')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'uruguay')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'uzbekistan')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'venezuela')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'vietnam')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'virgin islands')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'virgin islands (uk)')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'virgin islands (us)')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'yemen')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'zambia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'zimbabwe')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'united kingdom')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'ireland')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'alabama')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'alaska')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'arizona')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'arkansas')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'california')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'colorado')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'connecticut')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'delaware')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'florida')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'georgia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'hawaii')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'idaho')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'illinois')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'indiana')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'iowa')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'kansas')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'kentucky')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'louisiana')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'maine')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'maryland')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'massachusetts')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'michigan')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'minnesota')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'mississippi')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'missouri')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'montana')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'nebraska')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'nevada')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'new hampshire')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'new jersey')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'new mexico')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'new york')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'newyork')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'north carolina')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'north dakota')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'ohio')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'oklahoma')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'oregon')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'pennsylvania')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'rhode island')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'south carolina')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'south dakota')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'tennessee')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'texas')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'utah')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'vermont')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'virginia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'washington')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'west virginia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'wisconsin')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'wyoming')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'alberta')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'british columbia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'manitoba')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'new brunswick')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'newfoundland and labrador')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'newfoundland & labrador')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'newfoundland')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'labrador')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'northwest territories')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'nova scotia')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'nunavut')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'ontario')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'prince edward island')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'quebec')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'saskatchewan')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'yukon territory')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'yukon')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'bihar')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'hyderabad')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'maharastra')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'delhi')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'ahmedabad')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'pune')
INSERT dbo.KeywordGeneratorGeoLocationNames (KeywordGeneratorType, FromValue) VALUES('E', 'bangalore')
GO


-- END OF THIS FILE