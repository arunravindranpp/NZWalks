IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CAU_ConflictLookup]') AND type in (N'U'))
DROP TABLE [dbo].[CAU_ConflictLookup]
GO

/****** Object:  Table [dbo].[CAU_ConflictLookup]    Script Date: 24-01-2024 12:06:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CAU_ConflictLookup](
	[ConflictCheckID] [bigint] NULL
) ON [PRIMARY]
GO


