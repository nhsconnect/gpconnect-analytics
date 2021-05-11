if (object_id('Import.GetSplunkInstanceGroupMembers') is not null)
	drop function Import.GetSplunkInstanceGroupMembers;

go

create function Import.GetSplunkInstanceGroupMembers
(
	@SplunkInstance varchar(200)
)
returns table
as

	return 
	    select 
	        s2.SplunkInstance
    	from Configuration.SplunkInstance s 
    	inner join Configuration.SplunkInstance s2 on s.SplunkInstanceGroup = s2.SplunkInstanceGroup
    	where s.SplunkInstance = @SplunkInstance;

