if (object_id('Import.RemoveDirectoryFromFilePath') is not null)
	drop function Import.RemoveDirectoryFromFilePath;

go

create function Import.RemoveDirectoryFromFilePath
(
	@FilePath varchar(500),
	@PathSeparator varchar(1)
)
returns varchar(500)
as
begin

	return substring(@FilePath, (Import.LastCharIndexOf(@PathSeparator, @FilePath) + 1), len(@FilePath));
	
end;
