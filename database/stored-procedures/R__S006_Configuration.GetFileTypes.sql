if exists (select object_id('Configuration.GetFileTypes'))
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
 		QueryPeriodHours
	from Configuration.FileType;
