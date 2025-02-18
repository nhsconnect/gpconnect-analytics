using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Bogus;
using Core;
using Core.DTOs.Request;
using Core.DTOs.Response.Configuration;
using Core.DTOs.Response.Splunk;
using Core.Helpers;
using Core.Services.Interfaces;
using FluentAssertions;
using Functions.Services;
using Functions.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace Functions.Tests.Services;

public class SplunkServiceTests
{
    private readonly Mock<IConfigurationService> _mockConfigService;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<IImportService> _mockImportService;
    private readonly FakeLogger<SplunkService> _fakeLogger;
    private readonly SplunkService _splunkService;
    private readonly Mock<ITimeProvider> _mockTimeProvider;

    public SplunkServiceTests()
    {
        _mockTimeProvider = new Mock<ITimeProvider>();
        _mockConfigService = new Mock<IConfigurationService>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockImportService = new Mock<IImportService>();
        _fakeLogger = new FakeLogger<SplunkService>();

        _mockConfigService.Setup(x => x.GetFilePathConstants())
            .ReturnsAsync(new FilePathConstants
            {
                PathSeparator = "/", ProjectNameFilePrefix = "proj_", ComponentSeparator = "_", FileExtension = ".csv"
            });

        var splunkInstance = new SplunkInstance { Source = "splunk-source" };
        _mockConfigService.Setup(x => x.GetSplunkInstance(It.IsAny<SplunkInstances>())).ReturnsAsync(splunkInstance);

        _splunkService = new SplunkService(
            _mockConfigService.Object,
            _mockHttpClientFactory.Object,
            _mockImportService.Object,
            _fakeLogger,
            _mockTimeProvider.Object
        );
    }

    private static FileType GenerateFileType() => new Faker<FileType>()
        .RuleFor(f => f.FileTypeFilePrefix, f => f.Random.Word())
        .RuleFor(f => f.DirectoryName, f => f.System.DirectoryPath())
        .RuleFor(f => f.Enabled, true)
        .Generate();

    private static UriRequest GenerateUriRequest(DateTime? fixedFromDate = null, DateTime? fixedToDate = null,
        TimeSpan? hour = null, bool isFake = true)
    {
        if (!isFake)
        {
            return new UriRequest
            {
                Request = new Uri("https://splunk.com/api/fixed-test"),
                EarliestDate = fixedFromDate ?? DateTime.Now.AddDays(-1),
                LatestDate = fixedToDate ?? DateTime.Now,
                Hour = hour ?? new TimeSpan(0, 0, 0)
            };
        }

        var faker = new Faker();
        return new UriRequest()
        {
            Request = new Uri("https://splunk.com/api/fake-test"),
            EarliestDate = faker.Date.Past(),
            LatestDate = faker.Date.Recent(1),
            Hour = faker.Date.Timespan()
        };
    }


    [Fact]
    public async Task DownloadCSVDateRangeAsync_ShouldReturnExtractResponse_WhenValidInput()
    {
        var fileType = GenerateFileType();
        var uriRequest = GenerateUriRequest();

        var httpClient = new HttpClient(new Mock<HttpMessageHandler>().Object);
        _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var result = await _splunkService.DownloadCSVDateRangeAsync(fileType, uriRequest, true);

        result.Should().NotBeNull();
        result.FilePath.Should().Contain(fileType.FileTypeFilePrefix);
    }

    [Fact]
    public async Task DownloadCSVDateRangeAsync_ShouldThrowTimeoutException()
    {
        var fileType = GenerateFileType();
        var uriRequest = GenerateUriRequest();
        _mockConfigService.Setup(x => x.GetFilePathConstants()).ThrowsAsync(new TimeoutException());

        Func<Task> act = async () => await _splunkService.DownloadCSVDateRangeAsync(fileType, uriRequest, true);

        await act.Should().ThrowAsync<TimeoutException>();
        _fakeLogger.Collector.LatestRecord.Message.Should().Be("A timeout error has occurred");
    }

    [Fact]
    public async Task DownloadCSVDateRangeAsync_ShouldThrowException()
    {
        var fileType = GenerateFileType();
        var uriRequest = GenerateUriRequest();
        _mockConfigService.Setup(x => x.GetFilePathConstants()).ThrowsAsync(new Exception());

        Func<Task> act = async () => await _splunkService.DownloadCSVDateRangeAsync(fileType, uriRequest, true);
        await act.Should().ThrowAsync<Exception>();

        _fakeLogger.Collector.LatestRecord.Message.Should().Be("An error occurred in trying to execute a GET request");
    }

    [Fact]
    public async Task ExecuteBatchDownloadFromSplunk_ShouldCallDownloadAndImport_WhenFileTypeIsEnabled()
    {
        var fileType = GenerateFileType();
        var uriRequest = GenerateUriRequest();
        var extractResponse = new ExtractResponse { FilePath = "mock-path" };

        _mockConfigService.Setup(x => x.GetFilePathConstants()).ReturnsAsync(new FilePathConstants());
        _mockConfigService.Setup(x => x.GetSplunkInstance(It.IsAny<SplunkInstances>()))
            .ReturnsAsync(new SplunkInstance());

        _mockImportService.Setup(x => x.AddObjectFileMessage(fileType, extractResponse)).Returns(Task.CompletedTask);

        await _splunkService.ExecuteBatchDownloadFromSplunk(fileType, uriRequest, true);

        _mockImportService.Verify(x => x.AddObjectFileMessage(fileType, It.IsAny<ExtractResponse>()), Times.Once);
    }

