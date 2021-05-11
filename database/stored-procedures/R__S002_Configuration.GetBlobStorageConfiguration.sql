CREATE OR ALTER PROCEDURE Configuration.GetBlobStorageConfiguration
AS
	SELECT
		BlobPrimaryKey,
		ConnectionString,
		ContainerName,
		QueueName,
		SqlExternalDataSourceName
	FROM
		Configuration.BlobStorage;
