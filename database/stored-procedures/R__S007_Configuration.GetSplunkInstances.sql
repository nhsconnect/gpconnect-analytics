CREATE OR ALTER PROCEDURE Configuration.GetSplunkInstances
AS
	SELECT
		SplunkInstance,
		SplunkInstanceGroup
	FROM
		Configuration.SplunkInstance;
