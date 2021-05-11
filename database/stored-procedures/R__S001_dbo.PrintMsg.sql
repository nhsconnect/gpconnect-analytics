if (object_id('dbo.PrintMsg') is not null)
	drop procedure dbo.PrintMsg;

go

create procedure dbo.PrintMsg
(
	@Text varchar(8000)
)
as
	raiserror(@Text, 0, 1) with nowait;
