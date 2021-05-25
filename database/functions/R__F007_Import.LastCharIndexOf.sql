if (object_id('Import.LastCharIndexOf') is not null)
	drop function Import.LastCharIndexOf;

go

create function Import.LastCharIndexOf
(
	@StringToFind varchar(max), 
	@StringToSearch varchar(max)
)
returns integer
as
begin

	if (charindex(@StringToFind, @StringToSearch) = 0)
		return 0;

	return (len(@StringToSearch) - charindex(@StringToFind, reverse(@StringToSearch))) + 1;

end;
