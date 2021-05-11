CREATE OR ALTER PROCEDURE Import.InstallNextFile
    @FileTypeId SMALLINT,
    @MoreFilesToInstall BIT = 0 OUTPUT 
AS
    SET NOCOUNT ON;

    DECLARE @ExecutionStart DATETIME2 = GETDATE();
    DECLARE @Msg VARCHAR(8000);

    EXEC dbo.PrintMsg '-----------------------------------------------';
    SET @Msg = 'Import.InstallNextFile starting for @FileTypeId=' + CONVERT(VARCHAR, @FileTypeId);
    EXEC dbo.PrintMsg @Msg;
    EXEC dbo.PrintMsg '-----------------------------------------------';
    EXEC dbo.PrintMsg '';

    -----------------------------------------------------
	-- check @FileTypeId valid
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
        EXEC dbo.ThrowError '@FileTypeId not recognised';
        RETURN;
    END;

    -----------------------------------------------------
	-- check process isn't already running
	-----------------------------------------------------
    IF EXISTS
    (
        SELECT *
        FROM 
			Import.[File]
        WHERE 
			IsInstalling = 1
    )
    BEGIN
        EXEC dbo.ThrowError 'Another file is currently being installed';
        RETURN;
    END;

    -----------------------------------------------------
    -- get @FileTypeId configuration values
    -----------------------------------------------------
    DECLARE @QueryFromBaseDate DATETIME2;
    DECLARE @StagingTableName VARCHAR(200);

    SELECT 
        @QueryFromBaseDate = QueryFromBaseDate,
        @StagingTableName = StagingTableName
    FROM 
		Configuration.FileType
    WHERE
		FileTypeId = @FileTypeId;

    -----------------------------------------------------
    -- get last installed file
    -----------------------------------------------------
    DECLARE @PreviousFileId INTEGER;
    DECLARE @PreviousFilePath VARCHAR(500);
    DECLARE @PreviousQueryToDate DATETIME2;
    DECLARE @PreviousSplunkInstance VARCHAR(20);

    SELECT
        @PreviousFileId = f.FileId,
        @PreviousFilePath = f.FilePath,
        @PreviousQueryToDate = f.QueryToDate,
        @PreviousSplunkInstance = SplunkInstance
    FROM
		Import.[File] f
    WHERE 
		f.FileTypeId = @FileTypeId
		AND f.IsInstalled = 1;

    IF (@PreviousFileId IS NOT NULL)
    BEGIN
        SET @Msg = '- Previous file installed was @FileId=' + CONVERT(VARCHAR, @PreviousFileId);
        EXEC dbo.PrintMsg @Msg;
        SET @Msg = '    with @FilePath=' + @PreviousFilePath;
        EXEC dbo.PrintMsg @Msg;
        SET @Msg = '    and @QueryToDate=' + CONVERT(VARCHAR, @PreviousQueryToDate, 120);
        EXEC dbo.PrintMsg @Msg;
        EXEC dbo.PrintMsg '';
    END
    ELSE
    BEGIN
        SET @Msg = '- This is the first file of @FileTypeId=' + CONVERT(VARCHAR, @FileTypeId) + ' being installed';
        EXEC dbo.PrintMsg @Msg;
        SET @Msg = '    Using the @QueryFromBaseDate=' + CONVERT(VARCHAR, @QueryFromBaseDate, 120) + ' as the starting point';
        EXEC dbo.PrintMsg @Msg;
        EXEC dbo.PrintMsg '';
    END;

    -----------------------------------------------------
    -- determine next file to install
    -----------------------------------------------------
    DECLARE @FileId INTEGER;
    DECLARE @FilePath VARCHAR(500);
    DECLARE @QueryFromDate DATETIME2;
    DECLARE @SplunkInstance VARCHAR(20);

    SELECT TOP 1
        @FileId = FileId,
        @FilePath = FilePath,
        @QueryFromDate = QueryFromDate,
        @SplunkInstance = SplunkInstance
    FROM 
		Import.[File]
    WHERE 
		FileTypeId = @FileTypeId
		AND IsInstalled = 0
    ORDER BY 
		QueryFromDate ASC;

    IF (@FileId IS NOT NULL)
    BEGIN        
        SET @Msg = '- Next file to install is @FileId=' + CONVERT(VARCHAR, @FileId);
        EXEC dbo.PrintMsg @Msg;
        SET @Msg = '    with @FilePath=' + @FilePath;
        EXEC dbo.PrintMsg @Msg;
        SET @Msg = '    and @QueryFromDate=' + CONVERT(VARCHAR, @QueryFromDate, 120);
        EXEC dbo.PrintMsg @Msg;
        EXEC dbo.PrintMsg '';
    END;

    -----------------------------------------------------
    -- if no files to install return
    -----------------------------------------------------
    IF (@FileId IS NULL)
    BEGIN
        EXEC dbo.PrintMsg '- No file found to install, quitting install';
        EXEC dbo.PrintMsg '';
        SET @MoreFilesToInstall = 0;
        RETURN;
    END;

    -----------------------------------------------------
    -- ensure next file is adjacent to previous file
    -----------------------------------------------------

    EXEC dbo.PrintMsg '- Checking file is adjacent to previous file (or query base date, if the first file)';
    EXEC dbo.PrintMsg '';


    IF (@PreviousQueryToDate IS NULL)
        SET @PreviousQueryToDate = @QueryFromBaseDate;

    IF (@QueryFromDate < @PreviousQueryToDate)
    BEGIN
        EXEC dbo.ThrowError 'Can''t install next file as its query dates overlap with the last installed file (or the base date)';
        RETURN;
    END
    
    IF (@QueryFromDate > @PreviousQueryToDate)
    BEGIN
        EXEC dbo.ThrowError 'Can''t install next file as there is a gap in query dates with the last installed file (or the base date)';
        RETURN;
    END;

