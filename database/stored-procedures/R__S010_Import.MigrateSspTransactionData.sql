CREATE OR ALTER PROCEDURE Import.MigrateSspTransactionData
    @FileId SMALLINT,
    @RowsAdded INTEGER OUTPUT,
    @RowsUpdated INTEGER OUTPUT
AS
    -----------------------------------------------------
    -- check proc is called as part of install
    -----------------------------------------------------
    IF NOT EXISTS
    (
        SELECT * 
        FROM 
			sys.dm_tran_active_transactions tas 
        WHERE 
			transaction_id = CURRENT_TRANSACTION_ID()
			AND name = 'InstallFileTransaction'
    )
    BEGIN
        EXEC dbo.ThrowError 'Import.MigrateSspTransactionData called out of context';
        RETURN;
    END;

    -----------------------------------------------------
    -- capture any new interactions found
    -----------------------------------------------------
    INSERT INTO Data.Interaction
    (
        InteractionId,
        InteractionName,
        ServiceName
    )
    SELECT
        (SELECT (MAX(ISNULL(InteractionId, 0)) + 1) FROM Data.Interaction),
        Interaction.interaction,
        'other'
    FROM
    (
        SELECT DISTINCT
            interaction
        FROM 
			Import.SspTransactionStaging SspTransactionStaging
			LEFT OUTER JOIN Data.Interaction Interaction ON SspTransactionStaging.interaction = Interaction.InteractionName
        WHERE
			Interaction.InteractionName IS NULL
    )  Interaction;

    -----------------------------------------------------
    -- capture any unknown SspFrom asids found
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
    SELECT DISTINCT
        SspTransactionStaging.sspFrom,
        'UNKNOWN',
        'UNKNOWN',
        'UNKNOWN',
        '',
        'UNKNOWN',
        'UNKNOWN',
        @FileId
    FROM 
		Import.SspTransactionStaging SspTransactionStaging
		LEFT OUTER JOIN Data.AsidLookup AsidLookup ON SspTransactionStaging.sspFrom = AsidLookup.Asid
    WHERE 
		AsidLookup.Asid IS NULL;

    -----------------------------------------------------
    -- capture any unknown SspTo asids found
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
    SELECT DISTINCT
        SspTransactionStaging.sspTo,
        'UNKNOWN',
        'UNKNOWN',
        'UNKNOWN',
        '',
        'UNKNOWN',
        'UNKNOWN',
        @FileId
    FROM 
		Import.SspTransactionStaging SspTransactionStaging
		LEFT OUTER JOIN Data.AsidLookup AsidLookup ON SspTransactionStaging.sspTo = AsidLookup.Asid
	WHERE
		AsidLookup.Asid IS NULL;

    -----------------------------------------------------
    -- fix datetime offset into sql format
    -- (TODO is there a better way of doing this?)
    -----------------------------------------------------
    UPDATE 
		Import.SspTransactionStaging
    SET 
        _time = replace(_time, '+0000', '+00:00');

    -----------------------------------------------------
    -- install file data to destination table
    -----------------------------------------------------
    INSERT INTO Data.SspTransaction
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
    SELECT
        CONVERT(DATETIMEOFFSET, SspTransactionStaging._time),
        SspTransactionStaging.sspFrom,
        SspTransactionStaging.sspTo,
        SspTransactionStaging.SspTraceId,
        Interaction.InteractionId,
        SspTransactionStaging.responseCode,
        SspTransactionStaging.duration,
        SspTransactionStaging.responseSize,
        SspTransactionStaging.responseErrorMessage,
        SspTransactionStaging.method,
        @FileId
    FROM 
		Import.SspTransactionStaging SspTransactionStaging
    LEFT OUTER JOIN Data.Interaction Interaction ON SspTransactionStaging.interaction = Interaction.InteractionName

    SET @RowsAdded = @@ROWCOUNT;
    SET @RowsUpdated = 0;
