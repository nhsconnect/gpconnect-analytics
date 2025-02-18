using System.Net;
using System.Text;
using System.Text.Json;
using Core.DTOs.Request;
using Core.Repositories;
using FluentAssertions;
using Functions.Tests.TestHelpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;

namespace Functions.Tests;

public class StoreProviderConsumerDataTests
{
    private readonly Mock<IHierarchyProviderConsumerRepo> _repositoryMock;
    private readonly FakeLogger<StoreProviderConsumerData> _loggerMock;
    private readonly StoreProviderConsumerData _function;

    public StoreProviderConsumerDataTests()
    {
        _repositoryMock = new Mock<IHierarchyProviderConsumerRepo>();
        _loggerMock = new FakeLogger<StoreProviderConsumerData>();
        _function = new StoreProviderConsumerData(_repositoryMock.Object, _loggerMock);
    }


    [Fact]
    public async Task Run_ShouldReturnBadRequest_WhenRequestBodyIsEmpty()
    {
        // Arrange
        var request = MockRequests.MockRequestNoQuery();

        // Act
        var response = await _function.Run(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Run_ShouldReturnBadRequest_WhenInvalidJsonIsProvided()
    {
        // Arrange
        var request = MockRequests.MockRequestNoQuery();

        // Act
        var response = await _function.Run(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Run_ShouldReturnOk_WhenValidInputIsProvided()
    {
        // Arrange
        var records = new List<OrganisationHierarchyProvider>
        {
            new()
            {
                OdsCode = null,
                PracticeName = null,
                RegisteredPatientCount = 0,
                RegionCode = null,
                RegionName = null,
                Icb22Name = null,
                PcnName = null,
                Appointments13000 = 0
            }
        };
        var json = JsonSerializer.Serialize(records);
        var request = MockRequests.MockRequestWithBody(json);

        _repositoryMock.Setup(r => r.InsertHierarchyProviderConsumers(It.IsAny<List<OrganisationHierarchyProvider>>()))
            .ReturnsAsync(1);

        // Act
        var response = await _function.Run(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Run_ShouldReturnBadRequest_WhenRepositoryFails()
    {
        // Arrange
        var records = new List<OrganisationHierarchyProvider>
        {
            new()
            {
                OdsCode = null,
                PracticeName = null,
                RegisteredPatientCount = 0,
                RegionCode = null,
                RegionName = null,
                Icb22Name = null,
                PcnName = null,
                Appointments13000 = 0
            }
        };
        var json = JsonSerializer.Serialize(records);
        var request = MockRequests.MockRequestWithBody(json);

        _repositoryMock.Setup(r => r.InsertHierarchyProviderConsumers(It.IsAny<List<OrganisationHierarchyProvider>>()))
            .ReturnsAsync(0);

        // Act
        var response = await _function.Run(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Headers.GetValues("Content-Type").Should().Contain("text/plain; charset=utf-8");
        _loggerMock.Collector.LatestRecord.Message.Should().Be("No items of 1 were saved to the database");

        response.Body.Position = 0;
        using var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Be("Failed to save to the database - see logs for more information");
    }

    [Fact]
    public async Task Test_InvalidJson_ReturnsBadRequest_WithCorrectMessage()
    {
        // Arrange: 
        var invalidJsonBody = "{ message: 'Invalid JSON' "; // Invalid JSON (missing closing brace)

        var mockHttpRequest = MockRequests.MockRequestWithBody(invalidJsonBody);

        // Act
        var result = await _function.Run(mockHttpRequest);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        result.Body.Position = 0;
        using var reader = new StreamReader(result.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Contain("Invalid json input");

        _loggerMock.Collector.LatestRecord.Message.Should().Contain("Failed to deserialize request body:");
    }
}