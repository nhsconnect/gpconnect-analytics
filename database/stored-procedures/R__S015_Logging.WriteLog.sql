if exists (select object_id('Import.WriteLog'))
	drop procedure Import.WriteLog;

go

create procedure Logging.WriteLog
(
	@Application varchar(100),
	@Logged datetime2,
	@Level varchar(100),
	@Message varchar(8000),
	@Logger varchar(8000), 
	@Callsite varchar(8000), 
	@Exception varchar(8000)
)
as
	insert into Logging.Log
	(
		Application,
		Logged,
		Level,
		Message,
		Logger,
		Callsite,
		Exception
	)
	values
	(
		@Application,
		@Logged,
		@Level,
		@Message,
		@Logger,
		@Callsite,
		@Exception
	);
