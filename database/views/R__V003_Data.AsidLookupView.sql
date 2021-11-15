IF (object_id('Data.AsidLookupView') IS NOT NULL)
    DROP VIEW Data.AsidLookupView;
GO

CREATE VIEW [Data].[AsidLookupView]
AS
	SELECT
		a.Asid,
		a.OdsCode,
	        a.OrgName,
		a.SupplierName
	FROM
		Data.AsidLookup a
	WHERE
		IsDeleted = 0
GO