    [Fact]
    public void ExecuteBatchDownloadFromSplunk_ShouldLogWarning_WhenFileTypeIsDisabled()
    {
        var fileType = GenerateFileType();
        fileType.Enabled = false;

        var uriRequest = GenerateUriRequest();


        _splunkService.ExecuteBatchDownloadFromSplunk(fileType, uriRequest, true);

        _fakeLogger.Collector.LatestRecord.Message.Should()
            .Be($"Filetype {fileType.FileTypeFilePrefix} is not enabled. Please check if this is correct");

        _fakeLogger.Collector.LatestRecord.Level.Should().Be(LogLevel.Warning);
    }

    [Fact]
    public void ExecuteBatchDownloadFromSplunk_ShouldLogError_WhenExceptionThrown()
    {
        var fileType = GenerateFileType();

        var uriRequest = GenerateUriRequest();
        _mockConfigService.Setup(x => x.GetFilePathConstants()).ThrowsAsync(new Exception());

        _ = _splunkService.ExecuteBatchDownloadFromSplunk(fileType, uriRequest, true);

        _fakeLogger.Collector.LatestRecord.Message.Should()
            .Be("An error has occurred while attempting to execute an Azure function");

        _fakeLogger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task GetSearchResultsFromRequestUri_ShouldReturnExtractResponse_ApiTokenValid()
    {
        var fakeToken = GenerateFakeJwt();

        var uriRequest = GenerateUriRequest(isFake: false);
        var mockHandler = new Mock<HttpMessageHandler>();

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{ \"message\": \"Success\" }", Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        _mockHttpClientFactory.Setup(x => x.CreateClient("SplunkApiClient")).Returns(httpClient);
        _mockConfigService.Setup(x => x.GetSplunkClientConfiguration()).ReturnsAsync(new SplunkClient
        {
            QueryTimeout = 30,
            ApiToken = fakeToken,
        });

        // Act
        var resultMessage = await _splunkService.GetSearchResultFromRequestUri(uriRequest);

        // Assert: Ensure the response is not null
        resultMessage.Should().NotBeNull();
        resultMessage.ExtractResponseMessage.Should().NotBeNull();
        resultMessage.ExtractResponseStream.Should().NotBeNull();
        resultMessage.UriRequest.Should().Be(uriRequest);

        resultMessage.ExtractResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert: Read and verify response content
        var contentString = await new StreamReader(resultMessage.ExtractResponseStream).ReadToEndAsync();
        contentString.Should().Contain("Success"); // Verify the JSON content
    }

    [Fact]
    public async Task GetSearchResultsFromRequestUri_ShouldReturnExtractResponse_ApiToken_NotValid()
    {
        var fakeToken = GenerateFakeJwt(isValid: false);
        var uriRequest = GenerateUriRequest(isFake: false);
        _mockConfigService.Setup(x => x.GetSplunkClientConfiguration()).ReturnsAsync(new SplunkClient
        {
            QueryTimeout = 30,
            ApiToken = fakeToken,
        });

        // Act
        var resultMessage = await _splunkService.GetSearchResultFromRequestUri(uriRequest);

        // Assert: Ensure the response is not null
        resultMessage.ExtractResponseMessage.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        resultMessage.ExtractResponseMessage.ReasonPhrase.Should().Be("The authentication token has expired");
    }

    [Fact]
    public async Task GetSearchResultsFromRequestUri_ShouldReturnRequestTimeout_OnOperationCancelledException()
    {
        var fakeToken = GenerateFakeJwt(isValid: false);
        var uriRequest = GenerateUriRequest(isFake: false);
        _mockConfigService.Setup(x => x.GetSplunkClientConfiguration())
            .ThrowsAsync(new OperationCanceledException("Operation timed out"));

        // Act
        var resultMessage = await _splunkService.GetSearchResultFromRequestUri(uriRequest);

        // Assert: Ensure the response is not null
        resultMessage.ExtractResponseMessage.StatusCode.Should().Be(HttpStatusCode.RequestTimeout);
        resultMessage.ExtractResponseMessage.ReasonPhrase.Should().Be("Operation timed out");
    }


    #region Helpers

    private string GenerateFakeJwt(bool isValid = true)
    {
        var securityKey =
            new SymmetricSecurityKey("your-very-secure-and-long-secret-key-123456"u8.ToArray());
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "test-user"),
            new Claim(JwtRegisteredClaimNames.Iss, "fake-issuer"),
            new Claim(JwtRegisteredClaimNames.Aud, "expected-audience"),
            new Claim(JwtRegisteredClaimNames.Exp,
                isValid
                    ? (DateTimeOffset.UtcNow.AddYears(10)).ToUnixTimeSeconds().ToString()
                    : (DateTimeOffset.UtcNow.AddYears(-10)).ToUnixTimeSeconds().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: "fake-issuer",
            audience: "expected-audience",
            claims: claims,
            expires: isValid ? DateTime.UtcNow.AddYears(10) : DateTime.UtcNow.AddYears(-10),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    #endregion
}