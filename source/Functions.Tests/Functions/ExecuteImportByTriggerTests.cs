using Core.DTOs.Response.Queue;
using FluentAssertions;
using Functions.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Functions.Tests;

public class ExecuteImportByTriggerTests
{
    private readonly Mock<IImportService> _importServiceMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly ExecuteImportByTrigger _function;

    public ExecuteImportByTriggerTests()
    {
        _importServiceMock = new Mock<IImportService>();
        _loggerMock = new Mock<ILogger>();
        _function = new ExecuteImportByTrigger(_importServiceMock.Object);
    }

    [Fact]
    public async Task Run_Should_Call_InstallData_With_Correct_QueueItem()
    {
        // Arrange
        var queueItem = new Message
        {
            Override = true,
            BlobName = "fakeBlob",
            FileTypeId = 123
        };

        // Act
        await _function.Run(queueItem, _loggerMock.Object);

        // Assert
        _importServiceMock.Verify(s => s.InstallData(queueItem), Times.Once);
    }

    [Fact]
    public async Task Run_Should_Not_Throw_Exception()
    {
        // Arrange
        var queueItem = new Message
        {
            Override = true,
            BlobName = "fakeBlob",
            FileTypeId = 123
        };

        // Act
        var act = async () => await _function.Run(queueItem, _loggerMock.Object);

        // Assert
        await act.Should().NotThrowAsync();
    }
}