using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.DTO.Response.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IDataService _dataService;

        public EmailService(ILogger<EmailService> logger, IDataService dataService)
        {
            _dataService = dataService;
            _logger = logger;
        }

        public async Task SendProcessErrorEmail(Exception exc)
        {
            var result = await _dataService.ExecuteStoredProcedure<Email>("[Configuration].[GetEmailConfiguration]");
            var emailConfiguration = result.FirstOrDefault();
            var smtpClient = GetSmtpClient(emailConfiguration);

            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailConfiguration.SenderAddress),
                    IsBodyHtml = false,
                    Subject = emailConfiguration.DefaultSubject,
                    Body = exc.StackTrace,
                    To = { emailConfiguration.RecipientAddress }
                };
                smtpClient.Send(mailMessage);
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

        private SmtpClient GetSmtpClient(Email emailConfiguration)
        {            
            ServicePointManager.SecurityProtocol = GetSecurityProtocol(emailConfiguration.Encryption);
            var smtpClient = new SmtpClient
            {
                Host = emailConfiguration.Hostname,
                Port = emailConfiguration.Port,
                DeliveryFormat = SmtpDeliveryFormat.SevenBit,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true,
                Credentials = new NetworkCredential
                {
                    UserName = emailConfiguration.Username,
                    Password = emailConfiguration.Password
                }
            };

            return smtpClient;
        }

        private static SecurityProtocolType GetSecurityProtocol(string encryption)
        {
            return Enum.Parse<SecurityProtocolType>(encryption);
        }
    }
}
