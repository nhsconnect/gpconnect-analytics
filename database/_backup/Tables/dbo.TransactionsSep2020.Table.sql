/****** Object:  Table [dbo].[TransactionsSep2020]    Script Date: 27/04/2021 15:00:30 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TransactionsSep2020]') AND type in (N'U'))
DROP TABLE [dbo].[TransactionsSep2020]
GO
/****** Object:  Table [dbo].[TransactionsSep2020]    Script Date: 27/04/2021 15:00:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TransactionsSep2020](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[_time] [datetime] NULL,
	[sspFrom] [nvarchar](1000) NULL,
	[fromOrg] [nvarchar](1000) NULL,
	[fromSupplier] [nvarchar](1000) NULL,
	[sspTo] [nvarchar](1000) NULL,
	[toOrg] [nvarchar](1000) NULL,
	[toSupplier] [nvarchar](1000) NULL,
	[interaction] [nvarchar](1000) NULL,
	[responseCode] [int] NULL,
	[duration] [decimal](8, 3) NULL,
	[responseSize] [int] NULL,
	[responseErrorMessage] [nvarchar](1000) NULL,
	[method] [nvarchar](1000) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
