if (object_id('Import.ParseFilePath') is not null)
	drop procedure Import.ParseFilePath;

go

create procedure Import.ParseFilePath
(
	@FilePath varchar(500),
	@FileTypeId smallint,
	@QueryFromDate datetime2 output,
	@QueryToDate datetime2 output,
	@SplunkInstance varchar(10) output,
	@ExtractDate datetime2 output
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
	if (trim(isnull(@FilePath, '')) = '')
	begin
		exec dbo.ThrowError '@FilePath is empty';
		return;
	end;

	if not exists
	(
		select *
		from Configuration.FileType
		where FileTypeId = @FileTypeId
	)
	begin
		exec dbo.ThrowError '@FileTypeId not recognised';
		return;
	end;

	-----------------------------------------------------
	-- get file constants
	-----------------------------------------------------
	declare @PathSeparator varchar(1);
	declare @ProjectNameFilePrefix varchar(50);
	declare @ComponentSeparator varchar(1);
	declare @FileExtension varchar(5);

	select 
		@PathSeparator = PathSeparator,
		@ProjectNameFilePrefix = ProjectNameFilePrefix,
		@ComponentSeparator = ComponentSeparator,
		@FileExtension = FileExtension
	from Configuration.FilePathConstants;

	-----------------------------------------------------
	-- get file type variables
	-----------------------------------------------------
	declare @DirectoryName VARCHAR(200);
	declare @ExtractNameFilePrefix VARCHAR(50);

	select
		@DirectoryName = DirectoryName,
		@ExtractNameFilePrefix = FileTypeFilePrefix
	from Configuration.FileType
	where FileTypeId = @FileTypeId;

	-----------------------------------------------------
	-- validate and remove @DirectoryName, @PathSeperator
	-----------------------------------------------------
	if (@FilePath not like (@DirectoryName + @PathSeparator + '%'))
	begin
		exec dbo.ThrowError '@FilePath has unexpected directory name';
		return;
	end;

	set @FilePath = substring(@FilePath, len(@DirectoryName + @PathSeparator) + 1, len(@FilePath));

	-----------------------------------------------------
	-- validate and remove @FileExtension
	-----------------------------------------------------
	if (@FilePath not like ('%' + @FileExtension))
	begin
		exec dbo.ThrowError '@FileName has unexpected file name extension';
		return;
	end;

	set @FilePath = substring(@FilePath, 1, len(@FilePath) - len(@FileExtension));

	-----------------------------------------------------
	-- validate @ProjectNameFilePrefix value
	-----------------------------------------------------
	if (isnull(Import.StringSplitGetPiece(@FilePath, @ComponentSeparator, 1), '') != @ProjectNameFilePrefix)
	begin
		exec dbo.ThrowError '@FilePath has unexpected PROJECTNAME value';
		return;
	end;

	-----------------------------------------------------
	-- validate @ImportNameFilePrefix value
	-----------------------------------------------------
	if (isnull(Import.StringSplitGetPiece(@FilePath, @ComponentSeparator, 2), '') != @ExtractNameFilePrefix)
	begin
		exec dbo.ThrowError '@FilePath has unexpected IMPORTNAME value';
		return;
	end;

	-----------------------------------------------------
	-- parse @QueryFromDate value
	-----------------------------------------------------
	declare @QueryFromDateString varchar(50) = isnull(Import.StringSplitGetPiece(@FilePath, @ComponentSeparator, 3), '');

	begin try
		set @QueryFromDate = Import.ParseDateTime(@QueryFromDateString);
	end try
	begin catch
		declare @Msg1 varchar(1000) = 'QUERYDATEFROM could not be parsed from @FileName.  Error was "' + error_message() + '"';
		exec dbo.ThrowError @Msg1;
		return;
	end catch;

	-----------------------------------------------------
	-- parse @QueryToDate value
	-----------------------------------------------------
	declare @QueryToDateString VARCHAR(50) = ISNULL(Import.StringSplitGetPiece(@FilePath, @ComponentSeparator, 4), '');

	begin try
		set @QueryToDate = Import.ParseDateTime(@QueryToDateString);
	end try
	begin catch
		declare @Msg2 varchar(1000) = 'QUERYTODATE could not be parsed from @FileName.  Error was "' + error_message() + '"';
		exec dbo.ThrowError @Msg2;
		return;
	end catch;

	-----------------------------------------------------
	-- parse and validate @SplunkInstance value
	-----------------------------------------------------
	set @SplunkInstance = isnull(Import.StringSplitGetPiece(@FilePath, @ComponentSeparator, 5), '');

	if not exists
	(
		select *
		from Configuration.SplunkInstance
		where SplunkInstance = @SplunkInstance
	)
	begin
		exec dbo.ThrowError '@FilePath has unrecognised SPLUNKINSTANCE value';
		return;
	end;

	-----------------------------------------------------
	-- parse @ExtractDate value
	-----------------------------------------------------
	declare @ExtractDateString varchar(50) = isnull(Import.StringSplitGetPiece(@FilePath, @ComponentSeparator, 6), '');

	begin try
		set @ExtractDate = Import.ParseDateTime(@ExtractDateString);
	end try
	begin catch
		declare @msg3 varchar(1000) = 'EXTRACTDATE could not be parsed from @FileName.  Error was "' + error_message() + '"';
		exec dbo.ThrowError @msg3;
		return;
	end catch;
