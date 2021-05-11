DECLARE @ErrorWasCaught BIT = 0;

BEGIN TRY
    EXEC dbo.ThrowError 'Test error';
END TRY
BEGIN CATCH
    SET @ErrorWasCaught = 1;
END CATCH

IF (@ErrorWasCaught = 0)
BEGIN
    RAISERROR('Failure in dbo.ThrowError test', 18, 10) WITH NOWAIT;
END;