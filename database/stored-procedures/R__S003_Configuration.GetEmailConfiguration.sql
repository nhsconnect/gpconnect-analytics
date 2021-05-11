CREATE OR ALTER PROCEDURE Configuration.GetEmailConfiguration
AS
	SELECT
		SenderAddress,
		Hostname,
		Port,
		Encryption,
		AuthenticationRequired,
		Username,
		Password,
		DefaultSubject
	FROM
		Configuration.Email;
