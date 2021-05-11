if exists (select object_id('Configuration.GetFilePathConstants'))
	drop procedure Configuration.GetFilePathConstants;

go

create procedure Configuration.GetFilePathConstants
as

	select
		PathSeparator,
		ProjectNameFilePrefix,
		ComponentSeparator,
		FileExtension
	from Configuration.FilePathConstants;
