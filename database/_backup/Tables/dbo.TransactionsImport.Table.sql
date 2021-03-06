/****** Object:  Table [dbo].[TransactionsImport]    Script Date: 29/04/2021 10:45:58 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TransactionsImport]') AND type in (N'U'))
DROP TABLE [dbo].[TransactionsImport]
GO

/****** Object:  Table [dbo].[TransactionsImport]    Script Date: 29/04/2021 10:45:58 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TransactionsImport](
	[_time] [nvarchar](1000) NULL,
	[sspFrom] [nvarchar](1000) NULL,
	[sspTo] [nvarchar](1000) NULL,
	[sspTraceId] [nvarchar](1000) NULL,
	[interaction] [nvarchar](1000) NULL,
	[responseCode] [nvarchar](1000) NULL,
	[duration] [nvarchar](1000) NULL,
	[responseSize] [nvarchar](1000) NULL,
	[responseErrorMessage] [nvarchar](1000) NULL,
	[method] [nvarchar](1000) NULL
) ON [PRIMARY]
GO