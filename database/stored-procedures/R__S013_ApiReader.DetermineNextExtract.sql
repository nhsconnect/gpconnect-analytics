CREATE OR ALTER PROCEDURE ApiReader.DetermineNextExtract
(
	@FileTypeId SMALLINT,
	@ExtractRequired BIT OUTPUT,
	@QueryFromDate DATETIME2 OUTPUT,
	@QueryToDate DATETIME2 OUTPUT
)
AS
	-----------------------------------------------------
	-- default outputs to no extract required
	-----------------------------------------------------
	SET @ExtractRequired = 0;
	SET @QueryFromDate = NULL;
	SET @QueryToDate = NULL;

	-----------------------------------------------------
	-- ensure the file type supports query dates
	-----------------------------------------------------
	IF NOT EXISTS
	(
		SELECT *
		FROM 
			Configuration.FileType
		WHERE
			FileTypeId = @FileTypeId
	)
	BEGIN
		EXEC dbo.ThrowError '@FileType not recognised, or does not support query dates';
		RETURN;
	END;

	-----------------------------------------------------
	-- Get base query dates configuration
	-----------------------------------------------------
	DECLARE @QueryFromBaseDate DATETIME2;
	DECLARE @QueryPeriodHours INTEGER;

	SELECT 
		@QueryFromBaseDate = QueryFromBaseDate,
		@QueryPeriodHours = QueryPeriodHours
	FROM
		Configuration.FileType
	WHERE
		FileTypeId = @FileTypeId;

	-----------------------------------------------------
	-- calculate @QueryFrom and @QueryToDate
	-----------------------------------------------------
	DECLARE @QueryFromDateCandidate DATETIME2;
	DECLARE @QueryToDateCandidate DATETIME2;
	
	SELECT TOP 1
		@QueryFromDateCandidate = QueryToDate
	FROM 
		Import.[File]
	WHERE
		FileTypeId = @FileTypeId
	ORDER BY 
		QueryToDate DESC;
	
	IF (@QueryFromDateCandidate IS NULL)
		SET @QueryFromDateCandidate = @QueryFromBaseDate;
	
	SET @QueryToDateCandidate = DATEADD(HOUR, @QueryPeriodHours, @QueryFromDateCandidate);

	-----------------------------------------------------
	-- determine whether the query range is in the past
	-- and if so, set the download to required
	-----------------------------------------------------
	IF (@QueryToDateCandidate < GETDATE())
	BEGIN
		SET @ExtractRequired = 1;
		SET @QueryFromDate = @QueryFromDateCandidate;
		SET @QueryToDate = @QueryToDateCandidate;
	END;
