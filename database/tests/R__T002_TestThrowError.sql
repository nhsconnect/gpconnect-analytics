declare @ErrorWasCaught bit = 0;

begin try
    exec dbo.ThrowError 'Test error';
end try
begin catch
    set @ErrorWasCaught = 1;
end catch;

if (@ErrorWasCaught = 0)
begin
    raiserror('Failure in dbo.ThrowError test', 18, 10) with nowait;
end;
