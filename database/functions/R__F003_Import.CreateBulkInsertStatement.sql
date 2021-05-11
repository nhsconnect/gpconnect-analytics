if (object_id('Import.CreateBulkInsertStatement') is not null)
	drop function Import.CreateBulkInsertStatement;

go

create function Import.CreateBulkInsertStatement
(
	@TableName varchar(200),
	@FilePath varchar(500)
)
returns varchar(1000)
as
begin

	declare @DataSourceName varchar(255) = isnull(trim((select SqlExternalDataSourceName from Configuration.BlobStorage)), '');

	return
		'bulk insert ' + @TableName + ' ' +
		'from ''' + @FilePath + ''' ' +
		'with ' +
		'( ' +
		case 
			when @DataSourceName = '' then ''
			else 'data_source = ''' + @DataSourceName + ''', '
		end +
		'format = ''csv'', ' +
		'codepage = 65001, ' +
		'firstrow = 2, ' +
		'rowterminator = ''0x0a'', ' +
		'fieldquote = ''"'', ' +
		'tablock, ' +
		'fieldterminator = '','' ' +
		');';

end;