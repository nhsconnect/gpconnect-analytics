CREATE OR ALTER FUNCTION Import.CreateBulkInsertStatement
(
	@TableName VARCHAR(200),
	@FilePath VARCHAR(500)
)
RETURNS VARCHAR(1000)
AS
BEGIN

	DECLARE @DataSourceName VARCHAR(255) = ISNULL(TRIM((SELECT SqlExternalDataSourceName FROM Configuration.BlobStorage)), '');

	RETURN
		'BULK INSERT ' + @TableName + ' ' +
		'FROM ''' + @FilePath + ''' ' +
		'WITH ' +
		'( ' +
		CASE 
			WHEN @DataSourceName = '' THEN ''
			ELSE 'DATA_SOURCE = ''' + @DataSourceName + ''', '
		END +
		'FORMAT = ''CSV'', ' +
		'CODEPAGE = 65001, ' +
		'FIRSTROW = 2, ' +
		'ROWTERMINATOR = ''0x0a'', ' +
		'FIELDQUOTE = ''"'', ' +
		'TABLOCK, ' +
		'FIELDTERMINATOR = '','' ' +
		');';
END
