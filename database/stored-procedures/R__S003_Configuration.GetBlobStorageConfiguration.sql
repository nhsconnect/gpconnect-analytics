if (object_id('Configuration.GetBlobStorageConfiguration') is not null)
	drop procedure Configuration.GetBlobStorageConfiguration;

go

create procedure Configuration.GetBlobStorageConfiguration
as

	select
		BlobPrimaryKey,
		ConnectionString,
		ContainerName,
		QueueName,
		SqlExternalDataSourceName
	from Configuration.BlobStorage;