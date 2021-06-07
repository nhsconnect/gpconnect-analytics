using gpconnect_analytics.Configuration.Infrastructure;
using gpconnect_analytics.Configuration.Infrastructure.Logging;
using gpconnect_analytics.DAL;
using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.Functions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace gpconnect_analytics.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            MappingExtensions.ConfigureMappingServices();
            builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
            builder.Services.AddScoped<IBlobService, BlobService>();
            builder.Services.AddScoped<IImportService, ImportService>();
            builder.Services.AddScoped<ISplunkService, SplunkService>();
            builder.Services.AddScoped<IDataService, DataService>();
            builder.Services.AddScoped<IEmailService, EmailService>();

            var configuration = builder.GetContext().Configuration;
            builder.Services.AddLogging(loggingBuilder => LoggingExtensions.ConfigureLoggingServices(loggingBuilder, configuration));
            builder.Services.AddHttpClient("SplunkApiClient", options => Configuration.Infrastructure.HttpClient.HttpClientExtensions.ConfigureHttpClient(options))
                .ConfigurePrimaryHttpMessageHandler(() => Configuration.Infrastructure.HttpClient.HttpClientExtensions.CreateHttpMessageHandler());
        }        
    }
}
