using Bogus;
using Core.DTOs.Response.Configuration;
using Microsoft.Extensions.Configuration;

namespace Functions.Tests.TestHelpers;

public class ConfigurationHelpers
{
    public static IConfiguration CreateDefaultConfiguration(string connectionString)
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "ConnectionStrings:GPConnectAnalytics", connectionString }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        return configuration;
    }

    public static FilePathConstants GenerateFilePathConstants() =>
        new Faker<FilePathConstants>()
            .RuleFor(f => f.PathSeparator, "/")
            .RuleFor(f => f.ProjectNameFilePrefix, "proj_")
            .RuleFor(f => f.ComponentSeparator, "_")
            .RuleFor(f => f.FileExtension, ".csv")
            .Generate();
}