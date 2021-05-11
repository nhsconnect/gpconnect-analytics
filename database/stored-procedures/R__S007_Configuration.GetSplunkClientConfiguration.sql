if exists (select object_id('Configuration.GetSplunkClientConfiguration'))
	drop procedure Configuration.GetSplunkClientConfiguration;

go

create procedure Configuration.GetSplunkClientConfiguration
as

	select
		SplunkInstance,
		HostName,
		HostPort,
		BaseUrl,
		QueryParameters,
		QueryTimeout
	from Configuration.SplunkClient;
