/****** Object:  Table [dbo].[BlobStorageConfiguration]    Script Date: 29/04/2021 14:52:05 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BlobStorageConfiguration]') AND type in (N'U'))
DROP TABLE [dbo].[BlobStorageConfiguration]
GO

/****** Object:  Table [dbo].[BlobStorageConfiguration]    Script Date: 29/04/2021 14:52:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BlobStorageConfiguration](
	[PrimaryKey] [varchar](1000) NOT NULL,
	[ConnectionString] [varchar](1000) NOT NULL,
	[ContainerName] [varchar](255) NOT NULL,
	[ContainerLocationUrl] [varchar](255) NULL
) ON [PRIMARY]
GO

