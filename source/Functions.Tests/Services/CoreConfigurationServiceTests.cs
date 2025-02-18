using FluentAssertions;
using Functions.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Functions.Tests.Services;

public class CoreConfigurationServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly CoreConfigurationService _configurationService;

    public CoreConfigurationServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "ConnectionStrings:GPConnectAnalytics", "Server=myServer;Database=myDB;User Id=myUser;Password=myPass;" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();


        _configurationService = new CoreConfigurationService(configuration);
    }

    [Fact]
    public void GetConnectionString_ShouldReturnConnectionString_WhenValidNameIsProvided()
    {
        // Arrange
        const string connectionName = "GPConnectAnalytics";
        const string expectedConnectionString = "Server=myServer;Database=myDB;User Id=myUser;Password=myPass;";

        // Act
        var result = _configurationService.GetConnectionString(connectionName);

        // Assert
        result.Should().Be(expectedConnectionString);
    }


    [Fact]
    public void GetConnectionString_ShouldThrowArgumentException_WhenConnectionStringIsNull()
    {
        // Arrange
        const string connectionName = "InvalidConnection";

        // Act
        Action act = () => _configurationService.GetConnectionString(connectionName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("No connection string with given name");
    }
}