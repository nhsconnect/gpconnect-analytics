using Core;
using Core.Repositories;
using Core.Services.Interfaces;
using Functions;
using Functions.Configuration;
using Functions.Configuration.Infrastructure.HttpClient;
using Functions.Configuration.Infrastructure.Logging;
using Functions.Configuration.Infrastructure.Mapping;
using Functions.Services;
using Functions.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var builder = Host.CreateDefaultBuilder(args)
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // Configure your services here
        MappingExtensions.ConfigureMappingServices();
        services.AddSingleton<ICoreConfigurationService, CoreConfigurationService>();
        services.AddScoped<IConfigurationService, ConfigurationService>();
        services.AddScoped<IBlobService, BlobService>();
        services.AddScoped<IImportService, ImportService>();
        services.AddScoped<ISplunkService, SplunkService>();
        services.AddScoped<IDataService, DataService>();
        services.AddScoped<IBatchService, BatchService>();
        services.AddScoped<ILoggingService, LoggingService>();
        services.AddScoped<IDapperWrapper, DapperWrapper>();
        services.AddScoped<IHierarchyProviderConsumerRepo, HierarchyProviderConsumerRepo>();
        services.AddSingleton<IConnectionFactory, SqlConnectionFactory>();
        services.AddSingleton<IEmailConfigurationProvider, EmailConfigurationProvider>();
        services.AddScoped<ITimeProvider, TimeProvider>();

        // Configure logging with email configuration provider
        services.AddLogging(loggingBuilder =>
        {
            var emailProvider = services.BuildServiceProvider().GetRequiredService<IEmailConfigurationProvider>();
            LoggingExtensions.ConfigureLoggingServices(loggingBuilder, context.Configuration, emailProvider);
        });

        // Configure HttpClient
        services.AddHttpClient("SplunkApiClient", options =>
                HttpClientExtensions
                    .ConfigureHttpClient(options))
            .ConfigurePrimaryHttpMessageHandler(() =>
                HttpClientExtensions
                    .CreateHttpMessageHandler());
    });


var host = builder.Build();
host.Run();