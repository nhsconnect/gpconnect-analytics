/****** Object:  Table [dbo].[OrganisationHierarchy]    Script Date: 27/04/2021 15:00:30 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrganisationHierarchy]') AND type in (N'U'))
DROP TABLE [dbo].[OrganisationHierarchy]
GO
/****** Object:  Table [dbo].[OrganisationHierarchy]    Script Date: 27/04/2021 15:00:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrganisationHierarchy](
	[OdsCode] [varchar](500) NULL,
	[PracticeName] [varchar](500) NULL,
	[RegisteredPatients] [int] NULL,
	[Region] [varchar](500) NULL,
	[STP] [varchar](500) NULL,
	[CCG] [varchar](500) NULL
) ON [PRIMARY]
GO
