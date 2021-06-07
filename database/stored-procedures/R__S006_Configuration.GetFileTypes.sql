if (object_id('Configuration.GetFileTypes') is not null)
	drop procedure Configuration.GetFileTypes;

go

create procedure Configuration.GetFileTypes
as

	select
 		FileTypeId,
 		DirectoryName,
 		FileTypeFilePrefix,
 		SplunkQuery,
 		QueryFromBaseDate,
 		QueryPeriodHours,
		Enabled
	from Configuration.FileType;
