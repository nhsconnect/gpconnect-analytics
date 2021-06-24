if (object_id('Import.MigrateMeshTransactionData') is not null)
    DROP PROCEDURE [Import].[MigrateMeshTransactionData];
GO

CREATE PROCEDURE [Import].[MigrateMeshTransactionData]
    @FileId smallint,
    @RowsAdded integer output,
    @RowsUpdated integer output,
    @RowsDeleted integer output
AS
    declare @Msg varchar(8000);

    -----------------------------------------------------
    -- check proc is called as part of install
    -----------------------------------------------------
    if not exists
    (
        select * 
        from sys.dm_tran_active_transactions 
        where transaction_id = current_transaction_id()
        and name = 'InstallFileTransaction'
    )
    begin
        exec dbo.ThrowError 'Import.MigrateMeshTransactionData called out of context';
        return;
    end;

    -----------------------------------------------------
    -- install file data to destination table
    -----------------------------------------------------
    exec dbo.PrintMsg '    Inserting data into destination table';

	
	INSERT INTO [Data].[MeshTransaction] with (tablock)
			   ([Time]
			   ,[Sender]
			   ,[SenderOdsCode]
			   ,[SenderName]
			   ,[Recipient]
			   ,[RecipientOdsCode]
			   ,[RecipientName]
			   ,[Workflow]
			   ,[Filesize]
			   ,[FileId])
     SELECT
        convert(datetimeoffset, replace(replace(s._time, '+0000', '+00:00'), '+0100', '+01:00')),
        s.sender,
        ISNULL(s.senderOdsCode, ''),
        ISNULL(s.senderName, ''),
        s.recipient,
        ISNULL(s.recipientOdsCode, ''),
        ISNULL(s.recipientName, ''),
        s.workflow,
        s.fileSize,
        @FileId
    from Import.MeshTransactionStaging s
    order by convert(datetimeoffset, replace(replace(s._time, '+0000', '+00:00'), '+0100', '+01:00')) asc;

    set @RowsAdded = @@rowcount;
    set @RowsUpdated = 0;
    set @RowsDeleted = 0;

    set @Msg = '    ' + convert(varchar, @RowsAdded) + ' rows added to Data.MeshTransaction';
    exec dbo.PrintMsg @Msg;
    exec dbo.PrintMsg '';