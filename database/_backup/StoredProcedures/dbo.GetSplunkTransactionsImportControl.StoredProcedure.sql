/****** Object:  StoredProcedure [dbo].[GetSplunkTransactionsImportControl]    Script Date: 27/04/2021 15:00:30 ******/
DROP PROCEDURE IF EXISTS [dbo].[GetSplunkTransactionsImportControl]
GO
/****** Object:  StoredProcedure [dbo].[GetSplunkTransactionsImportControl]    Script Date: 27/04/2021 15:00:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetSplunkTransactionsImportControl]	
AS
BEGIN
	SELECT 
		[StartDate],
		[LastExecutionDate],
		[FilterThresholdInDays],
		[SourceUrl],
		[BlobStoragePrimaryKey],
		[BlobStoragePrimaryConnectionString],
		[BlobStorageContainerName],
		[BlobStorageContainerLocationUrl]
	FROM
		[dbo].[SplunkTransactionsImportControl]
END
GO
