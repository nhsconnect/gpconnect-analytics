using System.Data;
using System.Data.Common;
using Core;
using Core.Repositories;
using Core.Services.Interfaces;
using Dapper;
using FakeItEasy;
using FluentAssertions;
using function_app.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Xunit;

namespace gpconnect_analytics.Test.Services
{
    public class DataServiceTests
    {
        private readonly ICoreConfigurationService _fakeConfigService;
        private readonly IDapperWrapper _fakeDapper;
        private readonly DataService _dataService;

        private readonly string _testConnectionString = "Server=localhost;Database=TestDb;User Id=test;Password=test;";
        private readonly IConnectionFactory _fakeConnectionFactory;
        private readonly FakeLogger<DataService> _fakeLogger;

        public DataServiceTests()
        {
            _fakeLogger = new FakeLogger<DataService>();
            _fakeConfigService = A.Fake<ICoreConfigurationService>();
            _fakeDapper = A.Fake<IDapperWrapper>();
            _fakeConnectionFactory = A.Fake<IConnectionFactory>();
            A.CallTo(() => _fakeDapper.ExecuteAsync(
                    A<SqlConnection>.Ignored,
                    A<string>.Ignored,
                    A<object>.Ignored,
                    A<IDbTransaction>.Ignored))
                .Returns(Task.FromResult(1));


            A.CallTo(() => _fakeConfigService.GetConnectionString(A<string>.Ignored))
                .Returns(_testConnectionString);

            var fakeSqlConnection = A.Fake<DbConnection>();
            A.CallTo(() => _fakeConnectionFactory.CreateConnection(_testConnectionString))
                .Returns(fakeSqlConnection);

            _dataService = new DataService(_fakeLogger, _fakeConfigService, _fakeDapper, _fakeConnectionFactory);
        }

        [Fact]
        public async Task ExecuteRawUpsertSqlAsync_ShouldReturnRowsAffected()
        {
            // Arrange
            const string sqlCommand = "INSERT INTO TestTable (Id, Name) VALUES (@Id, @Name);";
            var parameters = new { Id = 1, Name = "Test" };

            A.CallTo(() => _fakeDapper.ExecuteAsync(
                    A<DbConnection>.Ignored, sqlCommand, parameters, A<IDbTransaction>.Ignored))
                .Returns(1);

            // Act
            var result = await _dataService.ExecuteRawUpsertSqlAsync(sqlCommand, parameters);

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public async Task ExecuteRawUpsertSqlAsync_ShouldLogAndThrowOnError()
        {
            // Arrange
            const string sqlCommand = "INVALID SQL COMMAND";
            var parameters = new { Id = 1, Name = "Test" };

            A.CallTo(() => _fakeDapper.ExecuteAsync(
                    A<DbConnection>.Ignored, sqlCommand, parameters, A<IDbTransaction>.Ignored))
                .Throws(new Exception("Simulated SQL error"));

            // Act
            var act = async () => await _dataService.ExecuteRawUpsertSqlAsync(sqlCommand, parameters);

            // Assert
            await act.Should().ThrowAsync<Exception>();

            var r = _fakeLogger.LatestRecord;

            // should be logged as an error
            Assert.Equal(LogLevel.Error, r.Level);

            // the message should include the text “Could not do something”
            Assert.Contains($"An error has occurred while executing the raw SQL command: {sqlCommand}", r.Message);
        }

        [Fact]
        public async Task ExecuteStoredProcedure_ShouldReturnResults()
        {
            // Arrange
            const string procedureName = "TestProcedure";
            var parameters = new DynamicParameters();
            var expectedResult = new List<string> { "Result1", "Result2" };

            A.CallTo(() => _fakeDapper.QueryStoredProcedureAsync<string>(
                    A<DbConnection>.Ignored, procedureName, parameters, 0))
                .Returns(expectedResult);

            // Act
            var result = await _dataService.ExecuteQueryStoredProcedure<string>(procedureName, parameters);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task ExecuteStoredProcedure_ShouldLogAndThrowOnError()
        {
            // Arrange
            const string procedureName = "TestProcedure";
            var parameters = new DynamicParameters();

            A.CallTo(() => _fakeDapper.QueryStoredProcedureAsync<string>(
                    A<DbConnection>.Ignored, procedureName, parameters, 0))
                .Throws(new Exception("Simulated SQL error"));

            // Act
            var act = async () => await _dataService.ExecuteQueryStoredProcedure<string>(procedureName, parameters);

            // Assert
            await act.Should().ThrowAsync<Exception>();
            var r = _fakeLogger.LatestRecord;

            // should be logged as an error
            Assert.Equal(LogLevel.Error, r.Level);

            // the message should include the text “Could not do something”
            Assert.Contains($"An error has occurred while attempting to execute the function {procedureName}",
                r.Message);
        }

        [Fact]
        public async Task ExecuteStoredProcedureWithOutputParameters_ShouldReturnParameters()
        {
            // Arrange
            const string procedureName = "TestProcedure";
            var parameters = new DynamicParameters();
            parameters.Add("OutputParam", dbType: DbType.String, direction: ParameterDirection.Output);

            A.CallTo(() => _fakeDapper.ExecuteAsync(
                    A<SqlConnection>.Ignored,
                    A<string>.Ignored,
                    A<object>.Ignored,
                    A<IDbTransaction>.Ignored))
                .Returns(Task.FromResult(1));

            // Act
            var result = await _dataService.ExecuteStoredProcedureWithOutputParameters(procedureName, parameters);

            // Assert
            result.Should().BeSameAs(parameters);
        }

        [Fact]
        public async Task ExecuteStoredProcedureWithOutputParameters_ShouldLogAndThrowOnError()
        {
            // Arrange
            const string procedureName = "TestProcedure";
            var parameters = new DynamicParameters();

            A.CallTo(() => _fakeDapper.ExecuteStoredProcedureAsync<DynamicParameters>(
                    A<DbConnection>.Ignored, procedureName, parameters, 0))
                .Throws(new Exception("Simulated SQL error"));

            // Act
            var act = async () =>
                await _dataService.ExecuteStoredProcedureWithOutputParameters(procedureName, parameters);

            // Assert
            await act.Should().ThrowAsync<Exception>();

            var r = _fakeLogger.LatestRecord;

            // should be logged as an error
            Assert.Equal(LogLevel.Error, r.Level);

            // the message should include the text “Could not do something”
            Assert.Contains($"An error has occurred while attempting to execute the function {procedureName}",
                r.Message);
        }
    }
}