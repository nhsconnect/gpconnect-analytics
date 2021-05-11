CREATE OR ALTER FUNCTION Import.CreateEnsureTableEmptyStatement
(
	@TableName VARCHAR(200)
)
RETURNS VARCHAR(1000)
AS
BEGIN
	RETURN 
		'IF EXISTS ' +
		'( ' +
		'SELECT * ' +
		'FROM ' + @TableName + 
		') ' +
		'BEGIN ' +
		'DECLARE @msg VARCHAR(8000) = ''' + @TableName + ' is not empty''; ' +
		'EXEC dbo.ThrowError @msg; ' +
		'END;';
END;
