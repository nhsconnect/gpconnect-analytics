if exists (select object_id('Import.DetermineNextExtract'))
	drop procedure Import.DetermineNextExtract;

go

create procedure ApiReader.DetermineNextExtract
(
	@FileTypeId smallint,
	@ExtractRequired bit output,
	@QueryFromDate datetime2 output,
	@QueryToDate datetime2 output
)
as
	-----------------------------------------------------
	-- default outputs to no extract required
	-----------------------------------------------------
	set @ExtractRequired = 0;
	set @QueryFromDate = null;
	set @QueryToDate = null;

	-----------------------------------------------------
	-- ensure the file type supports query dates
	-----------------------------------------------------
	if not exists
	(
		select *
		from Configuration.FileType
		where FileTypeId = @FileTypeId
	)
	begin
		exec dbo.ThrowError '@FileType not recognised, or does not support query dates';
		return;
	end;

	-----------------------------------------------------
	-- Get base query dates configuration
	-----------------------------------------------------
	declare @QueryFromBaseDate datetime2;
	declare @QueryPeriodHours integer;

	select 
		@QueryFromBaseDate = QueryFromBaseDate,
		@QueryPeriodHours = QueryPeriodHours
	from Configuration.FileType
	where FileTypeId = @FileTypeId;

	-----------------------------------------------------
	-- calculate @QueryFrom and @QueryToDate
	-----------------------------------------------------
	declare @QueryFromDateCandidate datetime2;
	declare @QueryToDateCandidate datetime2;
	
	select top 1
		@QueryFromDateCandidate = QueryToDate
	from Import.[File]
	where FileTypeId = @FileTypeId
	order by QueryToDate desc;
	
	if (@QueryFromDateCandidate is null)
		set @QueryFromDateCandidate = @QueryFromBaseDate;
	
	set @QueryToDateCandidate = dateadd(hour, @QueryPeriodHours, @QueryFromDateCandidate);

	-----------------------------------------------------
	-- determine whether the query range is in the past
	-- and if so, set the download to required
	-----------------------------------------------------
	if (@QueryToDateCandidate < getdate())
	begin
		set @ExtractRequired = 1;
		set @QueryFromDate = @QueryFromDateCandidate;
		set @QueryToDate = @QueryToDateCandidate;
	end;
