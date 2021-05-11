CREATE OR ALTER PROCEDURE ApiReader.AddFile
(
	@FileTypeId SMALLINT,
	@FilePath VARCHAR(500)
)
AS
	SET NOCOUNT ON;
	DECLARE @msg VARCHAR(1000);

	-----------------------------------------------------
	-- validate @FileTypeId
	-----------------------------------------------------
	IF NOT EXISTS
	(
		SELECT *
		FROM Configuration.FileType
		WHERE FileTypeId = @FileTypeId
	)
	BEGIN
		EXEC dbo.ThrowError '@FileTypeId is not recognised';
		RETURN;
	END;

	-----------------------------------------------------
	-- validate @FilePath
	-----------------------------------------------------
	SET @FilePath = TRIM(ISNULL(@FilePath, ''));

	IF (@FilePath = '')
	BEGIN
		EXEC dbo.ThrowError '@FilePath is empty';
		RETURN;
	END;
	
	IF EXISTS
	(
		SELECT * 
		FROM Import.[File]
		WHERE FilePath = @FilePath
	)
	BEGIN
		SET @msg = 'A file with @FilePath ' + @FilePath + ' already exists';
		EXEC dbo.ThrowError @msg;
		RETURN;
	END

	-----------------------------------------------------
	-- parse @FilePath
	-----------------------------------------------------
	DECLARE @ExtractDate DATETIME2;
	DECLARE @SplunkInstance VARCHAR(10);
	DECLARE @QueryFromDate DATETIME2;
	DECLARE @QueryToDate DATETIME2;

	-----------------------------------------------------
	-- parse @FilePath
	-----------------------------------------------------
	BEGIN TRY
		EXEC Import.ParseFilePath
			@FilePath,
			@FileTypeId,
			@QueryFromDate OUTPUT,
			@QueryToDate OUTPUT,
			@SplunkInstance OUTPUT,
			@ExtractDate  OUTPUT
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;

	-----------------------------------------------------
	-- validate dates in @FilePath
	-----------------------------------------------------
	IF (@ExtractDate > GETDATE())
	BEGIN
		EXEC dbo.ThrowError 'EXTRACTDATE in @FilePath is in the future';
		RETURN;
	END;

	IF (@QueryFromDate > GETDATE())
	BEGIN
		EXEC dbo.ThrowError 'QUERYFROMDATE in @FilePath is in the future';
		RETURN;
	END;

	IF (@QueryToDate > GETDATE())
	BEGIN
		EXEC dbo.ThrowError 'QUERYTODATE in @FilePath is in the future';
		RETURN;
	END;

	IF (@QueryFromDate > @QueryToDate)
	BEGIN
		EXEC dbo.ThrowError 'QUERYFROMDATE is after QUERYTODATE in @FilePath';
		RETURN;
	END;

	IF (@QueryToDate > @ExtractDate)
	BEGIN
		EXEC dbo.ThrowError 'QUERYTODATE is after EXTRACTDATE in @FilePath';
		RETURN;
	END;

	-----------------------------------------------------
	-- add file
	-----------------------------------------------------
	INSERT INTO Import.[File]
	(
		FileTypeId,
		FilePath,
		QueryFromDate,
		QueryToDate,
		SplunkInstance,
		ExtractDate,
		IsInstalling,
		IsInstalled,
		InstalledDate,
		RowsAdded,
		RowsUpdated,
		InstallDuration
	)
	VALUES
	(
		@FileTypeId,
		@FilePath,
		@QueryFromDate,
		@QueryToDate,
		@SplunkInstance,
		@ExtractDate,
		0,
		0,
		NULL,
		NULL,
		NULL,
		NULL
	);
