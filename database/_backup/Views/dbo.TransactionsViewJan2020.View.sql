/****** Object:  View [dbo].[TransactionsViewJan2020]    Script Date: 27/04/2021 15:00:30 ******/
DROP VIEW IF EXISTS [dbo].[TransactionsViewJan2020]
GO
/****** Object:  View [dbo].[TransactionsViewJan2020]    Script Date: 27/04/2021 15:00:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[TransactionsViewJan2020] AS SELECT T.*, FORMAT(CAST(T._time AS DATE), 'd-MMM-yyyy') 'TransactionDate', DATEPART(DAY,CAST(T._time AS DATE)) 'TransactionDay', DATEPART(MONTH,CAST(T._time AS DATE)) 'TransactionMonth', DATEPART(YEAR,CAST(T._time AS DATE)) 'TransactionYear', O1.SiteNacs 'FromSiteNacs', O1.SiteName 'FromSiteName', O1.CcgNacs 'FromCcgNacs', O1.CcgName 'FromCcgName', O1.PartyKey 'FromPartyKey', O1.ASID 'FromASID', O1.SupplierName 'FromSupplierName', O1.ProductVersion 'FromProductVersion', O1.SiteType 'FromSiteType', O2.SiteNacs 'ToSiteNacs', O2.SiteName 'ToSiteName', O2.CcgNacs 'ToCcgNacs', O2.CcgName 'ToCcgName', O2.PartyKey 'ToPartyKey', O2.ASID 'ToASID', O2.SupplierName 'ToSupplierName', O2.ProductVersion 'ToProductVersion', O2.SiteType 'ToSiteType' FROM TransactionsJan2020 T LEFT OUTER JOIN OdsLookup O1 ON T.SSPFROM = O1.ASID LEFT OUTER JOIN OdsLookup O2 ON T.SSPTO = O2.ASID; 
GO
