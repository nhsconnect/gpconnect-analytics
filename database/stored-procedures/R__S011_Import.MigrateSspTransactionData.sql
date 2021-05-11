if (object_id('Import.MigrateSspTransactionData') is not null)
    drop procedure Import.MigrateSspTransactionData;

go

create procedure Import.MigrateSspTransactionData
    @FileId smallint,
    @RowsAdded integer output,
    @RowsUpdated integer output
as
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
            interaction
        from Import.SspTransactionStaging s
		left outer join Data.Interaction i on s.interaction = i.InteractionName
        where i.InteractionName is null
    )  iNew;

    -----------------------------------------------------
    -- capture any unknown SspFrom asids found
    -----------------------------------------------------
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
        s.sspFrom,
        'UNKNOWN',
        'UNKNOWN',
        'UNKNOWN',
        '',
        'UNKNOWN',
        'UNKNOWN',
        @FileId
    from Import.SspTransactionStaging s
	left outer join Data.AsidLookup a on s.sspFrom = a.Asid
    where a.Asid is null;

    -----------------------------------------------------
    -- capture any unknown SspTo asids found
    -----------------------------------------------------
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
        s.sspTo,
        'UNKNOWN',
        'UNKNOWN',
        'UNKNOWN',
        '',
        'UNKNOWN',
        'UNKNOWN',
        @FileId
    from Import.SspTransactionStaging s
	left outer join Data.AsidLookup a on s.sspTo = a.Asid
	where a.Asid is null;

    -----------------------------------------------------
    -- fix datetime offset into sql format
    -- (TODO is there a better way of doing this?)
    -----------------------------------------------------
    update Import.SspTransactionStaging
    set 
        _time = replace(_time, '+0000', '+00:00');

    -----------------------------------------------------
    -- install file data to destination table
    -----------------------------------------------------
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
    left outer join Data.Interaction i on s.interaction = i.InteractionName

    set @RowsAdded = @@rowcount;
    set @RowsUpdated = 0;
