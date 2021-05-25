if (object_id('Import.ParseDateTime') is not null)
	drop function Import.ParseDateTime;

go

create function Import.ParseDateTime
(
	@DateTimeString varchar(20)
)
returns datetime2
as
begin
	
	-----------------------------------------------------
	-- parse datetime in format YYYYMMDDTHHmmss
	-----------------------------------------------------

	if (len(@DateTimeString) != 15)
		return convert(datetime2, 'error', 120);  -- force bad conversion

	declare @ExtractDateStringNewFormat varchar(30)
		= substring(@DateTimeString, 1, 4) + '-' 
		+ substring(@DateTimeString, 5, 2) + '-' 
		+ substring(@DateTimeString, 7, 2) + ' '
		+ substring(@DateTimeString, 10, 2) + ':'
		+ substring(@DateTimeString, 12, 2) + ':' 
		+ substring(@DateTimeString, 14, 2)

	return convert(datetime2, @ExtractDateStringNewFormat, 120);

end;
