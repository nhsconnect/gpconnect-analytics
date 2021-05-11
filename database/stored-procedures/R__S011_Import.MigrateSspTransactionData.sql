if (object_id('Import.MigrateSspTransactionData') is not null)
    drop procedure Import.MigrateSspTransactionData;

go

create procedure Import.MigrateSspTransactionData
    @FileId smallint,
    @RowsAdded integer output,
    @RowsUpdated integer output
as

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
        exec dbo.ThrowError 'Import.MigrateSspTransactionData called out of context';
        return;
    end;

    -----------------------------------------------------
    -- capture any new interactions found
    -----------------------------------------------------
    exec dbo.PrintMsg '    Scanning for unrecognised interactions';


    insert into Data.Interaction
    (
        InteractionId,
        InteractionName,
        ServiceName
    )
    select
        (select (max(isnull(InteractionId, 0)) + 1) from Data.Interaction),
        iNew.interaction,
        'other'
    from
    (
        select distinct
            isnull(interaction, '') as interaction
        from Import.SspTransactionStaging s
		left outer join Data.Interaction i on isnull(s.interaction, '') = i.InteractionName
        where i.InteractionName is null
    )  iNew;

    
    set @Msg = '    ' + convert(varchar, @@rowcount) + ' interactions found; adding to Data.Interaction';
    exec dbo.PrintMsg @Msg;

    -----------------------------------------------------
    -- capture any unknown asids found
    -----------------------------------------------------
    exec dbo.PrintMsg '    Scanning for unrecognised ASIDs';


    insert into Data.AsidLookup
    (
        Asid,
        OrgName,
        OdsCode,
        OrgType,
        Postcode,
        SupplierName,
        ProductName,
        FileId
    )
    select distinct
        newA.asid,
        'UNKNOWN',
        'UNKNOWN',
        'UNKNOWN',
        '',
        'UNKNOWN',
        'UNKNOWN',
        @FileId
    from 
    (
        select distinct
            s.sspFrom as asid
        from Import.SspTransactionStaging s
	    left outer join Data.AsidLookup a on s.sspFrom = a.Asid
	    where a.Asid is null

        union

        select distinct
            s.SspTo as asid
        from Import.SspTransactionStaging s
	    left outer join Data.AsidLookup a on s.sspTo = a.Asid
	    where a.Asid is null
    ) newA;


    set @Msg = '    ' + convert(varchar, @@rowcount) + ' ASIDs found; adding to Data.AsidLookup';
    exec dbo.PrintMsg @Msg;

    -----------------------------------------------------
    -- fix datetime offset into sql format
    -- (TODO is there a better way of doing this?)
    -----------------------------------------------------
    exec dbo.PrintMsg '    Fixing datetime format for migration';

    update Import.SspTransactionStaging
    set 
        _time = replace(_time, '+0000', '+00:00');

    update Import.SspTransactionStaging
    set 
        _time = replace(_time, '+0100', '+01:00');

    -----------------------------------------------------
    -- install file data to destination table
    -----------------------------------------------------
    exec dbo.PrintMsg '    Inserting data into destination table';

    insert into Data.SspTransaction
    (
        [Time],
        FromAsid,
        ToAsid,
        SspTraceId,
        InteractionId,
        ResponseCode,
        Duration,
        ResponseSize,
        ResponseErrorMessage,
        Method,
        FileId
    )
    select
        convert(datetimeoffset, s._time),
        s.sspFrom,
        s.sspTo,
        s.SspTraceId,
        i.InteractionId,
        s.responseCode,
        s.duration,
        s.responseSize,
        s.responseErrorMessage,
        s.method,
        @FileId
    from Import.SspTransactionStaging s
    left outer join Data.Interaction i on isnull(s.interaction, '') = i.InteractionName

    set @RowsAdded = @@rowcount;
    set @RowsUpdated = 0;

    set @Msg = '    ' + convert(varchar, @RowsAdded) + ' rows added to Data.SspTransaction';
    exec dbo.PrintMsg @Msg;
    exec dbo.PrintMsg '';
