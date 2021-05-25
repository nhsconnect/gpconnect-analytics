if (object_id('Import.MigrateSspTransactionData') is not null)
    drop procedure Import.MigrateSspTransactionData;

go

create procedure Import.MigrateSspTransactionData
    @FileId smallint,
    @RowsAdded integer output,
    @RowsUpdated integer output,
    @RowsDeleted integer output
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
        (select (max(isnull(InteractionId, 0))) from Data.Interaction) + row_number() over (order by interaction),
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
        IsDeleted,
        FileId
    )
    select distinct
        isnull(newA.asid, ''),
        'UNKNOWN',
        'UNKNOWN',
        'UNKNOWN',
        'UNKNOWN',
        'UNKNOWN',
        'UNKNOWN',
        0,
        @FileId
    from 
    (
        select distinct
            s.sspFrom as asid
        from Import.SspTransactionStaging s
	    left outer join Data.AsidLookup a on isnull(s.sspFrom, '') = a.Asid
	    where a.Asid is null

        union

        select distinct
            s.SspTo as asid
        from Import.SspTransactionStaging s
	    left outer join Data.AsidLookup a on isnull(s.sspTo, '') = a.Asid
	    where a.Asid is null
    ) newA;


    set @Msg = '    ' + convert(varchar, @@rowcount) + ' ASIDs found; adding to Data.AsidLookup';
    exec dbo.PrintMsg @Msg;


    -----------------------------------------------------
    -- install file data to destination table
    -----------------------------------------------------
    exec dbo.PrintMsg '    Inserting data into destination table';

    insert into Data.SspTransaction with (tablock)
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
        convert(datetimeoffset, replace(replace(s._time, '+0000', '+00:00'), '+0100', '+01:00')),
        isnull(s.sspFrom, ''),
        isnull(s.sspTo, ''),
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
    order by convert(datetimeoffset, replace(replace(s._time, '+0000', '+00:00'), '+0100', '+01:00')) asc;

    set @RowsAdded = @@rowcount;
    set @RowsUpdated = 0;
    set @RowsDeleted = 0;

    set @Msg = '    ' + convert(varchar, @RowsAdded) + ' rows added to Data.SspTransaction';
    exec dbo.PrintMsg @Msg;
    exec dbo.PrintMsg '';
