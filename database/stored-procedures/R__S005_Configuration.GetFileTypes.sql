CREATE OR ALTER PROCEDURE Configuration.GetFileTypes
AS
	SELECT
 		FileTypeId,
 		DirectoryName,
 		FileTypeFilePrefix,
 		SplunkQuery,
 		QueryFromBaseDate,
 		QueryPeriodHours
	FROM
		Configuration.FileType;
