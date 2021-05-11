if (object_id('Import.InstallNextFile') is not null)
    drop procedure Import.InstallNextFile;

go

create procedure Import.InstallNextFile
    @FileTypeId smallint,
    @MoreFilesToInstall bit = 0 output 
as

    set nocount on;

    declare @ExecutionStart datetime2 = getdate();
    declare @Msg varchar(8000);

    exec dbo.PrintMsg '-----------------------------------------------';
    set @Msg = 'Import.InstallNextFile starting for @FileTypeId=' + convert(varchar, @filetypeid);
    exec dbo.PrintMsg @Msg;
    exec dbo.PrintMsg '-----------------------------------------------';
    exec dbo.PrintMsg '';

    -----------------------------------------------------
	-- check @FileTypeId valid
	-----------------------------------------------------
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
	-- check process isn't already running
	-----------------------------------------------------
    if exists
    (
        select *
        from Import.[File]
        where IsInstalling = 1
    )
    begin
        exec dbo.ThrowError 'Another file is currently being installed';
        return;
    end;

    -----------------------------------------------------
    -- get @FileTypeId configuration values
    -----------------------------------------------------
    declare @QueryFromBaseDate datetime2;
    declare @StagingTableName varchar(200);

    select 
        @QueryFromBaseDate = QueryFromBaseDate,
        @StagingTableName = StagingTableName
    from Configuration.FileType
    where FileTypeId = @FileTypeId;

    -----------------------------------------------------
    -- get last installed file
    -----------------------------------------------------
    declare @PreviousFileId integer;
    declare @PreviousFilePath varchar(500);
    declare @PreviousQueryToDate datetime2;
    declare @PreviousSplunkInstance varchar(20);

    select
        @PreviousFileId = f.FileId,
        @PreviousFilePath = f.FilePath,
        @PreviousQueryToDate = f.QueryToDate,
        @PreviousSplunkInstance = SplunkInstance
    from Import.[File] f
    where f.FileTypeId = @FileTypeId
	and f.IsInstalled = 1;

    if (@PreviousFileId is not null)
    begin
        set @Msg = '- Previous file installed was @FileId=' + convert(varchar, @PreviousFileId);
        exec dbo.PrintMsg @Msg;
        set @Msg = '    with @FilePath=' + @PreviousFilePath;
        exec dbo.PrintMsg @Msg;
        set @Msg = '    and @QueryToDate=' + convert(varchar, @PreviousQueryToDate, 120);
        exec dbo.PrintMsg @Msg;
        exec dbo.PrintMsg '';
    end
    else
    begin
        set @Msg = '- This is the first file of @FileTypeId=' + convert(varchar, @FileTypeId) + ' being installed';
        exec dbo.PrintMsg @Msg;
        set @Msg = '    Using the @QueryFromBaseDate=' + convert(varchar, @QueryFromBaseDate, 120) + ' as the starting point';
        exec dbo.PrintMsg @Msg;
        exec dbo.PrintMsg '';
    end;

    -----------------------------------------------------
    -- determine next file to install
    -----------------------------------------------------
    declare @FileId integer;
    declare @FilePath varchar(500);
    declare @QueryFromDate datetime2;
    declare @SplunkInstance varchar(20);

    select top 1
        @FileId = FileId,
        @FilePath = FilePath,
        @QueryFromDate = QueryFromDate,
        @SplunkInstance = SplunkInstance
    from Import.[File]
    where FileTypeId = @FileTypeId
	and IsInstalled = 0
    order by QueryFromDate asc;

    if (@FileId is not null)
    begin        
        set @Msg = '- Next file to install is @FileId=' + convert(varchar, @FileId);
        exec dbo.PrintMsg @Msg;
        set @Msg = '    with @FilePath=' + @FilePath;
        exec dbo.PrintMsg @Msg;
        set @Msg = '    and @QueryFromDate=' + convert(varchar, @QueryFromDate, 120);
        exec dbo.PrintMsg @Msg;
        exec dbo.PrintMsg '';
    end;

    -----------------------------------------------------
    -- if no files to install return
    -----------------------------------------------------
    if (@FileId is null)
    begin
        exec dbo.PrintMsg '- No file found to install, quitting install';
        exec dbo.PrintMsg '';
        set @MoreFilesToInstall = 0;
        return;
    end;

    -----------------------------------------------------
    -- ensure next file is adjacent to previous file
    -----------------------------------------------------

    exec dbo.PrintMsg '- Checking file is adjacent to previous file (or query base date, if the first file)';
    exec dbo.PrintMsg '';


    if (@PreviousQueryToDate is null)
        set @PreviousQueryToDate = @QueryFromBaseDate;

    if (@QueryFromDate < @PreviousQueryToDate)
    begin
        exec dbo.ThrowError 'Can''t install next file as its query dates overlap with the last installed file (or the base date)';
        return;
    end;
    
    if (@QueryFromDate > @PreviousQueryToDate)
    begin
        exec dbo.ThrowError 'Can''t install next file as there is a gap in query dates with the last installed file (or the base date)';
        return;
    end;

