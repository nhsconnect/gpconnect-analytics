if (object_id('Import.MigrateAsidLookupData') is not null)
    drop procedure Import.MigrateAsidLookupData;

go

create procedure Import.MigrateAsidLookupData
(
    @FileId smallint,
    @RowsAdded integer output,
    @RowsUpdated integer output,
    @RowsDeleted integer output
)
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
        exec dbo.ThrowError 'Import.MigrateAsidLookupData called out of context';
        return;
    end;

    -----------------------------------------------------
    -- migrate data to destination
    -----------------------------------------------------

    -- insert new rows
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
    select
        s.ASID,
        s.OrgName,
        s.NACS,
        s.OrgType,
        s.PostCode,
        isnull(s.MName, ''),
        isnull(s.PName, ''),
        0,
        @FileId
    from Import.AsidLookupStaging s
	left outer join Data.AsidLookup a on s.ASID = a.Asid
    where a.Asid is null;

    set @RowsAdded = @@rowcount;

    -- update existing rows where data has changed
    -- or a previously deleted row has been re-introduced
    update a
    set
        a.OrgName = s.OrgName,
        a.OdsCode = s.NACS,
        a.OrgType = s.OrgType,
        a.Postcode = s.PostCode,
        a.SupplierName = isnull(s.MName, ''),
        a.ProductName = isnull(s.PName, ''),
        a.IsDeleted = 0,
        a.FileId = @FileId
    from Data.AsidLookup a
    inner join Import.AsidLookupStaging s on a.Asid = s.ASID
    where
    (
        a.OrgName != s.OrgName
        or a.OdsCode != s.NACS
        or a.OrgType != s.OrgType
        or a.Postcode != s.PostCode
        or a.SupplierName != isnull(s.MName, '')
        or a.ProductName != isnull(s.PName, '')
        or a.IsDeleted = 1
    );

    set @RowsUpdated = @@rowcount;

    -- soft delete rows that have been removed
    update a
    set
        a.IsDeleted = 1,
        a.FileId = @FileId
    from Data.AsidLookup a
    left outer join Import.AsidLookupStaging s on a.Asid = s.ASID
    where s.ASID is null
    and a.IsDeleted = 0;

    set @RowsDeleted = @@rowcount;
    