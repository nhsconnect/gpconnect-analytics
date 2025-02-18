using System.Net;
using Core.Helpers;
using FluentAssertions;
using Functions.Services.Interfaces;
using Functions.Tests.TestHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;

namespace Functions.Tests;

public class GetDataFromApiTodayTests
{
    private readonly GetDataFromApiToday _function;
    private readonly FakeLogger _loggerMock;
    private readonly Mock<IBatchService> _batchService;

    public GetDataFromApiTodayTests()
    {
        _batchService = new Mock<IBatchService>();
        _loggerMock = new FakeLogger();
        _function = new GetDataFromApiToday(_batchService.Object, _loggerMock);
    }

    [Fact]
    public async Task SspTrans_ShouldReturnSuccessful()
    {
        // Arrange
        var request = MockRequests.CreateTodayMockRequest();

        _batchService
            .Setup(s => s.StartBatchDownloadForTodayAsync(FileTypes.ssptrans))
            .ReturnsAsync(2);


        // Act
        var result = await _function.GetDataFromSspTransByDateRange(request);

        // Assert
        result.Body.Position = 0; // reset the position to the beginning of the stream
        result.StatusCode.Should().Be(HttpStatusCode.OK);

        using var reader = new StreamReader(result.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Contain("Processed 2 requests");
    }

    [Fact]
    public async Task SspTrans_ShouldReturn500_WhenErrorThrown()
    {
        // Arrange
        var request = MockRequests.CreateTodayMockRequest();

        _batchService
            .Setup(s => s.StartBatchDownloadForTodayAsync(FileTypes.ssptrans))
            .ThrowsAsync(new Exception("Error while downloading"));


        // Act
        var result = await _function.GetDataFromSspTransByDateRange(request);

        // Assert
        result.Body.Position = 0; // reset the position to the beginning of the stream
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        using var reader = new StreamReader(result.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Be("An error occurred whilst processing batch download - see logs for more details");
    }

    [Fact]
    public async Task SspTrans_ShouldLogError_WhenErrorThrown()
    {
        // Arrange
        var request = MockRequests.CreateTodayMockRequest();

        _batchService
            .Setup(s => s.StartBatchDownloadForTodayAsync(FileTypes.ssptrans))
            .ThrowsAsync(new Exception("Error while downloading"));


        // Act
        var result = await _function.GetDataFromSspTransByDateRange(request);

        // Assert
        _loggerMock.Collector.LatestRecord.Message.Should()
            .Be("An error occurred during batch download when processing urls");

        _loggerMock.Collector.LatestRecord.Exception.Should().BeOfType<Exception>();
        _loggerMock.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
        _loggerMock.Collector.LatestRecord.Exception?.Message.Should()
            .Be("Error while downloading");
    }

    [Fact]
    public async Task AsidLookup_ShouldReturnSuccessful()
    {
        // Arrange
        var request = MockRequests.CreateTodayMockRequest();

        _batchService
            .Setup(s => s.StartBatchDownloadForTodayAsync(FileTypes.asidlookup))
            .ReturnsAsync(2);


        // Act
        var result = await _function.GetDataFromAsidLookupByDateRange(request);

        // Assert
        result.Body.Position = 0; // reset the position to the beginning of the stream
        result.StatusCode.Should().Be(HttpStatusCode.OK);

        using var reader = new StreamReader(result.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Contain("Processed 2 requests");
    }

    [Fact]
    public async Task AsidLookup_ShouldReturn500_WhenErrorThrown()
    {
        // Arrange
        var request = MockRequests.CreateTodayMockRequest();

        _batchService
            .Setup(s => s.StartBatchDownloadForTodayAsync(FileTypes.asidlookup))
            .ThrowsAsync(new Exception("Error while downloading"));


        // Act
        var result = await _function.GetDataFromAsidLookupByDateRange(request);

        // Assert
        result.Body.Position = 0; // reset the position to the beginning of the stream
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        using var reader = new StreamReader(result.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Be("An error occurred whilst processing batch download - see logs for more details");
    }

    [Fact]
    public async Task AsidLookup_ShouldLogError_WhenErrorThrown()
    {
        // Arrange
        var request = MockRequests.CreateTodayMockRequest();

        _batchService
            .Setup(s => s.StartBatchDownloadForTodayAsync(FileTypes.asidlookup))
            .ThrowsAsync(new Exception("Error while downloading"));


        // Act
        var result = await _function.GetDataFromAsidLookupByDateRange(request);

        // Assert
        _loggerMock.Collector.LatestRecord.Message.Should()
            .Be("An error occurred during batch download when processing urls");

        _loggerMock.Collector.LatestRecord.Exception.Should().BeOfType<Exception>();
        _loggerMock.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
        _loggerMock.Collector.LatestRecord.Exception?.Message.Should()
            .Be("Error while downloading");
    }

    [Fact]
    public async Task MeshTrans_ShouldReturnSuccessful()
    {
        // Arrange
        var request = MockRequests.CreateTodayMockRequest();

        _batchService
            .Setup(s => s.StartBatchDownloadForTodayAsync(FileTypes.meshtrans))
            .ReturnsAsync(2);


        // Act
        var result = await _function.GetDataFromMeshTransByDateRange(request);

        // Assert
        result.Body.Position = 0; // reset the position to the beginning of the stream
        result.StatusCode.Should().Be(HttpStatusCode.OK);

        using var reader = new StreamReader(result.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Contain("Processed 2 requests");
    }

    [Fact]
    public async Task MeshTrans_ShouldReturn500_WhenErrorThrown()
    {
        // Arrange
        var request = MockRequests.CreateTodayMockRequest();

        _batchService
            .Setup(s => s.StartBatchDownloadForTodayAsync(FileTypes.meshtrans))
            .ThrowsAsync(new Exception("Error while downloading"));


        // Act
        var result = await _function.GetDataFromMeshTransByDateRange(request);

        // Assert
        result.Body.Position = 0; // reset the position to the beginning of the stream
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        using var reader = new StreamReader(result.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Be("An error occurred whilst processing batch download - see logs for more details");
    }

    [Fact]
    public async Task MeshTrans_ShouldLogError_WhenErrorThrown()
    {
        // Arrange
        var request = MockRequests.CreateTodayMockRequest();

        _batchService
            .Setup(s => s.StartBatchDownloadForTodayAsync(FileTypes.meshtrans))
            .ThrowsAsync(new Exception("Error while downloading"));


        // Act
        var result = await _function.GetDataFromMeshTransByDateRange(request);

        // Assert
        _loggerMock.Collector.LatestRecord.Message.Should()
            .Be("An error occurred during batch download when processing urls");

        _loggerMock.Collector.LatestRecord.Exception.Should().BeOfType<Exception>();
        _loggerMock.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
        _loggerMock.Collector.LatestRecord.Exception?.Message.Should()
            .Be("Error while downloading");
    }
}