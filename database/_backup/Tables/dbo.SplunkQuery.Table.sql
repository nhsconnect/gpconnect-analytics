/****** Object:  Table [dbo].[SplunkQuery]    Script Date: 27/04/2021 15:00:30 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SplunkQuery]') AND type in (N'U'))
DROP TABLE [dbo].[SplunkQuery]
GO
/****** Object:  Table [dbo].[SplunkQuery]    Script Date: 27/04/2021 15:00:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SplunkQuery](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[QueryName] [varchar](50) NOT NULL,
	[QueryValue] [varchar](1000) NOT NULL
) ON [PRIMARY]
GO
