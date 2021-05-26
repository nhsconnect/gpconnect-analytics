using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.DTO.Response.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL
{
    public class EmailService : IEmailService
    {
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<EmailService> _logger;
        private readonly SmtpClient _smtpClient;
        private Email _emailConfiguration;

        public EmailService(IConfigurationService configurationService, SmtpClient smtpClient, ILogger<EmailService> logger)
        {
            _configurationService = configurationService;
            _logger = logger;
            _smtpClient = smtpClient;
        }

        public async Task SendProcessErrorEmail(Exception exc)
        {
            _emailConfiguration = await _configurationService.GetEmailConfiguration();
            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailConfiguration.SenderAddress),
                    IsBodyHtml = false,
                    Subject = _emailConfiguration.DefaultSubject,
                    Body = exc.StackTrace,
                    To = { _emailConfiguration.RecipientAddress }
                };
                _smtpClient.Send(mailMessage);
            }
            catch (SmtpException smtpException)
            {
                _logger?.LogError(smtpException, "An SMTP error has occurred while attempting to send an email");
                throw;
            }
            catch (Exception exception)
            {
                _logger?.LogError(exception, "A general error has occurred while attempting to send an email");
                throw;
            }
        }
    }
}
