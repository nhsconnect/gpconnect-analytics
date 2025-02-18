using System.Net;
using Core.Helpers;
using FluentAssertions;
using Functions.Services.Interfaces;
using Functions.Tests.TestHelpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace Functions.Tests;

public class GetDataFromApiByDateRangeTests
{
    private readonly Mock<IBatchService> _batchServiceMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly GetDataFromApiByDateRange _function;

    public GetDataFromApiByDateRangeTests()
    {
        _batchServiceMock = new Mock<IBatchService>();
        _loggerMock = new Mock<ILogger>();
        _function = new GetDataFromApiByDateRange(_batchServiceMock.Object, _loggerMock.Object);
    }


    [Fact]
    public async Task GetDataFromSspTransByDateRange_Should_Return_Successful_Response()
    {
        // Arrange
        var request = MockRequests.CreateDateRangeMockRequest("2024-01-01", "2024-01-31");

        _batchServiceMock
            .Setup(s => s.StartBatchDownloadAsync(FileTypes.ssptrans, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(10); // Simulate 10 rows processed


        // Act
        var result = await _function.GetDataFromSspTransByDateRange(request);

        // Assert
        result.Body.Position = 0; // reset the position to the beginning of the stream
        result.StatusCode.Should().Be(HttpStatusCode.OK);

        using var reader = new StreamReader(result.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Contain("Batch Download successful: 10 requests processed");
    }

    [Fact]
    public async Task GetDataFromSSpTransByDateRange_Should_LogError_AndReturn_InternalServerError_OnError()
    {
        // Arrange
        var request = MockRequests.CreateDateRangeMockRequest("2024-01-01", "2024-01-31");

        _batchServiceMock
            .Setup(s => s.StartBatchDownloadAsync(FileTypes.ssptrans, It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Error processing batch")); // Simulate an error


        // Act
        var result = await _function.GetDataFromSspTransByDateRange(request);

        // Assert
        result.Body.Position = 0; // reset the position to the beginning of the stream
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        using var reader = new StreamReader(result.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Contain("Failed to download - see logs");
    }

    [Fact]
    public async Task GetDataFromApiDateRange_MeshTrans_Should_Return_Successful_Response()

    {
        // Arrange
        var request = MockRequests.CreateDateRangeMockRequest("2024-01-01", "2024-01-31");

        _batchServiceMock
            .Setup(s => s.StartBatchDownloadAsync(FileTypes.meshtrans, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(10); // Simulate 10 rows processed


        // Act
        var result = await _function.GetDataFromMeshTransByDateRange(request);

        // Assert
        result.Body.Position = 0; // reset the position to the beginning of the stream
        result.StatusCode.Should().Be(HttpStatusCode.OK);

        using var reader = new StreamReader(result.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Contain("Batch Download successful: 10 requests processed");
    }

    [Fact]
    public async Task GetDataFromMeshTransByDateRange_Should_LogError_AndReturn_InternalServerError_OnError()
    {
        // Arrange
        var request = MockRequests.CreateDateRangeMockRequest("2024-01-01", "2024-01-31");

        _batchServiceMock
            .Setup(s => s.StartBatchDownloadAsync(FileTypes.meshtrans, It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Error processing batch")); // Simulate an error


        // Act
        var result = await _function.GetDataFromMeshTransByDateRange(request);

        // Assert
        result.Body.Position = 0; // reset the position to the beginning of the stream
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        using var reader = new StreamReader(result.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Contain("Failed to download - see logs");
    }

    [Fact]
    public async Task GetDataFromApiByDateRange_AsidLookup_Should_Return_Successful_Response()

    {
        // Arrange
        var request = MockRequests.CreateDateRangeMockRequest("2024-01-01", "2024-01-31");

        _batchServiceMock
            .Setup(s => s.StartBatchDownloadAsync(FileTypes.asidlookup, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(10); // Simulate 10 rows processed


        // Act
        var result = await _function.GetDataFromAsidLookupByDateRange(request);

        // Assert
        result.Body.Position = 0; // reset the position to the beginning of the stream
        result.StatusCode.Should().Be(HttpStatusCode.OK);

        using var reader = new StreamReader(result.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Contain("Batch Download successful: 10 requests processed");
    }

    [Fact]
    public async Task GetDataFromAsidLookupByDateRange_Should_LogError_AndReturn_InternalServerError_OnError()
    {
        // Arrange
        var request = MockRequests.CreateDateRangeMockRequest("2024-01-01", "2024-01-31");

        _batchServiceMock
            .Setup(s => s.StartBatchDownloadAsync(FileTypes.asidlookup, It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Error processing batch")); // Simulate an error


        // Act
        var result = await _function.GetDataFromAsidLookupByDateRange(request);

        // Assert
        result.Body.Position = 0; // reset the position to the beginning of the stream
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        using var reader = new StreamReader(result.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Contain("Failed to download - see logs");
    }
}