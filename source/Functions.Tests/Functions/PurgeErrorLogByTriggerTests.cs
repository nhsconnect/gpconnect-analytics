using Functions.Services.Interfaces;
using Functions.Tests.TestHelpers;
using Microsoft.Azure.Functions.Worker;
using Moq;

namespace Functions.Tests;

public class PurgeErrorLogByTriggerTests
{
    private readonly TimerInfo _timerInfo;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private PurgeErrorLogByTrigger _function;

    public PurgeErrorLogByTriggerTests()
    {
        _loggingServiceMock = new Mock<ILoggingService>();
        _timerInfo = MockTriggers.CreateMockTimerInfo();


        _function = new PurgeErrorLogByTrigger(_loggingServiceMock.Object);
    }

    [Fact]
    public async Task PurgeErrorLogByTrigger_Should_Invoke_LoggingServiceCorrectly()
    {
        // Arrange
        // Act
        await _function.PurgeErrorLog(_timerInfo);

        // Assert
        _loggingServiceMock.Verify(x => x.PurgeErrorLog(), Times.Once);
    }
}