-- TODO deal with spinea / spineb overlap
-- TODO deal with spinea / spineb alignment when switching to cloud

    -----------------------------------------------------
    -- start install
    -----------------------------------------------------
    begin try
        begin transaction InstallFileTransaction

            exec dbo.PrintMsg '- Starting install';
            exec dbo.PrintMsg '';

            -----------------------------------------------------
            -- check staging table is empty
            -----------------------------------------------------        
            declare @EnsureTableEmptySql nvarchar(1000) = Import.CreateEnsureTableEmptyStatement(@StagingTableName);
            execute sp_executesql @EnsureTableEmptySql;

            -----------------------------------------------------
            -- SET install flag on the file
            -----------------------------------------------------
            update Import.[File]
            set
                IsInstalling = 1
            where FileId = @FileId;

            -----------------------------------------------------
            -- stage file into @StagingTable
            -----------------------------------------------------
            set @Msg = '- Staging data into ' + @StagingTableName;
            exec dbo.PrintMsg @Msg;
    
            declare @Sql nvarchar(1000) = Import.CreateBulkInsertStatement(@StagingTableName, @FilePath);
            execute sp_executesql @Sql;

            set @Msg = '    ' + convert(varchar, @@rowcount) + ' rows staged';
            exec dbo.PrintMsg @Msg;
            exec dbo.PrintMsg '';

            -----------------------------------------------------
            -- migrate data into destination tables
            -----------------------------------------------------
            exec dbo.PrintMsg '- Migrating data from staging to destination';

            declare @RowsAdded integer;
            declare @RowsUpdated integer;
            
            if (@FileTypeId = 1)
            begin
                exec Import.MigrateAsidLookupData
                    @FileId = @FileId,
                    @RowsAdded = @RowsAdded output,
                    @RowsUpdated = @RowsUpdated output;
            end
            else if (@FileTypeId = 2)
            begin
                exec Import.MigrateSspTransactionData
                    @FileId = @FileId,
                    @RowsAdded = @RowsAdded output,
                    @RowsUpdated = @RowsUpdated output;
            end
            else
            begin
                exec dbo.ThrowError 'Migrate data stored procedure does not exists for that @FileTypeId';
                return;                
            end;

            set @Msg = '    ' + convert(varchar, @RowsAdded) + ' rows added';
            exec dbo.PrintMsg @Msg;
            set @Msg = '    ' + convert(varchar, @RowsUpdated) + ' rows updated';
            exec dbo.PrintMsg @Msg;
            exec dbo.PrintMsg '';

            -----------------------------------------------------
            -- SET file as installed
            -----------------------------------------------------
            declare @InstallDuration integer = datediff(second, @ExecutionStart, getdate());

            update Import.[File]
            set
                IsInstalling = 0,
                IsInstalled = 1,
                InstalledDate = getdate(),
                RowsAdded = @RowsAdded,
                RowsUpdated = @RowsUpdated,
                InstallDuration = @InstallDuration
            where FileId = @FileId;

            -----------------------------------------------------
            -- clear down the staging table
            -----------------------------------------------------
            exec dbo.PrintMsg '- Clear down the staging table';

            declare @TruncateSql nvarchar(1000) = Import.CreateTruncateTableStatement(@StagingTableName);
            execute sp_executesql @TruncateSql;

            exec dbo.PrintMsg '    Clear complete';
            exec dbo.PrintMsg '';

            -----------------------------------------------------
            -- SET whether more files are waiting install
            -----------------------------------------------------
            set @MoreFilesToInstall = 0;
            
            if exists 
            (
                select *
                from Import.[File]
                where FileTypeId = @FileTypeId
				and IsInstalled = 0
            )
            begin
                set @MoreFilesToInstall = 1;
            end;

            exec dbo.PrintMsg '- File successfully installed';
            set @Msg = '    Install took ' + convert(varchar, @InstallDuration) + ' seconds'
            set @Msg = '    There are ' + case when @MoreFilesToInstall = 0 then ' no ' else '' end + ' more files to install of @FileTypeId=' + convert(varchar, @FileTypeId)
            exec dbo.PrintMsg '';

		commit transaction InstallFileTransaction;
    end try
    begin catch
        exec dbo.PrintMsg '';
        exec dbo.PrintMsg '- Error occurred, rolling back';

        rollback transaction InstallFileTransaction;

        exec dbo.PrintMsg '    Rolled back'
        exec dbo.PrintMsg '';

        throw;
    end catch;
