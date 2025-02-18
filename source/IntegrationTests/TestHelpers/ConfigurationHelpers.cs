using Microsoft.Extensions.Configuration;

namespace IntegrationTests.TestHelpers;

public class ConfigurationHelpers
{
    public static IConfiguration CreateDefaultConfiguration(string connectionString = "")
    {
        var connection =
            string.IsNullOrEmpty(connectionString)
                ? "Server=myServer;Database=myDB;User Id=myUser;Password=myPass;"
                : connectionString;

        var inMemorySettings = new Dictionary<string, string?>
        {
            { "ConnectionStrings:GPConnectAnalytics", connection }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        return configuration;
    }
}