/****** Object:  StoredProcedure [dbo].[ImportSplunkTransaction]    Script Date: 27/04/2021 15:00:30 ******/
DROP PROCEDURE IF EXISTS [dbo].[ImportSplunkTransaction]
GO
/****** Object:  StoredProcedure [dbo].[ImportSplunkTransaction]    Script Date: 27/04/2021 15:00:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[ImportSplunkTransaction]
	@_time DATETIME,
	@sspFrom NVARCHAR(1000),
	@sspTo NVARCHAR(1000),
	@fromOrg NVARCHAR(1000),
	@fromSupplier NVARCHAR(1000),
	@toOrg NVARCHAR(1000),
	@toSupplier NVARCHAR(1000),
	@interaction NVARCHAR(1000),
	@responseCode NVARCHAR(1000),
	@duration NVARCHAR(1000),
	@responseSize NVARCHAR(1000),
	@responseErrorMessage NVARCHAR(1000),
	@method NVARCHAR(1000)
AS
BEGIN
	INSERT INTO [dbo].[TransactionsImport]
           ([_time]
           ,[sspFrom]
           ,[fromOrg]
           ,[fromSupplier]
           ,[sspTo]
           ,[toOrg]
           ,[toSupplier]
           ,[interaction]
           ,[responseCode]
           ,[duration]
           ,[responseSize]
           ,[responseErrorMessage]
           ,[method])
     VALUES
           (@_time
           ,@sspFrom
           ,@sspTo
           ,@fromSupplier
           ,@sspTo
           ,@toOrg
           ,@toSupplier
           ,@interaction
           ,@responseCode
           ,@duration
           ,@responseSize
           ,@responseErrorMessage
           ,@method)
END
GO
