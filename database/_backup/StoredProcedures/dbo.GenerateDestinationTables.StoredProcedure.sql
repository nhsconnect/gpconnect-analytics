/****** Object:  StoredProcedure [dbo].[GenerateDestinationTables]    Script Date: 27/04/2021 15:00:30 ******/
DROP PROCEDURE IF EXISTS [dbo].[GenerateDestinationTables]
GO
/****** Object:  StoredProcedure [dbo].[GenerateDestinationTables]    Script Date: 27/04/2021 15:00:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[GenerateDestinationTables]
	@StartDate DATE
AS
BEGIN	
	DECLARE @EndDate DATE = GETDATE()

	;WITH cte AS 
	(
		SELECT dt = DATEADD(DAY, -(DAY(@StartDate) - 1), @StartDate)
		UNION ALL
		SELECT DATEADD(MONTH, 1, dt)
		FROM cte
		WHERE dt < DATEADD(DAY, -(DAY(@EndDate) - 1), @EndDate)
	)
	SELECT 
		ROW_NUMBER() OVER (ORDER BY dt) AS RowNumber,
		'IF NOT EXISTS (SELECT * FROM sysobjects WHERE name=''Transactions' + CONVERT(CHAR(3), dt, 100) + CONVERT(CHAR(4), dt, 120) + ''' AND xtype=''U'') ' + CHAR(13) +
		'CREATE TABLE [dbo].[Transactions' + CONVERT(CHAR(3), dt, 100) + CONVERT(CHAR(4), dt, 120) + '] ' + CHAR(13) +
		'([Id] [int] IDENTITY(1,1) NOT NULL, ' + CHAR(13) +
		'[_time] [datetime] NULL, ' + CHAR(13) +
		'[sspFrom] [nvarchar](1000) NULL, ' + CHAR(13) +
		'[fromOrg] [nvarchar](1000) NULL, ' + CHAR(13) +
		'[fromSupplier] [nvarchar](1000) NULL, ' + CHAR(13) +
		'[sspTo] [nvarchar](1000) NULL, ' + CHAR(13) +
		'[toOrg] [nvarchar](1000) NULL, ' + CHAR(13) +
		'[toSupplier] [nvarchar](1000) NULL, ' + CHAR(13) +
		'[interaction] [nvarchar](1000) NULL, ' + CHAR(13) +
		'[responseCode] [int] NULL, ' + CHAR(13) +
		'[duration] [decimal](8, 3) NULL, ' + CHAR(13) +
		'[responseSize] [int] NULL, ' + CHAR(13) +
		'[responseErrorMessage] [nvarchar](1000) NULL, ' + CHAR(13) +
		'[method] [nvarchar](1000) NULL, ' + CHAR(13) +
		'PRIMARY KEY CLUSTERED ([Id] ASC) ' + CHAR(13) +
		'WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ' + CHAR(13) +
		'ON [PRIMARY]) ON [PRIMARY]; ' AS SqlCommand		
	INTO #Temp
	FROM cte;
	
	DECLARE @StartRowCounter INT = 1
	DECLARE @EndRowCounter INT = (SELECT MAX(RowNumber) FROM #Temp)	
	DECLARE @SQLString VARCHAR(MAX) = ''

	WHILE @StartRowCounter <= @EndRowCounter
	BEGIN
		SELECT @SQLString = SqlCommand FROM #Temp WHERE RowNumber = @StartRowCounter
		EXEC(@SQLString)
		SELECT @StartRowCounter = @StartRowCounter + 1
	END
END

GO
