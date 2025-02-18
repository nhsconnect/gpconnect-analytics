using System.Data;
using System.Data.Common;
using Core.Repositories;
using Core.Services.Interfaces;
using Dapper;
using FluentAssertions;
using Functions.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Xunit;

namespace Core.Tests.Services
{
    public class DataServiceTests
    {
        private readonly Mock<ICoreConfigurationService> _mockConfigService;
        private readonly Mock<IDapperWrapper> _mockDapper;
        private readonly DataService _dataService;
        private readonly string _testConnectionString = "Server=localhost;Database=TestDb;User Id=test;Password=test;";
        private readonly Mock<IConnectionFactory> _mockConnectionFactory;
        private readonly FakeLogger<DataService> _fakeLogger;
        private readonly Mock<DbConnection> _mockConnection;

        public DataServiceTests()
        {
            _fakeLogger = new FakeLogger<DataService>();
            _mockConfigService = new Mock<ICoreConfigurationService>();
            _mockDapper = new Mock<IDapperWrapper>();
            _mockConnectionFactory = new Mock<IConnectionFactory>();

            _mockDapper.Setup(d => d.ExecuteAsync(
                    It.IsAny<SqlConnection>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync(1);

            _mockConfigService.Setup(c => c.GetConnectionString(It.IsAny<string>()))
                .Returns(_testConnectionString);

            _mockConnection = new Mock<DbConnection>();
            _mockConnectionFactory.Setup(cf => cf.CreateConnection(_testConnectionString))
                .Returns(_mockConnection.Object);

            _dataService = new DataService(_fakeLogger, _mockConfigService.Object, _mockDapper.Object,
                _mockConnectionFactory.Object);
        }

        [Fact]
        public async Task ExecuteRawUpsertSqlAsync_ShouldReturnRowsAffected()
        {
            // Arrange
            const string sqlCommand = "INSERT INTO TestTable (Id, Name) VALUES (@Id, @Name);";
            var parameters = new { Id = 1, Name = "Test" };

            _mockDapper.Setup(d => d.ExecuteAsync(
                    It.IsAny<DbConnection>(), sqlCommand, parameters, It.IsAny<IDbTransaction>()))
                .ReturnsAsync(1);

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

            _mockDapper.Setup(d => d.ExecuteAsync(
                    It.IsAny<DbConnection>(), sqlCommand, parameters, It.IsAny<IDbTransaction>()))
                .ThrowsAsync(new Exception("Simulated SQL error"));

            // Act
            var act = async () => await _dataService.ExecuteRawUpsertSqlAsync(sqlCommand, parameters);

            // Assert
            await act.Should().ThrowAsync<Exception>();
            var r = _fakeLogger.LatestRecord;
            Assert.Equal(LogLevel.Error, r.Level);
            Assert.Contains($"An error has occurred while executing the raw SQL command: {sqlCommand}", r.Message);
        }

        [Fact]
        public async Task ExecuteStoreProcedure_ShouldReturn_Integer()
        {
            // Arrange
            const string procedureName = "TestProcedure";
            var parameters = new DynamicParameters();
            var expectedResult = 1;

            _mockDapper.Setup(d => d.ExecuteAsync(
                    It.IsAny<DbConnection>(), procedureName, parameters, null))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _dataService.ExecuteStoredProcedure(procedureName, parameters);

            // Assert
            result.Should().Be(expectedResult);
        }

        [Fact]
        public async Task ExecuteStoreProcedure_Should_LogErrorAndThrow()
        {
            // Arrange
            const string procedureName = "TestProcedure";
            var parameters = new DynamicParameters();
            var expectedResult = 1;

            _mockDapper.Setup(d => d.ExecuteAsync(
                    It.IsAny<DbConnection>(), procedureName, parameters, null))
                .ThrowsAsync(new Exception("Database went wrong"));

            // Act
            await _dataService.Invoking(x => x.ExecuteStoredProcedure(procedureName, parameters))
                .Should().ThrowAsync<Exception>();

            var log = _fakeLogger.LatestRecord;
            log.Message.Should().Be($"An error has occurred while attempting to execute the function {procedureName}");
        }

        [Fact]
        public async Task ExecuteQueryStoredProcedure_ShouldReturnResults()
        {
            // Arrange
            const string procedureName = "TestProcedure";
            var parameters = new DynamicParameters();
            var expectedResult = new List<string> { "Result1", "Result2" };

            _mockDapper.Setup(d => d.QueryStoredProcedureAsync<string>(
                    It.IsAny<DbConnection>(), procedureName, parameters, 0))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _dataService.ExecuteQueryStoredProcedure<string>(procedureName, parameters);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task ExecuteQueryStoredProcedure_ShouldLogAndThrowOnError()
        {
            // Arrange
            const string procedureName = "TestProcedure";
            var parameters = new DynamicParameters();

            _mockDapper.Setup(d => d.QueryStoredProcedureAsync<string>(
                    It.IsAny<DbConnection>(), procedureName, parameters, 0))
                .ThrowsAsync(new Exception("Simulated SQL error"));

            // Act
            var act = async () => await _dataService.ExecuteQueryStoredProcedure<string>(procedureName, parameters);

            // Assert
            await act.Should().ThrowAsync<Exception>();
            var r = _fakeLogger.LatestRecord;
            Assert.Equal(LogLevel.Error, r.Level);
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

            _mockDapper.Setup(d => d.ExecuteAsync(
                    It.IsAny<SqlConnection>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync(1);

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

            _mockDapper.Setup(d => d.ExecuteStoredProcedureAsync<DynamicParameters>(
                    It.IsAny<DbConnection>(), procedureName, parameters, 0))
                .ThrowsAsync(new Exception("Simulated SQL error"));

            // Act
            var act = async () =>
                await _dataService.ExecuteStoredProcedureWithOutputParameters(procedureName, parameters);

            // Assert
            await act.Should().ThrowAsync<Exception>();
            var r = _fakeLogger.LatestRecord;
            Assert.Equal(LogLevel.Error, r.Level);
            Assert.Contains($"An error has occurred while attempting to execute the function {procedureName}",
                r.Message);
        }
    }
}