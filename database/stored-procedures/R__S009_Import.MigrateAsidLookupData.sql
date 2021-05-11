CREATE OR ALTER PROCEDURE Import.MigrateAsidLookupData
(
    @FileId SMALLINT,
    @RowsAdded INTEGER OUTPUT,
    @RowsUpdated INTEGER OUTPUT
)
AS
    -----------------------------------------------------
    -- check proc is called as part of install
    -----------------------------------------------------
    IF NOT EXISTS
    (
        SELECT * 
        FROM 
			sys.dm_tran_active_transactions 
        WHERE 
			transaction_id = CURRENT_TRANSACTION_ID()
			AND NAME = 'InstallFileTransaction'
    )
    BEGIN
        EXEC dbo.ThrowError 'Import.MigrateAsidLookupData called out of context';
        RETURN;
    END;

    -----------------------------------------------------
    -- migrate data to destination
    -----------------------------------------------------
    INSERT INTO Data.AsidLookup
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
    SELECT
        AsidLookupStaging.ASID,
        AsidLookupStaging.OrgName,
        AsidLookupStaging.NACS,
        AsidLookupStaging.OrgType,
        AsidLookupStaging.PostCode,
        ISNULL(AsidLookupStaging.MName, ''),
        ISNULL(AsidLookupStaging.PName, ''),
        @FileId
    FROM 
		Import.AsidLookupStaging AsidLookupStaging
		LEFT OUTER JOIN Data.AsidLookup AsidLookup ON AsidLookupStaging.ASID = AsidLookup.Asid
    WHERE
		AsidLookup.Asid IS NULL;

    SET @RowsAdded = @@ROWCOUNT;

    UPDATE 
		AsidLookup
    SET
        AsidLookup.OrgName = AsidLookupStaging.OrgName,
        AsidLookup.OdsCode = AsidLookupStaging.NACS,
        AsidLookup.OrgType = AsidLookupStaging.OrgType,
        AsidLookup.Postcode = AsidLookupStaging.PostCode,
        AsidLookup.SupplierName = ISNULL(AsidLookupStaging.MName, ''),
        AsidLookup.ProductName = ISNULL(AsidLookupStaging.PName, ''),
        AsidLookup.FileId = @FileId
    FROM 
		Data.AsidLookup AsidLookup
    INNER JOIN Import.AsidLookupStaging AsidLookupStaging on AsidLookup.Asid = AsidLookupStaging.ASID
    WHERE
    (
        AsidLookup.OrgName != AsidLookupStaging.OrgName
        OR AsidLookup.OdsCode != AsidLookupStaging.NACS
        OR AsidLookup.OrgType != AsidLookupStaging.OrgType
        OR AsidLookup.Postcode != AsidLookupStaging.PostCode
        OR AsidLookup.SupplierName != AsidLookupStaging.MName
        OR AsidLookup.ProductName != AsidLookupStaging.PName
    );

    SET @RowsUpdated = @@ROWCOUNT;

