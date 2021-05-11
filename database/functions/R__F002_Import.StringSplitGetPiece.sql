CREATE OR ALTER FUNCTION Import.StringSplitGetPiece
(
	@String VARCHAR(1000),
	@Separator VARCHAR(1),
	@PieceNumber SMALLINT
)
RETURNS VARCHAR(1000)
AS
BEGIN

	DECLARE @Result VARCHAR(1000);

	SELECT 
		@Result = s.value
	FROM
	(
		SELECT 
			ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS id,
			value
		FROM STRING_SPLIT(@String, @Separator)
	) s
	WHERE s.id = @PieceNumber;
	
	RETURN @Result;

END;

