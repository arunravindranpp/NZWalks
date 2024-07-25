-- CAU_SanctionsContact.sql
-- 2024-06-19 12:10 UTC

USE PACETechRefresh_UAT
-- USE PACE4
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


IF OBJECT_ID('dbo.CAU_SanctionsContact', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.CAU_SanctionsContact
END
GO

CREATE TABLE [dbo].[CAU_SanctionsContact] (
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Area] [nvarchar](100) NULL,
	[Region] [nvarchar](100) NULL,
	[Country] [nvarchar](180) NULL,
	[WFAR_Area] [nvarchar](100) NULL,
	[WFAR_Region] [nvarchar](100) NULL,
	[WFAR_Country] [nvarchar](180) NULL,
    [GCOContact] [nvarchar](255) NULL,
	[RMContact] [nvarchar](255) NULL

	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'Canada', 'Canada', 'Americas', 'Canada', 'Canada', 'Charmaine Chung', 'Danielle Abbott, Liz Kiss')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'Israel', 'Israel', 'Americas', 'Israel', 'Israel', 'N/A', 'Galit Niv')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'Latam North', 'Bolivia', 'Americas', 'LATAM', 'Bolivia', 'Roberto Godoy', 'Analia Brunet')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'Latam North', 'Colombia', 'Americas', 'LATAM', 'Colombia', 'Roberto Godoy', 'Analia Brunet')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'Latam North', 'Costa Rica', 'Americas', 'LATAM', 'Costa Rica', 'Roberto Godoy', 'Analia Brunet')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'Latam North', 'Dominican Republic', 'Americas', 'LATAM', 'Dominican Republic', 'Roberto Godoy', 'Analia Brunet')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'Latam North', 'Ecuador', 'Americas', 'LATAM', 'Ecuador', 'Roberto Godoy', 'Analia Brunet')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'Latam North', 'El Salvador', 'Americas', 'LATAM', 'El Salvador', 'Roberto Godoy', 'Analia Brunet')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'Latam North', 'Guatemala', 'Americas', 'LATAM', 'Guatemala', 'Roberto Godoy', 'Analia Brunet')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'Latam North', 'Honduras', 'Americas', 'LATAM', 'Honduras', 'Roberto Godoy', 'Analia Brunet')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'Latam North', 'Mexico', 'Americas', 'LATAM', 'Mexico', 'Roberto Godoy', 'Analia Brunet')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'Latam North', 'Nicaragua', 'Americas', 'LATAM', 'Nicaragua', 'Roberto Godoy', 'Analia Brunet')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'Latam North', 'Panama', 'Americas', 'LATAM', 'Panama', 'Roberto Godoy', 'Analia Brunet')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'Latam North', 'Peru', 'Americas', 'LATAM', 'Peru', 'Roberto Godoy', 'Analia Brunet')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'Latam North', 'Venezuela', 'Americas', 'LATAM', 'Venezuela', 'Roberto Godoy', 'Analia Brunet')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'Latam South', 'Argentina', 'Americas', 'LATAM', 'Argentina', 'Roberto Godoy', 'Analia Brunet')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'Latam South', 'Brazil', 'Americas', 'LATAM', 'Brazil', 'Roberto Godoy', 'Alexandre Hoeppers, Analia Brunet')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'Latam South', 'Chile', 'Americas', 'LATAM', 'Chile', 'Roberto Godoy', 'Albert Oppenlander, Analia Brunet')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'Latam South', 'Paraguay', 'Americas', 'LATAM', 'Paraguay', 'Roberto Godoy', 'Analia Brunet, Rolando Castillo')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'Latam South', 'Uruguay', 'Americas', 'LATAM', 'Uruguay', 'Roberto Godoy', 'Marcelo Recagno, Analia Brunet')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'USA', 'USA', 'Americas', 'USA', 'United States of America', 'Josh Konvisser, Faizan Tukdi', 'Marta Skuza, Robb Canning , Abby V Boyd, USSanctions@ey.com')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'BBC', 'All countries', 'Americas', 'US-Central', 'All countries', 'Andre Mon Desir', 'Jamaine McFall')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'EY Caribbean', 'All countries', 'Americas', 'US-Central', 'All countries', 'Andre Mon Desir', 'Peter Gittens')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'ASEAN', 'All countries', 'Asia-Pac', 'ASEAN', 'All countries', 'Ahmad Khalid Abdullah Sani, Lai Hing Chan', 'Susanti Susanti, Mayday B Cypres')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'ASEAN', 'Singapore', 'Asia-Pac', 'ASEAN', 'Singapore', 'Russell Pereira', 'Veronica Ng, Joleen Tay')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'ASEAN', 'Brunei', 'Asia-Pac', 'ASEAN', 'Brunei', 'Lai Hing Chan', 'Veronica Ng, Joleen Tay')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'ASEAN', 'Myanmar', 'Asia-Pac', 'ASEAN', 'Myanmar', 'Lai Hing Chan', 'Veronica Ng, Joleen Tay')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'ASEAN', 'Vietnam, Cambodia and Laos', 'Asia-Pac', 'ASEAN', 'Vietnam, Cambodia and Laos', 'Anh Nguyen Pham', 'Ernest Chin-Kang Yoong')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'ASEAN', 'Guam, Saipan', 'Asia-Pac', 'ASEAN', 'Guam, Saipan', 'Lai Hing Chan', 'Nino E Aquino')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'ASEAN', 'Indonesia', 'Asia-Pac', 'ASEAN', 'Indonesia', 'Ary Prasetyo', 'Susanti Susanti')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'ASEAN', 'Malaysia', 'Asia-Pac', 'ASEAN', 'Malaysia', 'Anita Mary Fernandez', 'Raymond Cheong')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'ASEAN', 'Sri Lanka, Maldives', 'Asia-Pac', 'ASEAN', 'Sri Lanka, Maldives', 'Lai Hing Chan, Sajie Perera', 'Aasiri Gunasekera')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'ASEAN', 'Philippines', 'Asia-Pac', 'ASEAN', 'Philippines', 'Carolina Racelis', 'Lucy L Chan')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'ASEAN', 'Thailand', 'Asia-Pac', 'ASEAN', 'Thailand', 'Karnjana Sangchai, Siriphatsorn Angsuwan', 'Saifon Inkaew')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('Americas', 'Japan', 'Japan', 'Japan', 'Japan', 'Japan', 'japan.gco.sanction@jp.ey.com', 'riskmanagementjapan@jp.ey.com')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('AsiaPac', 'Greater China', 'All countries', 'Asia-Pac', 'Greater China', 'All countries', 'Adrienne Chan, Tommy KF Chan, gcosanctions@hk.ey.com', 'Alden Leung, Jasmine WH So')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('AsiaPac', 'Korea', 'Korea', 'Asia-Pac', 'Korea', 'Korea', 'Hani.Kim@kr.ey.com', 'Dong Guen Lee, Ga Eun Lee')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('AsiaPac', 'Oceania', 'All countries', 'Asia-Pac', 'Oceania', 'All countries', 'Stephan Van Der Walt, Michelle Smyth', 'John Gonsalves')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Africa', 'All countries', 'EMEIA', 'Africa', 'All countries', 'Kerry Kleinhans', 'Rubeshne Gobardan')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Africa', 'Albania/Kosovo/North Macedonia', 'EMEIA', 'CESA', 'Albania/Kosovo/North Macedonia', 'Binnaz Ramadan, Krzysztof Kwieciński', 'Mitko H Stoykov')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Africa', 'Greece', 'EMEIA', 'CESA', 'Greece', 'Ioli Katsiroumpa, Alexandra Vraka, Asteria Kalamara, Aggeliki Laopodi, Krzysztof Kwieciński', 'Mitko H Stoykov, Evgenia Kousathana')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Africa', 'Baltics (Estonia/ Latvia/ Lithuania)', 'EMEIA', 'CESA', 'Baltics (Estonia/ Latvia/ Lithuania)', 'Jone Sabaitiene, Krzysztof Kwieciński', 'Mitko H Stoykov')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Africa', 'Cyprus', 'EMEIA', 'CESA', 'Cyprus', 'Natassa Kiliari, Evyenia Epaminondou, Krzysztof Kwieciński', 'Mitko H Stoykov')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Africa', 'Czech Republic', 'EMEIA', 'CESA', 'Czech Republic', 'Gabriela Grezlova, Krzysztof Kwieciński', 'Mitko H Stoykov')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Africa', 'Romania/Moldova', 'EMEIA', 'CESA', 'Romania/Moldova', 'Geanina Tache, Catalina Girleanu, Krzysztof Kwieciński', 'Mitko H Stoykov')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Africa', 'Poland', 'EMEIA', 'CESA', 'Poland', 'Katarzyna Fidala, Łukasz Obrębski, Michał Mąkosa, Zuzanna Gorbacz, Karolina Gorzała, Sonia Podgórska, Hubert Zalewski, Marek Szulc, Magda Grzywacz, Kinga P Kucharska, Dominika Hajduk, Krzysztof Kwieciński', 'Mitko H Stoykov')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Africa', 'Turkey', 'EMEIA', 'CESA', 'Turkey', 'Oguz Tarhan, Hande Mahmutoglu, Perin İncili, Beste Gul, Krzysztof Kwieciński', 'Mitko H Stoykov')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'CESA', 'Bulgaria', 'EMEIA', 'CESA', 'Bulgaria', 'Binnaz Ramadan, Krzysztof Kwieciński', 'Mitko H Stoykov')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'CESA', 'Hungary', 'EMEIA', 'CESA', 'Hungary', 'Irisz Szel, Daniel Kokas, Zsofia Barat, Gabriella Molnar, Krzysztof Kwieciński', 'Mitko H Stoykov')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'CESA', 'Croatia / Slovenia', 'EMEIA', 'CESA', 'Croatia / Slovenia', 'Ana Baric, Krzysztof Kwieciński', 'Mitko H Stoykov')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'CESA', 'Slovak Republic', 'EMEIA', 'CESA', 'Slovak Republic', 'Patricia Laky, Nina Hanečka, Krzysztof Kwieciński', 'Mitko H Stoykov')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'CESA', 'Bosnia & Herzegovina/ Montenegro/Serbia', 'EMEIA', 'CESA', 'Bosnia and Herzegovina/Bosnia & Herzegovina/ Montenegro/Serbia', 'Mirko Kovac, Krzysztof Kwieciński', 'Mitko H Stoykov')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'CESA', 'Azerbaijan', 'EMEIA', 'CESA', 'Azerbaijan', 'Konul B Bayramova', 'Mitko H Stoykov')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'CESA', 'Ukraine', 'EMEIA', 'CESA', 'Ukraine', 'Iryna V Filipchuk', 'Mitko H Stoykov')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'CESA', 'Georgia', 'EMEIA', 'CESA', 'Georgia', 'Giorgi Mirtskhulava', 'Mitko H Stoykov')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'CESA', 'Armenia', 'EMEIA', 'CESA', 'Armenia', 'Lilit Shahinyan', 'Mitko H Stoykov')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'CESA', 'Kazakhstan', 'EMEIA', 'CESA', 'Kazakhstan', 'Alexandr V Solyanko', 'Mitko H Stoykov')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'CESA', 'Uzbekistan', 'EMEIA', 'CESA', 'Uzbekistan', 'Farrukh Khudayberdiev', 'Mitko H Stoykov')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'FSO', 'FSO', 'EMEIA', 'FSO', 'FSO', 'Vivek Prashar', 'Robert Dobronevskij')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'FSO', 'Luxembourg', 'EMEIA', 'Europe west', 'Luxembourg', 'Raynald Laverny', 'Fabien Telmat')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'India', 'India', 'EMEIA', 'India', 'India', 'Rohit Gupta (Rohit19.Gupta@in.ey.com), Sujatha Balachander (Sujatha.Balachander@in.ey.com)', 'Manmat Surana')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'MENA', 'All countries', 'EMEIA', 'MENA', 'All countries', 'Daoud Moukheiber', 'asksanctionteam@ae.ey.com, Michelle McAloon')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Nordics', 'All countries', 'EMEIA', 'Nordics', 'All countries', 'Andreas Fahlen', 'asksanctionteam@ae.ey.com, Michelle McAloon')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'UK&I', 'All countries', 'EMEIA', 'UKI', 'All countries', 'financialcrime@uk.ey.com', 'Dave Bickley, Colin Pickard')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Europe west', 'All countries', 'EMEIA', 'Europe west', 'All countries', 'europewest.sanctions@nl.ey.com', 'Dave Bickley, Colin Pickard')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Europe west', 'Germany', 'EMEIA', 'Europe west', 'Germany', 'Jana Werling', 'Andre Fedorchenko, Anke Rosenfeld')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Europe west', 'Switzerland', 'EMEIA', 'Europe west', 'Switzerland', 'Hilary von Arx', 'qrm.infodesk@ch.ey.com')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Europe west', 'Austria', 'EMEIA', 'Europe west', 'Austria', 'Miriam Schwab, Gabriele Lechner', 'Karl Fuchs')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Europe west', 'Belgium', 'EMEIA', 'Europe west', 'Belgium', 'Kristof Lefever', 'Petra Aerts')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Europe west', 'France', 'EMEIA', 'Europe west', 'France', 'Charlotte Lance, Romain Stimpfling', 'Stephanie Prades-nihoul, RF.Analysts@fr.ey.com')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Europe west', 'FSSA', 'EMEIA', 'Europe west', 'FSSA', 'Hermance Bouba', 'Christelle-Tatiana Onanga Bouyou')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Europe west', 'Netherlands', 'EMEIA', 'Europe west', 'Netherlands', 'Joyce Deriga', 'Aleksandra Vasilic')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Europe west', 'Italy', 'EMEIA', 'Europe west', 'Italy', 'Luigi Neirotti', 'Angelo Tresoldi')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Europe west', 'Spain', 'EMEIA', 'Europe west', 'Spain', 'Marta Acevedo Ocaña', 'Pedro Rodríguez')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Europe west', 'Portugal', 'EMEIA', 'Europe west', 'Portugal', 'Antonio Garcia Pereira, Margarida F. Garcia', 'Fátima Freitas, Daniel Guerreiro')
INSERT INTO dbo.CAU_SanctionsContact (Area, Region, Country, WFAR_Area, WFAR_Region, WFAR_Country, GCOContact, RMContact) VALUES ('EMEIA', 'Europe west', 'Tunisia', 'EMEIA', 'Europe west', 'Tunisia', 'Anis Laadhar', 'Anis Laadhar')


-- END OF THIS FILE
