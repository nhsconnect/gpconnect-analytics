using gpconnect_analytics.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Mail;

namespace gpconnect_analytics.Configuration.Infrastructure
{
    public static class SmtpClientExtensions
    {
        public static void AddSmtpClientServices(this IServiceCollection services, IConfiguration configuration)
        {
            ServicePointManager.SecurityProtocol = GetSecurityProtocol(configuration);
            services.AddScoped(serviceProvider => new SmtpClient
            {
                Host = configuration.GetSection("Email:host_name").Value,
                Port = configuration.GetSection("Email:port").Value.StringToInteger(),
                DeliveryFormat = SmtpDeliveryFormat.SevenBit,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true,
                Credentials = GetCredentials(configuration)
            });
        }

        private static SecurityProtocolType GetSecurityProtocol(IConfiguration configuration)
        {
            var encryptionMethod = configuration.GetSection("Email:encryption").GetConfigurationString("Tls12", false);
            return Enum.Parse<SecurityProtocolType>(encryptionMethod);
        }

        private static ICredentialsByHost GetCredentials(IConfiguration configuration)
        {
            return new NetworkCredential
            {
                UserName = configuration.GetSection("Email:user_name").GetConfigurationString(null, true),
                Password = configuration.GetSection("Email:password").GetConfigurationString(null, true)
            };
        }
    }
}