-- TODO deal with spinea / spineb overlap
-- TODO deal with spinea / spineb alignment when switching to cloud

    -----------------------------------------------------
    -- start install
    -----------------------------------------------------
    BEGIN TRY
        BEGIN TRANSACTION InstallFileTransaction

            EXEC dbo.PrintMsg '- Starting install';
            EXEC dbo.PrintMsg '';

            -----------------------------------------------------
            -- check staging table is empty
            -----------------------------------------------------        
            DECLARE @EnsureTableEmptySql NVARCHAR(1000) = Import.CreateEnsureTableEmptyStatement(@StagingTableName);
            EXECUTE SP_EXECUTESQL @EnsureTableEmptySql;

            -----------------------------------------------------
            -- SET install flag on the file
            -----------------------------------------------------
            UPDATE 
				Import.[File]
            SET
                IsInstalling = 1
            WHERE
				FileId = @FileId;

            -----------------------------------------------------
            -- stage file into @StagingTable
            -----------------------------------------------------
            SET @Msg = '- Staging data into ' + @StagingTableName;
            EXEC dbo.PrintMsg @Msg;
    
            DECLARE @Sql NVARCHAR(1000) = Import.CreateBulkInsertStatement(@StagingTableName, @FilePath);
            EXECUTE SP_EXECUTESQL @Sql;

            SET @Msg = '    ' + CONVERT(VARCHAR, @@ROWCOUNT) + ' rows staged';
            EXEC dbo.PrintMsg @Msg;
            EXEC dbo.PrintMsg '';

            -----------------------------------------------------
            -- migrate data into destination tables
            -----------------------------------------------------
            EXEC dbo.PrintMsg '- Migrating data from staging to destination';

            DECLARE @RowsAdded INTEGER;
            DECLARE @RowsUpdated INTEGER;
            
            IF (@FileTypeId = 1)
            BEGIN
                EXEC Import.MigrateAsidLookupData
                    @FileId = @FileId,
                    @RowsAdded = @RowsAdded OUTPUT,
                    @RowsUpdated = @RowsUpdated OUTPUT;
            END
            ELSE IF (@FileTypeId = 2)
            BEGIN
                EXEC Import.MigrateSspTransactionData
                    @FileId = @FileId,
                    @RowsAdded = @RowsAdded OUTPUT,
                    @RowsUpdated = @RowsUpdated OUTPUT;
            END
            ELSE
            BEGIN
                EXEC dbo.ThrowError 'Migrate data stored procedure does not exists for that @FileTypeId';
                RETURN;                
            END;

            SET @Msg = '    ' + CONVERT(VARCHAR, @RowsAdded) + ' rows added';
            EXEC dbo.PrintMsg @Msg;
            SET @Msg = '    ' + CONVERT(VARCHAR, @RowsUpdated) + ' rows updated';
            EXEC dbo.PrintMsg @Msg;
            EXEC dbo.PrintMsg '';

            -----------------------------------------------------
            -- SET file as installed
            -----------------------------------------------------
            DECLARE @InstallDuration INTEGER = DATEDIFF(SECOND, @ExecutionStart, GETDATE());

            UPDATE 
				Import.[File]
            SET
                IsInstalling = 0,
                IsInstalled = 1,
                InstalledDate = GETDATE(),
                RowsAdded = @RowsAdded,
                RowsUpdated = @RowsUpdated,
                InstallDuration = @InstallDuration
            WHERE 
				FileId = @FileId;

            -----------------------------------------------------
            -- clear down the staging table
            -----------------------------------------------------
            EXEC dbo.PrintMsg '- Clear down the staging table';

            DECLARE @TruncateSql NVARCHAR(1000) = Import.CreateTruncateTableStatement(@StagingTableName);
            EXECUTE SP_EXECUTESQL @TruncateSql;

            EXEC dbo.PrintMsg '    Clear complete';
            EXEC dbo.PrintMsg '';

            -----------------------------------------------------
            -- SET whether more files are waiting install
            -----------------------------------------------------
            SET @MoreFilesToInstall = 0;
            
            IF EXISTS 
            (
                SELECT *
                FROM 
					Import.[File]
                WHERE 
					FileTypeId = @FileTypeId
					AND IsInstalled = 0
            )
            BEGIN
                SET @MoreFilesToInstall = 1;
            END;

            EXEC dbo.PrintMsg '- File successfully installed';
            SET @Msg = '    Install took ' + CONVERT(VARCHAR, @InstallDuration) + ' seconds'
            SET @Msg = '    There are ' + CASE WHEN @MoreFilesToInstall = 0 THEN ' no ' ELSE '' END + ' more files to install of @FileTypeId=' + CONVERT(VARCHAR, @FileTypeId)
            EXEC dbo.PrintMsg '';

		COMMIT TRANSACTION InstallFileTransaction;
	END TRY
	BEGIN CATCH
		EXEC dbo.PrintMsg '- Error occurred, rolling back'
        
        ROLLBACK TRANSACTION InstallFileTransaction;

        EXEC dbo.PrintMsg '    Rolled back'
        EXEC dbo.PrintMsg '';

		THROW;
	END CATCH;
