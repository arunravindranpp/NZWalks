
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF object_id('CAU_SkipCountries') is null
BEGIN
CREATE TABLE [dbo].[CAU_SkipCountries](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CountryCode] [nvarchar](10) NULL,
	[Country] [nvarchar](250) NULL,
	[SSLName] NVARCHAR(100) NULL
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] 

Truncate Table [dbo].[CAU_SkipCountries]
INSERT INTO [dbo].[CAU_SkipCountries] ([CountryCode], [Country],[SSLName])
VALUES
    ('ISR', 'Israel', NULL),
    ('SDN', 'Sudan', NULL),
    ('IRN', 'Iran', NULL),
	('SYR', 'Syria', NULL),
	('RUS', 'Russia', NULL),
	('BLR', 'Belarus', NULL),
    ('NLD', 'Netherlands','Law');
END

 