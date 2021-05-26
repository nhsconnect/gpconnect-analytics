if (object_id('Configuration.GetEmailConfiguration') is not null)
	drop procedure Configuration.GetEmailConfiguration;

go

create procedure Configuration.GetEmailConfiguration
as

	select
		SenderAddress,
		Hostname,
		Port,
		Encryption,
		AuthenticationRequired,
		Username,
		Password,
		DefaultSubject,
		RecipientAddress
	from Configuration.Email;

