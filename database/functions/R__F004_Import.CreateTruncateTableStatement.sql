if exists (select object_id('Import.CreateTruncateTableStatement'))
	drop function Import.CreateTruncateTableStatement;

go

create function Import.CreateTruncateTableStatement
(
	@TableName varchar(200)
)
returns varchar(1000)
as
begin

	return 'truncate table ' + @TableName + ';';

end;
