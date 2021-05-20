CREATE OR ALTER PROCEDURE Configuration.GetSplunkClientConfiguration
AS
	SELECT
		SplunkInstance,
		HostName,
		HostPort,
		BaseUrl,
		QueryParameters,
		QueryTimeout,
		ApiToken
	FROM 
		Configuration.SplunkClient;
