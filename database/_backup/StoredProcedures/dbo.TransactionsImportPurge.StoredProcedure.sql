/****** Object:  StoredProcedure [dbo].[TransactionsImportPurge]    Script Date: 27/04/2021 15:00:30 ******/
DROP PROCEDURE IF EXISTS [dbo].[TransactionsImportPurge]
GO
/****** Object:  StoredProcedure [dbo].[TransactionsImportPurge]    Script Date: 27/04/2021 15:00:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[TransactionsImportPurge]	
AS
BEGIN
TRUNCATE TABLE TransactionsImport
END
GO
