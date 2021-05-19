if (object_id('Data.SspTransactionView') is not null)
    drop view Data.SspTransactionView;

go

create view Data.SspTransactionView
as

    select 
        s.SspTransactionId,
        s.Time,
        s.FromAsid,
        aFrom.OdsCode as FromOdsCode,
        aFrom.OrgName as FromOrgName,
        aFrom.SupplierName as FromSupplierName,
        s.ToAsid,
        aTo.OdsCode as ToOdsCode,
        aTo.OrgName as ToOrgName,
        aTo.SupplierName as ToSupplierName,
        s.SspTraceId,
        i.InteractionName,
        s.ResponseCode,
        s.Duration,
        s.ResponseSize,
        s.ResponseErrorMessage,
        s.Method
    from Data.SspTransaction s
    inner join Data.Interaction i on s.InteractionId = i.InteractionId
    inner join Data.AsidLookup aFrom on s.FromAsid = aFrom.Asid
    inner join Data.AsidLookup aTo on s.ToAsid = aTo.Asid;

