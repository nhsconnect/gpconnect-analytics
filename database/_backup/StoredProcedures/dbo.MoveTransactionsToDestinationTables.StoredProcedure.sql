/****** Object:  StoredProcedure [dbo].[MoveTransactionsToDestinationTables]    Script Date: 27/04/2021 15:00:30 ******/
DROP PROCEDURE IF EXISTS [dbo].[MoveTransactionsToDestinationTables]
GO
/****** Object:  StoredProcedure [dbo].[MoveTransactionsToDestinationTables]    Script Date: 27/04/2021 15:00:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[MoveTransactionsToDestinationTables]
AS
BEGIN
	;WITH cte AS 
	(
		SELECT DISTINCT
		'MERGE Transactions' + FORMAT(CAST(t._Time AS DATE), 'MMMyyyy') + ' AS TARGET ' + CHAR(13) +
		'USING (SELECT * FROM TransactionsImport T WHERE DATEPART(MONTH,T._Time) = ' + CONVERT(VARCHAR, DATEPART(MONTH,T._Time)) + ' AND DATEPART(YEAR, T._time) = ' + CONVERT(VARCHAR, DATEPART(YEAR,T._Time)) + ') AS SOURCE ' + CHAR(13) +
		'ON (TARGET._time = SOURCE._time) ' + CHAR(13) + 
		'WHEN NOT MATCHED BY TARGET THEN ' + CHAR(13) +
		'INSERT ([_time],[sspFrom],[fromOrg],[fromSupplier],[sspTo],[toOrg],[toSupplier],[interaction],[responseCode],[duration],[responseSize],[responseErrorMessage],[method])  ' + CHAR(13) +
		'VALUES (SOURCE.[_time],SOURCE.[sspFrom],SOURCE.[fromOrg],SOURCE.[fromSupplier],SOURCE.[sspTo],SOURCE.[toOrg],SOURCE.[toSupplier],SOURCE.[interaction],SOURCE.[responseCode],SOURCE.[duration],SOURCE.[responseSize],SOURCE.[responseErrorMessage],SOURCE.[method]);' AS SqlCommand
	FROM 
		TransactionsImport t
	)
	SELECT 
		ROW_NUMBER() OVER (ORDER BY SqlCommand) AS RowNumber,
		cte.SqlCommand
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
