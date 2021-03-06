/****** Object:  StoredProcedure [dbo].[UpdateSplunkTransactionsImportControl]    Script Date: 27/04/2021 15:00:30 ******/
DROP PROCEDURE IF EXISTS [dbo].[UpdateSplunkTransactionsImportControl]
GO
/****** Object:  StoredProcedure [dbo].[UpdateSplunkTransactionsImportControl]    Script Date: 27/04/2021 15:00:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[UpdateSplunkTransactionsImportControl]
AS
BEGIN
	UPDATE
		SplunkTransactionsImportControl
	SET
		StartDate = (SELECT ISNULL(DATEADD(D, -1, CAST(MAX(_time) AS DATE)), CAST(DATEADD(D, -1, GETDATE()) AS DATE)) FROM TransactionsImport),
		LastExecutionDate = GETDATE()
END
GO
