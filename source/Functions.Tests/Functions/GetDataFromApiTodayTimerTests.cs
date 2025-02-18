using Core.Helpers;
using Functions.Services.Interfaces;
using Functions.Tests.TestHelpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace Functions.Tests;

public class GetDataFromApiByTriggerTests
{
    private readonly Mock<IBatchService> _batchServiceMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly GetDataFromApiByTrigger _function;

    public GetDataFromApiByTriggerTests()
    {
        _batchServiceMock = new Mock<IBatchService>();
        _loggerMock = new Mock<ILogger>();
        _function = new GetDataFromApiByTrigger(_batchServiceMock.Object, _loggerMock.Object);
    }


    [Fact]
    public async Task GetDataFromAsidLookup_Should_Invoke_BatchService()
    {
        // Arrange
        var timerInfo = MockTriggers.CreateMockTimerInfo();

        // Act
        await _function.GetDataFromAsidLookup(timerInfo);

        // Assert
        _batchServiceMock.Verify(
            x => x.StartBatchDownloadForTodayAsync(FileTypes.asidlookup),
            Times.Once);
    }

    [Fact]
    public async Task GetDataFromSspTrans_Should_Invoke_BatchService()
    {
        // Arrange
        var timerInfo = MockTriggers.CreateMockTimerInfo();

        // Act
        await _function.GetDataFromSspTrans(timerInfo);

        // Assert
        _batchServiceMock.Verify(
            x => x.StartBatchDownloadForTodayAsync(FileTypes.ssptrans),
            Times.Once);
    }

    [Fact]
    public async Task GetDataFromMeshTrans_Should_Invoke_BatchService()
    {
        // Arrange
        var timerInfo = MockTriggers.CreateMockTimerInfo();

        // Act
        await _function.GetDataFromMeshTrans(timerInfo);

        // Assert
        _batchServiceMock.Verify(
            x => x.StartBatchDownloadForTodayAsync(FileTypes.meshtrans),
            Times.Once);
    }
}