using Core.Services.Interfaces;
using Dapper;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Functions;
using Functions.Configuration;
using IntegrationTests.TestHelpers;
using Microsoft.Data.SqlClient;
using Moq;
using Testcontainers.MsSql;

namespace IntegrationTests;

public class EmailConfigurationProviderTests : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("P@ssw0rd123")
        .WithPortBinding(1433, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
        .Build();


    [Fact]
    public void GetEmailConfiguration_ShouldReturnEmailConfiguration()
    {
        // Arrange
        var configuration = ConfigurationHelpers.CreateDefaultConfiguration(_container.GetConnectionString());
        var connectionFactory = new SqlConnectionFactory();
        var emailConfiguration = new EmailConfigurationProvider(connectionFactory);

        // Act
        var result = emailConfiguration.GetEmailConfiguration(configuration);

        // Assert
        result.Should().NotBeNull();
        result!.SenderAddress.Should().Be("gpconnectappointmentchecker.test@test.net");
        result!.Hostname.Should().Be("fakeHost");
        result!.Port.Should().Be(587);
        result!.Encryption.Should().Be("Tls12");
        result!.AuthenticationRequired.Should().Be(true);
        result.DefaultSubject.Should().Be("GP Connect Analytics - Error");
        result.RecipientAddress.Should().Be("gpconnectappointmentchecker.test@test.net");
        result.Username.Should().Be("gpconnectappointmentchecker.test@test.net");
        result.Password.Should().Be("fakePassword123!");
    }

    public async Task InitializeAsync()
    {
        // Create docker container for testing
        await _container.StartAsync();

        var mockCoreConfigurationService = new Mock<ICoreConfigurationService>();

        // Set up the GetConnectionString method to return container's connection string
        mockCoreConfigurationService.Setup(x => x.GetConnectionString(It.IsAny<string>()))
            .Returns(_container.GetConnectionString());

        await using var sqlConnection = new SqlConnection(_container.GetConnectionString());
        await sqlConnection.OpenAsync();
        await sqlConnection.ExecuteAsync("CREATE SCHEMA Configuration");

        // CREATE EMAIL PROCEDURE
        using (var command = new SqlCommand(@"
                create procedure Configuration.GetEmailConfiguration as
						select
							SenderAddress,
							Hostname,
							Port,
							Encryption,
							AuthenticationRequired,
							Username,
							Password,
							DefaultSubject,
							RecipientAddress
						from Configuration.Email;", sqlConnection))
        {
            await command.ExecuteNonQueryAsync();
        }

        // CREATE EMAIL TABLE
        await sqlConnection.ExecuteAsync("""
                                         BEGIN
                                          SET ANSI_NULLS ON
                                          SET QUOTED_IDENTIFIER ON
                                         
                                          CREATE TABLE [Configuration].[Email](
                                         		[SingleRowLock] [bit] NOT NULL,
                                         		[SenderAddress] [varchar](100) NOT NULL,
                                         		[Hostname] [varchar](100) NOT NULL,
                                         		[Port] [smallint] NOT NULL,
                                         		[Encryption] [varchar](10) NOT NULL,
                                         		[AuthenticationRequired] [bit] NOT NULL,
                                         		[Username] [varchar](100) NOT NULL,
                                         		[Password] [varchar](100) NOT NULL,
                                         		[DefaultSubject] [varchar](100) NOT NULL,
                                         		[RecipientAddress] [varchar](100) NOT NULL
                                          ) ON [PRIMARY]
                                         END
                                         """);

        // SEED TABLE
        await sqlConnection.ExecuteAsync("""
                                         INSERT INTO [Configuration].[Email](
                                         SingleRowLock,
                                         SenderAddress,
                                         Hostname,
                                         Port,
                                         Encryption,
                                         AuthenticationRequired,
                                         Username,
                                         Password,
                                         DefaultSubject,
                                         RecipientAddress)
                                         VALUES (1,
                                                'gpconnectappointmentchecker.test@test.net',
                                                'fakeHost',
                                                587,
                                                'Tls12',
                                                1,
                                                'gpconnectappointmentchecker.test@test.net',
                                                'fakePassword123!',
                                                'GP Connect Analytics - Error',
                                                'gpconnectappointmentchecker.test@test.net')
                                         """);

        await sqlConnection.CloseAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();
}