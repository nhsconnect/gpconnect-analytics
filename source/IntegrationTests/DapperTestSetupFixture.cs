using Core.Repositories;
using Core.Services.Interfaces;
using Dapper;
using DotNet.Testcontainers.Builders;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Testcontainers.MsSql;

namespace IntegrationTests;

public class DapperTestSetupFixture : IAsyncLifetime
{
    public MsSqlContainer Container;


    public async Task InitializeAsync()
    {
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

        // Step 1: Create the table
        var createTableSql = @"
            CREATE TABLE Users (
                Id INT IDENTITY(1,1) PRIMARY KEY, 
                Name NVARCHAR(100)
            )";

        using var connection = new SqlConnection(Container.GetConnectionString());
        await connection.ExecuteAsync(createTableSql);

        // Step 2: Create the stored procedure (must be executed separately)
        var createProcedureSql = @"
            CREATE PROCEDURE sp_TestAddUser 
                @Name NVARCHAR(100)
            AS
            BEGIN
                INSERT INTO Users (Name) VALUES (@Name);
                SELECT SCOPE_IDENTITY() AS Id; -- Correct way to return the inserted ID
            END;
    ";

        await connection.ExecuteAsync(createProcedureSql);

        // Step 3: Add Query SP
        var createQuerySql = @"
            CREATE PROCEDURE sp_TestGetAllUsers
            AS
            BEGIN
                SELECT * FROM Users;
            END;
        ";

        await connection.ExecuteAsync(createQuerySql);
    }

    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}