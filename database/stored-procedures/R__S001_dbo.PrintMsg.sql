if exists (select object_id('dbo.PrintMsg'))
	drop procedure dbo.PrintMsg;

go

create procedure dbo.PrintMsg
(
	@Text varchar(8000)
)
as
	raiserror(@Text, 0, 1) with nowait;
