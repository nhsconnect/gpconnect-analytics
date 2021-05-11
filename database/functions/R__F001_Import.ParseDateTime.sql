if exists (select object_id('Import.ParseDateTime'))
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
	declare @ExtractDateStringNewFormat varchar(30)
		= substring(@DateTimeString, 1, 4) + '-' 
		+ substring(@DateTimeString, 5, 2) + '-' 
		+ substring(@DateTimeString, 7, 2) + ' '
		+ substring(@DateTimeString, 10, 2) + ':'
		+ substring(@DateTimeString, 12, 2) + ':' 
		+ substring(@DateTimeString, 14, 2)

	return convert(datetime2, @ExtractDateStringNewFormat, 120);

end;
