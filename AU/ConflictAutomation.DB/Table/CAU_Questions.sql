IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CAU_Questions]') AND type in (N'U'))
DROP TABLE [dbo].[CAU_Questions]
GO

/****** Object:  Table [dbo].[CAU_Questions]    Script Date: 24-01-2024 12:07:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CAU_Questions](
	[Title] [nvarchar](1000) NULL,
	[QuestionNumber] [nvarchar](1000) NULL
) ON [PRIMARY]
GO

GO
INSERT [dbo].[CAU_Questions] ([Title], [QuestionNumber]) VALUES (N'Auctioned', N'999')
GO
INSERT [dbo].[CAU_Questions] ([Title], [QuestionNumber]) VALUES (N'Contractually limit our ability', N'524136')
GO
INSERT [dbo].[CAU_Questions] ([Title], [QuestionNumber]) VALUES (N'Dispute/Litigation', N'790063')
GO
INSERT [dbo].[CAU_Questions] ([Title], [QuestionNumber]) VALUES (N'Government entity involved', N'748417')
GO
INSERT [dbo].[CAU_Questions] ([Title], [QuestionNumber]) VALUES (N'High Profile', N'515489')
GO
INSERT [dbo].[CAU_Questions] ([Title], [QuestionNumber]) VALUES (N'Hostile', N'041349')
GO
INSERT [dbo].[CAU_Questions] ([Title], [QuestionNumber]) VALUES (N'Hostile', N'597835')
GO
INSERT [dbo].[CAU_Questions] ([Title], [QuestionNumber]) VALUES (N'Pursuit Check Performed', N'988259')
GO
INSERT [dbo].[CAU_Questions] ([Title], [QuestionNumber]) VALUES (N'Sanctioned', N'784636')
GO
INSERT [dbo].[CAU_Questions] ([Title], [QuestionNumber]) VALUES (N'Sanctioned', N'840695')
GO
INSERT [dbo].[CAU_Questions] ([Title], [QuestionNumber]) VALUES (N'Whether to contact counterparty (G)CSP/audit partner?', N'813175')
GO
