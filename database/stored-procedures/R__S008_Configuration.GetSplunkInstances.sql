if exists (select object_id('Configuration.GetSplunkInstances'))
	drop procedure Configuration.GetSplunkInstances;

go

create procedure Configuration.GetSplunkInstances
as

	select
		SplunkInstance,
		SplunkInstanceGroup
	from Configuration.SplunkInstance;

