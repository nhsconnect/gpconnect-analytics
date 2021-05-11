if (object_id('Import.MigrateAsidLookupData') is not null)
    drop procedure Import.MigrateAsidLookupData;

go

create procedure Import.MigrateAsidLookupData
(
    @FileId smallint,
    @RowsAdded integer output,
    @RowsUpdated integer output
)
AS
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
    select
        s.ASID,
        s.OrgName,
        s.NACS,
        s.OrgType,
        s.PostCode,
        isnull(s.MName, ''),
        isnull(s.PName, ''),
        @FileId
    from Import.AsidLookupStaging s
	left outer join Data.AsidLookup a on s.ASID = a.Asid
    where a.Asid is null;

    set @RowsAdded = @@rowcount;

    update a
    set
        a.OrgName = s.OrgName,
        a.OdsCode = s.NACS,
        a.OrgType = s.OrgType,
        a.Postcode = s.PostCode,
        a.SupplierName = isnull(s.MName, ''),
        a.ProductName = isnull(s.PName, ''),
        a.FileId = @FileId
    from Data.AsidLookup a
    inner join Import.AsidLookupStaging s on a.Asid = s.ASID
    where
    (
        a.OrgName != s.OrgName
        or a.OdsCode != s.NACS
        or a.OrgType != s.OrgType
        or a.Postcode != s.PostCode
        or a.SupplierName != s.MName
        or a.ProductName != s.PName
    );

    set @RowsUpdated = @@rowcount;
