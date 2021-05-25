if (object_id('dbo.ThrowError') is not null)
	drop procedure dbo.ThrowError;

go

create procedure dbo.ThrowError
(
	@Text varchar(8000)
)
as
	raiserror(@Text, 18, 10) with nowait;

