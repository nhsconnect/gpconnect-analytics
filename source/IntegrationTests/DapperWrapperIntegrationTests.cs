using Bogus;
using Core.Repositories;
using Dapper;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace IntegrationTests;

public class DapperWrapperIntegrationTests(DapperTestSetupFixture fixture) : IClassFixture<DapperTestSetupFixture>
{
    private readonly string _connectionString = fixture.Container.GetConnectionString();
    private readonly DapperWrapper _dapperWrapper = new();
    private readonly Faker _faker = new();

    [Fact]
    public async Task ExecuteStoredProcedureAsync_Should_Insert_User()
    {
        // Arrange
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var name = _faker.Name.FullName();
        var parameters = new { Name = name };

        // Act
        var result = await _dapperWrapper.ExecuteStoredProcedureAsync<int>(connection, "sp_TestAddUser", parameters);

        // Assert
        result.Should().BeGreaterThan(0);

        var users = await connection.QueryAsync<string>("SELECT Name FROM Users WHERE NAME = @name", new { name });
        users.Should().Contain(name);
    }

    [Fact]
    public async Task ExecuteSqlAsync_Should_Insert_User()
    {
        // Arrange
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var name = _faker.Name.FullName();
        var sql = "INSERT INTO Users (Name) VALUES(@name)";

        // Act
        await _dapperWrapper.ExecuteAsync(connection, sql, new { name });

        // Assert
        var users = await connection.QueryAsync<string>("SELECT Name FROM Users WHERE NAME = @name", new { name });
        users.Should().Contain(name);
    }

    [Fact]
    public async Task QueryStoredProcedureAsync_ShouldReturn_ExpectedQueryResult()
    {
        // Arrange
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var name = _faker.Name.FullName();
        var parameters = new { name };

        await InsertUser(name, connection);

        // Act
        var result =
            await _dapperWrapper.QueryStoredProcedureAsync<TestUser>(connection, "sp_TestGetAllUsers", new { });

        // Assert
        result.Should().Contain(x => x.Name == name);
    }

    [Fact]
    public async Task QueryAsync_Should_QueryDb()
    {
        // Arrange
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var parameters = new { name = _faker.Name.FullName() };
        await InsertUser(parameters.name, connection);

        // Act
        var result =
            await _dapperWrapper.QueryAsync<TestUser>(connection, "SELECT name FROM USERS WHERE name = @name",
                parameters);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().HaveCount(1);
        result.First().Should().BeEquivalentTo(new TestUser
        {
            Name = parameters.name
        });
    }

    // ------- ERRORS

    [Fact]
    public async Task ExecuteStoredProcedureAsync_Should_Throw_Exception_On_Error()
    {
        // Arrange
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var parameters = new { Name = _faker.Name.FullName() };

        // Act & Assert
        _dapperWrapper.Invoking(x =>
                x.ExecuteStoredProcedureAsync<int>(connection, "sp_NonExistentProcedure", parameters))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Error executing stored procedure: sp_NonExistentProcedure");
    }

    [Fact]
    public async Task QueryStoredProcedureAsync_Should_Throw_Exception_On_Error()
    {
        // Arrange
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var parameters = new { Name = _faker.Name.FullName() };

        // Act & Assert
        _dapperWrapper.Invoking(x =>
                x.QueryStoredProcedureAsync<TestUser>(connection, "sp_NonExistentProcedure", parameters))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Error executing stored procedure: sp_NonExistentProcedure");
    }

    #region Helper Methods

    private async Task InsertUser(string name, SqlConnection connection)
    {
        var sql = "INSERT INTO USERS (NAME) VALUES(@name)";
        await connection.ExecuteAsync(sql, new { name });
    }

    private class TestUser
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }

    #endregion
}