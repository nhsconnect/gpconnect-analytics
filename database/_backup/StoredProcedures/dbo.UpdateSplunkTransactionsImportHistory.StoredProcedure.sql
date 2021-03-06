/****** Object:  StoredProcedure [dbo].[UpdateSplunkTransactionsImportHistory]    Script Date: 27/04/2021 15:00:30 ******/
DROP PROCEDURE IF EXISTS [dbo].[UpdateSplunkTransactionsImportHistory]
GO
/****** Object:  StoredProcedure [dbo].[UpdateSplunkTransactionsImportHistory]    Script Date: 27/04/2021 15:00:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE   PROCEDURE [dbo].[UpdateSplunkTransactionsImportHistory]
	@ImportDate DATETIME,
	@EmailSender VARCHAR(255),
	@EmailSubject VARCHAR(255),
	@EmailSent DATETIME,
	@AttachmentFilename VARCHAR(255),
	@AttachmentRowCount INT,
	@AttachmentStartRow VARCHAR(2000),
	@AttachmentEndRow VARCHAR(2000),
	@ImportRowCount INT,
	@ImportStartRow VARCHAR(2000),
	@ImportEndRow VARCHAR(2000)
AS
BEGIN	
	INSERT INTO [dbo].[SplunkTransactionsImportHistory]
    (
		[ImportDate]
        ,[EmailSender]
        ,[EmailSubject]
        ,[EmailSent]
        ,[AttachmentFilename]
        ,[AttachmentRowCount]
        ,[AttachmentStartRow]
        ,[AttachmentEndRow]
        ,[ImportRowCount]
        ,[ImportStartRow]
        ,[ImportEndRow]
	)
    VALUES
	(
		@ImportDate,
		@EmailSender,
		@EmailSubject,
		@EmailSent,
		@AttachmentFilename,
		@AttachmentRowCount,
		@AttachmentStartRow,
		@AttachmentEndRow,
		@ImportRowCount,
		@ImportStartRow,
		@ImportEndRow
	)
END
GO
