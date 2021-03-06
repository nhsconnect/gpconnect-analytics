/****** Object:  StoredProcedure [dbo].[GenerateDestinationViews]    Script Date: 27/04/2021 15:00:30 ******/
DROP PROCEDURE IF EXISTS [dbo].[GenerateDestinationViews]
GO
/****** Object:  StoredProcedure [dbo].[GenerateDestinationViews]    Script Date: 27/04/2021 15:00:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GenerateDestinationViews]
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
		'DROP VIEW IF EXISTS TransactionsView' + CONVERT(CHAR(3), dt, 100) + CONVERT(CHAR(4), dt, 120) AS SqlDropCommand,
		'CREATE VIEW [dbo].[TransactionsView' + CONVERT(CHAR(3), dt, 100) + CONVERT(CHAR(4), dt, 120) + '] AS ' + CHAR(13) +
		'SELECT T.*, ' + CHAR(13) +
		'FORMAT(CAST(T._time AS DATE), ''d-MMM-yyyy'') ''TransactionDate'', ' + CHAR(13) +
		'DATEPART(DAY,CAST(T._time AS DATE)) ''TransactionDay'', ' + CHAR(13) +
		'DATEPART(MONTH,CAST(T._time AS DATE)) ''TransactionMonth'', ' + CHAR(13) +
		'DATEPART(YEAR,CAST(T._time AS DATE)) ''TransactionYear'', ' + CHAR(13) +
		'O1.SiteNacs ''FromSiteNacs'', ' + CHAR(13) +
		'O1.SiteName ''FromSiteName'', ' + CHAR(13) +
		'O1.CcgNacs ''FromCcgNacs'', ' + CHAR(13) +
		'O1.CcgName ''FromCcgName'', ' + CHAR(13) +
		'O1.PartyKey ''FromPartyKey'', ' + CHAR(13) +
		'O1.ASID ''FromASID'', ' + CHAR(13) +
		'O1.SupplierName ''FromSupplierName'', ' + CHAR(13) +
		'O1.ProductVersion ''FromProductVersion'', ' + CHAR(13) +
		'O1.SiteType ''FromSiteType'', ' + CHAR(13) +
		'O2.SiteNacs ''ToSiteNacs'', ' + CHAR(13) +
		'O2.SiteName ''ToSiteName'', ' + CHAR(13) +
		'O2.CcgNacs ''ToCcgNacs'', ' + CHAR(13) +
		'O2.CcgName ''ToCcgName'', ' + CHAR(13) +
		'O2.PartyKey ''ToPartyKey'', ' + CHAR(13) +
		'O2.ASID ''ToASID'', ' + CHAR(13) +
		'O2.SupplierName ''ToSupplierName'', ' + CHAR(13) +
		'O2.ProductVersion ''ToProductVersion'', ' + CHAR(13) +
		'O2.SiteType ''ToSiteType'' ' + CHAR(13) +
		'FROM Transactions' + CONVERT(CHAR(3), dt, 100) + CONVERT(CHAR(4), dt, 120) + ' T ' + CHAR(13) +
		'LEFT OUTER JOIN OdsLookup O1 ON T.SSPFROM = O1.ASID ' + CHAR(13) +
		'LEFT OUTER JOIN OdsLookup O2 ON T.SSPTO = O2.ASID; ' AS SqlCommand
	INTO #Temp
	FROM cte;
	
	DECLARE @StartRowCounter INT = 1
	DECLARE @EndRowCounter INT = (SELECT MAX(RowNumber) FROM #Temp)	
	DECLARE @SQLDropCommandString VARCHAR(MAX) = ''
	DECLARE @SQLCommandString VARCHAR(MAX) = ''

	WHILE @StartRowCounter <= @EndRowCounter
	BEGIN
		SELECT @SQLCommandString = SqlCommand, @SQLDropCommandString = SqlDropCommand  FROM #Temp WHERE RowNumber = @StartRowCounter
		EXEC(@SQLDropCommandString)
		EXEC(@SQLCommandString)
		SELECT @StartRowCounter = @StartRowCounter + 1
	END
END

GO
