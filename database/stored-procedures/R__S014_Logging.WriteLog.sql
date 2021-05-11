CREATE OR ALTER PROCEDURE Logging.WriteLog
(
	@Application VARCHAR(100),
	@Logged DATETIME2,
	@Level VARCHAR(100),
	@Message VARCHAR(8000),
	@Logger VARCHAR(8000), 
	@Callsite VARCHAR(8000), 
	@Exception VARCHAR(8000)
)
AS
	INSERT INTO Logging.Log
	(
		Application,
		Logged,
		Level,
		Message,
		Logger,
		Callsite,
		Exception
	)
	VALUES
	(
		@Application,
		@Logged,
		@Level,
		@Message,
		@Logger,
		@Callsite,
		@Exception
	)
