if exists (select object_id('Import.StringSplitGetPiece'))
	drop function Import.StringSplitGetPiece;

go

create function Import.StringSplitGetPiece
(
	@String varchar(1000),
	@Separator varchar(1),
	@PieceNumber smallint
)
returns varchar(1000)
as
begin

	declare @Result varchar(1000);

	select 
		@Result = s.value
	from
	(
		select 
			row_number() over (order by (select null)) as id,
			value
		from string_split(@String, @Separator)
	) s
	where s.id = @PieceNumber;
	
	return @Result;

end;
