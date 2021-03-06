/****** Object:  Table [dbo].[SplunkTransactionsImportHistory]    Script Date: 27/04/2021 15:00:30 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SplunkTransactionsImportHistory]') AND type in (N'U'))
DROP TABLE [dbo].[SplunkTransactionsImportHistory]
GO
/****** Object:  Table [dbo].[SplunkTransactionsImportHistory]    Script Date: 27/04/2021 15:00:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SplunkTransactionsImportHistory](
	[ImportDate] [datetime] NOT NULL,
	[EmailSender] [varchar](255) NOT NULL,
	[EmailSubject] [varchar](255) NOT NULL,
	[EmailSent] [datetime] NOT NULL,
	[AttachmentFilename] [varchar](255) NOT NULL,
	[AttachmentRowCount] [int] NOT NULL,
	[AttachmentStartRow] [varchar](2000) NOT NULL,
	[AttachmentEndRow] [varchar](2000) NOT NULL,
	[ImportRowCount] [int] NOT NULL,
	[ImportStartRow] [varchar](2000) NOT NULL,
	[ImportEndRow] [varchar](2000) NOT NULL
) ON [PRIMARY]
GO
