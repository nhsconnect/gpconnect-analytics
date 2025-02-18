using Core.Services.Interfaces;
using Dapper;
using Functions.Services;
using Moq;

namespace Functions.Tests;

public class LoggingServiceTests
{
    private readonly Mock<IDataService> _mockDataService;
    private readonly LoggingService _loggingService;

    public LoggingServiceTests()
    {
        _mockDataService = new Mock<IDataService>();
        _loggingService = new LoggingService(_mockDataService.Object);
    }

    [Fact]
    public async Task PurgeErrorLog_ShouldCallDataService()
    {
        // Arrange
        _mockDataService.Setup(m => m.ExecuteStoredProcedure("Logging.PurgeErrorLog", It.IsAny<DynamicParameters>()))
            .ReturnsAsync(1);

        // Act
        await _loggingService.PurgeErrorLog();

        // Assert
        _mockDataService.Verify(m => m.ExecuteStoredProcedure("Logging.PurgeErrorLog", It.IsAny<DynamicParameters>()),
            Times.Once);
    }
}