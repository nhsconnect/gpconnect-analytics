using Core.Repositories;
using Core.Services.Interfaces;
using Dapper;
using DotNet.Testcontainers.Builders;
using Functions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Testcontainers.MsSql;

namespace IntegrationTests;

public class MsSqlContainerFixture : IAsyncLifetime
{
    public MsSqlContainer Container;
    private HierarchyProviderConsumerRepo _repo;
    private FakeLogger _logger;


    public async Task InitializeAsync()
    {
        _logger = new FakeLogger();

        Container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPortBinding(1443, true)
            .WithPassword("P@ssw0rd123")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
            .Build();

        // Create docker container for testing
        await Container.StartAsync();

        var mockCoreConfigurationService = new Mock<ICoreConfigurationService>();

        // Set up the GetConnectionString method to return container's connection string
        mockCoreConfigurationService.Setup(x => x.GetConnectionString(It.IsAny<string>()))
            .Returns(Container.GetConnectionString());


        _repo = new HierarchyProviderConsumerRepo(mockCoreConfigurationService.Object, new DapperWrapper(),
            new SqlConnectionFactory(), _logger);

        await using var sqlConnection = new SqlConnection(Container.GetConnectionString());
        await sqlConnection.OpenAsync();
        await sqlConnection.ExecuteAsync("CREATE SCHEMA DATA");
        await sqlConnection.ExecuteAsync(@"SET ANSI_NULLS ON
                                             BEGIN
                                             SET QUOTED_IDENTIFIER ON
                                             END
                                             BEGIN
                                             CREATE TABLE [Data].[HierarchyProviderConsumers](
                                              [OdsCode] [nvarchar](450) NOT NULL,
                                              [PracticeName] [nvarchar](max) NULL,
                                              [RegisteredPatientCount] [int] NOT NULL,
                                              [RegionCode] [nvarchar](max) NULL,
                                              [RegionName] [nvarchar](max) NULL,
                                              [Icb22Name] [nvarchar](max) NULL,
                                              [PcnName] [nvarchar](max) NULL,
                                              [Appointments13000] [int] NOT NULL
                                             ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
                                             END
                                             BEGIN
                                             SET ANSI_PADDING ON
                                             END
                                             BEGIN
                                             ALTER TABLE [Data].[HierarchyProviderConsumers] ADD  CONSTRAINT [PK_HierarchyProviderConsumers] PRIMARY KEY CLUSTERED 
                                             (
                                              [OdsCode] ASC
                                             )WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
                                             END");

        await sqlConnection.CloseAsync();
    }

    public async Task DisposeAsync() => await Container.DisposeAsync();
}