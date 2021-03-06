/****** Object:  Table [dbo].[SplunkTransactionsImportControl]    Script Date: 27/04/2021 15:00:30 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SplunkTransactionsImportControl]') AND type in (N'U'))
DROP TABLE [dbo].[SplunkTransactionsImportControl]
GO
/****** Object:  Table [dbo].[SplunkTransactionsImportControl]    Script Date: 27/04/2021 15:00:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SplunkTransactionsImportControl](
	[StartDate] [datetime] NULL,
	[LastExecutionDate] [datetime] NULL,
	[FilterThresholdInDays] [int] NULL
) ON [PRIMARY]
GO

INSERT INTO [dbo].[SplunkTransactionsImportControl]
           ([StartDate]
           ,[LastExecutionDate]
           ,[FilterThresholdInDays])
     VALUES
           ('2018-12-01 00:00:00.000'
           ,NULL
           ,NULL)
GO