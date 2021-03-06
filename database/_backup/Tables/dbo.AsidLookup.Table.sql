/****** Object:  Table [dbo].[OdsLookup]    Script Date: 29/04/2021 10:25:36 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AsidLookup]') AND type in (N'U'))
DROP TABLE [dbo].[AsidLookup]
GO

/****** Object:  Table [dbo].[OdsLookup]    Script Date: 29/04/2021 10:25:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AsidLookup](
	[ASID] [varchar](1000) NULL,
	[MName] [varchar](1000) NULL,
	[NACS] [varchar](1000) NULL,
	[OrgName] [varchar](1000) NULL,
	[OrgType] [varchar](1000) NULL,
	[PName] [varchar](1000) NULL,
	[PostCode] [varchar](1000) NULL
) ON [PRIMARY]
GO