CREATE OR ALTER PROCEDURE Import.ParseFilePath
(
	@FilePath VARCHAR(500),
	@FileTypeId SMALLINT,
	@QueryFromDate DATETIME2 OUTPUT,
	@QueryToDate DATETIME2 OUTPUT,
	@SplunkInstance VARCHAR(10) OUTPUT,
	@ExtractDate DATETIME2 OUTPUT
)
/*
	General format:

	PROJECTNAME-IMPORTNAME-QUERYFROMDATE-QUERYTODATE-SPLUNKINSTANCE-EXTRACTDATE.csv

	Where:

	- QUERYDATEFROM, QUERYDATETO and EXTRACTDATE are YYYYMMDDTHHmmss
*/
AS
	-----------------------------------------------------
	-- sanity check parameters
	-----------------------------------------------------
	IF (TRIM(ISNULL(@FilePath, '')) = '')
	BEGIN
		EXEC dbo.ThrowError '@FilePath is empty';
		RETURN;
	END;

	IF NOT EXISTS
	(
		SELECT *
		FROM Configuration.FileType
		WHERE FileTypeId = @FileTypeId
	)
	BEGIN
		EXEC dbo.ThrowError '@FileTypeId not recognised';
		RETURN;
	END;

	-----------------------------------------------------
	-- get file constants
	-----------------------------------------------------
	DECLARE @PathSeparator VARCHAR(1);
	DECLARE @ProjectNameFilePrefix VARCHAR(50);
	DECLARE @ComponentSeparator VARCHAR(1);
	DECLARE @FileExtension VARCHAR(5);

	SELECT 
		@PathSeparator = PathSeparator,
		@ProjectNameFilePrefix = ProjectNameFilePrefix,
		@ComponentSeparator = ComponentSeparator,
		@FileExtension = FileExtension
	FROM
		Configuration.FilePathConstants;

	-----------------------------------------------------
	-- get file type variables
	-----------------------------------------------------
	DECLARE @DirectoryName VARCHAR(200);
	DECLARE @ExtractNameFilePrefix VARCHAR(50);

	SELECT
		@DirectoryName = DirectoryName,
		@ExtractNameFilePrefix = FileTypeFilePrefix
	FROM 
		Configuration.FileType
	WHERE
		FileTypeId = @FileTypeId;

	-----------------------------------------------------
	-- validate and remove @DirectoryName, @PathSeperator
	-----------------------------------------------------
	IF (@FilePath NOT LIKE (@DirectoryName + @PathSeparator + '%'))
	BEGIN
		EXEC dbo.ThrowError '@FilePath has unexpected directory name';
		RETURN;
	END;

	SET @FilePath = SUBSTRING(@FilePath, LEN(@DirectoryName + @PathSeparator) + 1, LEN(@FilePath));

	-----------------------------------------------------
	-- validate and remove @FileExtension
	-----------------------------------------------------
	IF (@FilePath not like ('%' + @FileExtension))
	BEGIN
		EXEC dbo.ThrowError '@FileName has unexpected file name extension';
		RETURN;
	END;

	SET @FilePath = SUBSTRING(@FilePath, 1, LEN(@FilePath) - LEN(@FileExtension));

	-----------------------------------------------------
	-- validate @ProjectNameFilePrefix value
	-----------------------------------------------------
	IF (ISNULL(Import.StringSplitGetPiece(@FilePath, @ComponentSeparator, 1), '') != @ProjectNameFilePrefix)
	BEGIN
		EXEC dbo.ThrowError '@FilePath has unexpected PROJECTNAME value';
		RETURN;
	END;

	-----------------------------------------------------
	-- validate @ImportNameFilePrefix value
	-----------------------------------------------------
	IF (ISNULL(Import.StringSplitGetPiece(@FilePath, @ComponentSeparator, 2), '') != @ExtractNameFilePrefix)
	BEGIN
		EXEC dbo.ThrowError '@FilePath has unexpected IMPORTNAME value';
		RETURN;
	END;

	-----------------------------------------------------
	-- parse @QueryFromDate value
	-----------------------------------------------------
	DECLARE @QueryFromDateString VARCHAR(50) = ISNULL(Import.StringSplitGetPiece(@FilePath, @ComponentSeparator, 3), '');

	BEGIN TRY
		SET @QueryFromDate = Import.ParseDateTime(@QueryFromDateString);
	END TRY
	BEGIN CATCH
		DECLARE @msg1 VARCHAR(1000) = 'QUERYDATEFROM could not be parsed from @FileName.  Error was "' + ERROR_MESSAGE() + '"';
		EXEC dbo.ThrowError @msg1;
		RETURN;
	END CATCH;

	-----------------------------------------------------
	-- parse @QueryToDate value
	-----------------------------------------------------
	DECLARE @QueryToDateString VARCHAR(50) = ISNULL(Import.StringSplitGetPiece(@FilePath, @ComponentSeparator, 4), '');

	BEGIN TRY
		SET @QueryToDate = Import.ParseDateTime(@QueryToDateString);
	END TRY
	BEGIN CATCH
		DECLARE @msg2 VARCHAR(1000) = 'QUERYTODATE could not be parsed from @FileName.  Error was "' + ERROR_MESSAGE() + '"';
		EXEC dbo.ThrowError @msg2;
		RETURN;
	END CATCH;

	-----------------------------------------------------
	-- parse and validate @SplunkInstance value
	-----------------------------------------------------
	SET @SplunkInstance = ISNULL(Import.StringSplitGetPiece(@FilePath, @ComponentSeparator, 5), '');

	IF NOT EXISTS
	(
		SELECT *
		FROM Configuration.SplunkInstance
		WHERE SplunkInstance = @SplunkInstance
	)
	BEGIN
		EXEC dbo.ThrowError '@FilePath has unrecognised SPLUNKINSTANCE value';
		RETURN;
	END;

	-----------------------------------------------------
	-- parse @ExtractDate value
	-----------------------------------------------------
	DECLARE @ExtractDateString VARCHAR(50) = ISNULL(Import.StringSplitGetPiece(@FilePath, @ComponentSeparator, 6), '');

	BEGIN TRY
		SET @ExtractDate = Import.ParseDateTime(@ExtractDateString);
	END TRY
	BEGIN CATCH
		DECLARE @msg3 VARCHAR(1000) = 'EXTRACTDATE could not be parsed from @FileName.  Error was "' + ERROR_MESSAGE() + '"';
		EXEC dbo.ThrowError @msg3;
		RETURN;
	END CATCH;
