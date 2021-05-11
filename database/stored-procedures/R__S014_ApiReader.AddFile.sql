if exists (select object_id('Import.AddFile'))
	drop procedure Import.AddFile;

go

create procedure ApiReader.AddFile
(
	@FileTypeId smallint,
	@FilePath varchar(500)
)
as
	set nocount on;
	declare @Msg varchar(1000);

	-----------------------------------------------------
	-- validate @FileTypeId
	-----------------------------------------------------
	if not exists
	(
		select *
		from Configuration.FileType
		where FileTypeId = @FileTypeId
	)
	begin
		exec dbo.ThrowError '@FileTypeId is not recognised';
		return;
	end;

	-----------------------------------------------------
	-- validate @FilePath
	-----------------------------------------------------
	set @FilePath = trim(isnull(@FilePath, ''));

	if (@FilePath = '')
	begin
		exec dbo.ThrowError '@FilePath is empty';
		return;
	end;
	
	if exists
	(
		select * 
		from Import.[File]
		where FilePath = @FilePath
	)
	begin
		set @msg = 'A file with @FilePath ' + @FilePath + ' already exists';
		exec dbo.ThrowError @msg;
		return;
	end

	-----------------------------------------------------
	-- parse @FilePath
	-----------------------------------------------------
	declare @ExtractDate datetime2;
	declare @SplunkInstance varchar(10);
	declare @QueryFromDate datetime2;
	declare @QueryToDate datetime2;

	-----------------------------------------------------
	-- parse @FilePath
	-----------------------------------------------------
	begin try
		exec Import.ParseFilePath
			@FilePath,
			@FileTypeId,
			@QueryFromDate output,
			@QueryToDate output,
			@SplunkInstance output,
			@ExtractDate output
	end try
	begin catch
		throw;
	end catch;

	-----------------------------------------------------
	-- validate dates in @FilePath
	-----------------------------------------------------
	if (@ExtractDate > getdate())
	begin
		exec dbo.ThrowError 'EXTRACTDATE in @FilePath is in the future';
		return;
	end;

	if (@QueryFromDate > getdate())
	begin
		exec dbo.ThrowError 'QUERYFROMDATE in @FilePath is in the future';
		return;
	end;

	if (@QueryToDate > getdate())
	begin
		exec dbo.ThrowError 'QUERYTODATE in @FilePath is in the future';
		return;
	end;

	if (@QueryFromDate > @QueryToDate)
	begin
		exec dbo.ThrowError 'QUERYFROMDATE is after QUERYTODATE in @FilePath';
		return;
	end;

	if (@QueryToDate > @ExtractDate)
	begin
		exec dbo.ThrowError 'QUERYTODATE is after EXTRACTDATE in @FilePath';
		return;
	end;

	-----------------------------------------------------
	-- add file
	-----------------------------------------------------
	insert into Import.[File]
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
	values
	(
		@FileTypeId,
		@FilePath,
		@QueryFromDate,
		@QueryToDate,
		@SplunkInstance,
		@ExtractDate,
		0,
		0,
		null,
		null,
		null,
		null
	);
