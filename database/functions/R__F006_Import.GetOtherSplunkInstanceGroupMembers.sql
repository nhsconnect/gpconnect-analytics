if (object_id('Import.GetOtherSplunkInstanceGroupMembers') is not null)
	drop function Import.GetOtherSplunkInstanceGroupMembers;

go

create function Import.GetOtherSplunkInstanceGroupMembers
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
    	where s.SplunkInstance = @SplunkInstance
		and s2.SplunkInstance != @SplunkInstance;
