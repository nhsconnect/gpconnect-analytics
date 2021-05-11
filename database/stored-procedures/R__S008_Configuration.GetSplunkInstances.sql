if (object_id('Configuration.GetSplunkInstances') is not null)
	drop procedure Configuration.GetSplunkInstances;

go

create procedure Configuration.GetSplunkInstances
as

	select
		SplunkInstance,
		SplunkInstanceGroup
	from Configuration.SplunkInstance;

