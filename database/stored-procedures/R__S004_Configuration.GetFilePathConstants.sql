CREATE OR ALTER PROCEDURE Configuration.GetFilePathConstants
AS
	SELECT
		PathSeparator,
		ProjectNameFilePrefix,
		ComponentSeparator,
		FileExtension
	FROM
		Configuration.FilePathConstants;
