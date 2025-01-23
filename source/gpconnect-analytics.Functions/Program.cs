using Core;
using Core.Repositories;
using Core.Services.Interfaces;
using function_app.Configuration.Infrastructure.Logging;
using function_app.Configuration.Infrastructure.Mapping;
using function_app.Services;
using function_app.Services.Interfaces;
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
        services.AddScoped<IConnectionFactory, SqlConnectionFactory>();


        // Configure logging
        services.AddLogging(loggingBuilder =>
            LoggingExtensions.ConfigureLoggingServices(loggingBuilder, context.Configuration));

        // Configure HttpClient
        services.AddHttpClient("SplunkApiClient", options =>
                gpconnect_analytics.Configuration.Infrastructure.HttpClient.HttpClientExtensions
                    .ConfigureHttpClient(options))
            .ConfigurePrimaryHttpMessageHandler(() =>
                gpconnect_analytics.Configuration.Infrastructure.HttpClient.HttpClientExtensions
                    .CreateHttpMessageHandler());
    });


var host = builder.Build();
host.Run();