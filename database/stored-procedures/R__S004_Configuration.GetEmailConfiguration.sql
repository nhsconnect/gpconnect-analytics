if exists (select object_id('Configuration.GetEmailConfiguration'))
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
		DefaultSubject
	from Configuration.Email;

