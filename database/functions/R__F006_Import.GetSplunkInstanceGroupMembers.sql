CREATE OR ALTER FUNCTION Import.GetSplunkInstanceGroupMembers
(
	@SplunkInstance VARCHAR(200)
)
RETURNS TABLE
AS
	RETURN 
	    SELECT 
	        SplunkInstance2.SplunkInstance
    	FROM
			Configuration.SplunkInstance SplunkInstance 
    		INNER JOIN Configuration.SplunkInstance SplunkInstance2 on SplunkInstance.SplunkInstanceGroup = SplunkInstance2.SplunkInstanceGroup
    	WHERE
			SplunkInstance.SplunkInstance = @SplunkInstance;

