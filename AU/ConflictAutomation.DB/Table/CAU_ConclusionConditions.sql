
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF object_id('CAU_ConclusionConditions') is null
BEGIN
CREATE TABLE [dbo].[CAU_ConclusionConditions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Cases] [int] NOT NULL,
	[Condition1] [nvarchar](max) NULL,
	[Condition2] [nvarchar](max) NULL,
	[ConflictConclusion] [nvarchar](max) NULL,
	[DisclaimerIndependence] [nvarchar](max) NULL,
	[RationaleInstructions] [nvarchar](max) NULL
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END

Truncate Table CAU_ConclusionConditions
GO
INSERT [dbo].[CAU_ConclusionConditions] ([Cases], [Condition1], [Condition2], [ConflictConclusion], [DisclaimerIndependence],[RationaleInstructions])
VALUES 
(1, N'NA', N'NA', N'No conflict of interest has been found which precludes the acceptance of this engagement.', N'This clearance only addresses Conflicts considerations. No independence issues regarding the scope permissibility have been considered.', N'')

GO
INSERT [dbo].[CAU_ConclusionConditions] ([Cases], [Condition1], [Condition2], [ConflictConclusion], [DisclaimerIndependence],[RationaleInstructions])
VALUES 
(2, N'NA', N'NA', N'No conflict of interest has been found which precludes the acceptance of this engagement.', N'This clearance only addresses Conflicts considerations. No independence issues regarding the scope permissibility have been considered.', N'Since <xxx> is subject to economic or trade sanctions, kindly liaise with the local GCO team <GCO name> and Local RM contact <RM contact name> with respect to the feasibility of acceptance of this engagement (if not already done). We have attached a match report in the conclusion for your reference.')


INSERT [dbo].[CAU_ConclusionConditions] ([Cases], [Condition1], [Condition2], [ConflictConclusion], [DisclaimerIndependence],[RationaleInstructions])
VALUES 
(3, N'NA', N'NA', N'No conflict of interest has been found which precludes the acceptance of this engagement.',N'This clearance only addresses Conflicts considerations. No independence issues regarding the scope permissibility have been considered.', N'Since <xxx> is subject to economic or trade sanctions, kindly liaise with the local GCO team <GCO name> and Local RM contact <RM contact name> with respect to the feasibility of acceptance of this engagement . We have attached a match report in the conclusion for your reference.')

GO
INSERT [dbo].[CAU_ConclusionConditions] ([Cases], [Condition1], [Condition2], [ConflictConclusion], [DisclaimerIndependence],[RationaleInstructions])
VALUES 
(4, N'Please note that <xxx> entity is subject to economic or trade sanctions, hence, kindly liaise with the local GCO team <GCO name> and Local RM contact <RM contact name> with respect to the feasibility of acceptance of this engagement. We have attached a match report in the conclusion for your reference ', N'NA', N'No conflicts of interest have been identified which preclude the acceptance of this engagement. However, the condition(s) raised must be met prior to the acceptance of this engagement.', N'This clearance only addresses Conflicts considerations. No independence issues regarding the scope permissibility have been considered.', N'')
