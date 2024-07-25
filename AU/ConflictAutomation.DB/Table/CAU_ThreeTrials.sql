
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF object_id('CAU_ThreeTrials') is null
BEGIN
CREATE TABLE [dbo].[CAU_ThreeTrials](
	[LogId] [bigint] IDENTITY(1,1) NOT NULL,
	[ConflictCheckID] [bigint] NULL,
	[TimeOfOccurence] [datetime] NULL,
	[Recurrence] int NULL,
	[ErrorMessage] [nvarchar](max) NULL,
	[Environment] [nvarchar](10) NULL,
PRIMARY KEY CLUSTERED 
(
	[LogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END