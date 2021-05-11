if (object_id('Import.CreateEnsureTableEmptyStatement') is not null)
	drop function Import.CreateEnsureTableEmptyStatement;

go

create function Import.CreateEnsureTableEmptyStatement
(
	@TableName varchar(200)
)
returns varchar(1000)
as
begin
	
	return 
		'if exists ' +
		'( ' +
		'select * ' +
		'from ' + @TableName + 
		') ' +
		'begin ' +
		'declare @msg varchar(8000) = ''' + @TableName + ' is not empty''; ' +
		'exec dbo.ThrowError @msg; ' +
		'end;';

end;
