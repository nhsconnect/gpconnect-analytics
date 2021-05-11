if exists (select object_id('Configuration.GetBlobStorageConfiguration'))
	drop procedure Configuration.GetBlobStorageConfiguration;

go

create or alter procedure Configuration.GetBlobStorageConfiguration
as

	select
		BlobPrimaryKey,
		ConnectionString,
		ContainerName,
		QueueName,
		SqlExternalDataSourceName
	from Configuration.BlobStorage;