CREATE OR ALTER VIEW Data.SspTransactionView
AS

    SELECT 
        s.SspTransactionId,
        s.Time,
        s.FromAsid,
        aFrom.OdsCode AS FromOdsCode,
        aFrom.OrgName AS FromOrgName,
        aFrom.SupplierName AS FromSupplierName,
        s.ToAsid,
        aTo.OdsCode AS ToOdsCode,
        aTo.OrgName AS ToOrgName,
        aTo.SupplierName AS ToSupplierName,
        s.SspTraceId,
        i.InteractionName,
        s.ResponseCode,
        s.Duration,
        s.ResponseSize,
        s.ResponseErrorMessage,
        s.Method
    FROM Data.SspTransaction s
    INNER JOIN Data.Interaction i ON s.InteractionId = i.InteractionId
    INNER JOIN Data.AsidLookup aFrom ON s.FromAsid = aFrom.Asid
    INNER JOIN Data.AsidLookup aTo ON s.ToAsid = aTo.Asid

