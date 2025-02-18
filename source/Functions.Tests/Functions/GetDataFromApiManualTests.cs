using System.Net;
using FluentAssertions;
using Functions.Services.Interfaces;
using Functions.Tests.TestHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;

namespace Functions.Tests;

public class GetDataFromApiManualTests
{
    private readonly Mock<IImportService> _importService;
    private readonly FakeLogger _loggerMock;
    private readonly GetDataFromApiManual _function;


    public GetDataFromApiManualTests()
    {
        _importService = new Mock<IImportService>();
        _loggerMock = new FakeLogger();
        _function = new GetDataFromApiManual(_importService.Object, _loggerMock);
    }

    [Fact]
    public async Task GetDataFromApi_ShouldReturnSuccessfulResponse()
    {
        // Arrange
        var request = MockRequests.MockAddDownloadRequest();
        _importService.Setup(x => x.AddDownloadedFileManually(It.IsAny<string>()))
            .Returns(Task.CompletedTask);


        // Act
        var response = await _function.AddDownloadedFile(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Body.Position = 0; // reset the position to the beginning of the stream

        using var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Contain("Successfully added files");

        // Assert
    }

    [Fact]
    public async Task GetDataFromApi_ReturnBadRequest_WhenMissingFilePathQueryParam()
    {
        // Arrange
        var request = MockRequests.MockRequestNoQuery();

        // Act
        var response = await _function.AddDownloadedFile(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Body.Position = 0; // reset the position to the beginning of the stream

        // Assert
        using var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Contain("Filepath missing");
    }

    [Fact]
    public async Task GetDataFromApi_ReturnInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        var request = MockRequests.MockAddDownloadRequest();
        _importService.Setup(x => x.AddDownloadedFileManually(It.IsAny<string>()))
            .Throws(new Exception("An error occurred"));

        // Act
        var response = await _function.AddDownloadedFile(request);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Body.Position = 0; // reset the position to the beginning of the stream

        // Assert
        using var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Contain("Something went wrong - see error logs for more details");
    }

    [Fact]
    public async Task GetDataFromApi_LogsError_WhenExceptionThrown()
    {
        // Arrange
        var request = MockRequests.MockAddDownloadRequest();
        _importService.Setup(x => x.AddDownloadedFileManually(It.IsAny<string>()))
            .Throws(new Exception("An error occurred"));

        // Act
        var response = await _function.AddDownloadedFile(request);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Body.Position = 0; // reset the position to the beginning of the stream

        // Assert
        _loggerMock.Collector.LatestRecord.Message.Should().Be("Error adding downloaded file: An error occurred");
        _loggerMock.Collector.LatestRecord.Exception.Should().BeOfType<Exception>();
        _loggerMock.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }
}