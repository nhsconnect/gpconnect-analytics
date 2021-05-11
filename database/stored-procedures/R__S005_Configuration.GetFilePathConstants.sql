if (object_id('Configuration.GetFilePathConstants') is not null)
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
