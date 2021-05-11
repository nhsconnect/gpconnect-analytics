CREATE OR ALTER FUNCTION Import.ParseDateTime
(
	@DateTimeString VARCHAR(20)
)
RETURNS DATETIME2
AS
BEGIN
	-----------------------------------------------------
	-- parse datetime in format YYYYMMDDTHHmmss
	-----------------------------------------------------
	DECLARE @ExtractDateStringNewFormat VARCHAR(30)
		= SUBSTRING(@DateTimeString, 1, 4) + '-' 
			+ SUBSTRING(@DateTimeString, 5, 2) + '-' 
			+ SUBSTRING(@DateTimeString, 7, 2) + ' '
			+ SUBSTRING(@DateTimeString, 10, 2) + ':'
			+ SUBSTRING(@DateTimeString, 12, 2) + ':' 
			+ SUBSTRING(@DateTimeString, 14, 2)

	RETURN CONVERT(DATETIME2, @ExtractDateStringNewFormat, 120);
END;