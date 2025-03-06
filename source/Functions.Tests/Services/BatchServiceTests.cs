using System.Net;
using Bogus;
using Core.DTOs.Request;
using Core.DTOs.Response.Configuration;
using Core.DTOs.Response.Splunk;
using Core.Helpers;
using Core.Services.Interfaces;
using Dapper;
using FluentAssertions;
using Functions.Services;
using Functions.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Xunit.Abstractions;
using static System.DateTime;

namespace Functions.Tests.Services;

public class BatchServiceTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly Mock<IConfigurationService> _mockConfigurationService;
    private readonly Mock<ISplunkService> _mockSplunkService;
    private readonly Mock<IDataService> _mockDataService;
    private readonly BatchService _batchService;
    private readonly FileType _fileType;
    private readonly SplunkClient _splunkClient;
    private readonly List<UriRequest> _uriList;
    private readonly ExtractResponse _mockExtractResponse;

    private readonly FakeLogger<BatchService> _fakeLogger;

    public BatchServiceTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _mockConfigurationService = new Mock<IConfigurationService>();
        _mockSplunkService = new Mock<ISplunkService>();
        _fakeLogger = new FakeLogger<BatchService>();
        _mockDataService = new Mock<IDataService>();

        _batchService = new BatchService(
            _mockConfigurationService.Object,
            _mockSplunkService.Object,
            _fakeLogger,
            _mockDataService.Object
        );

        _fileType = new FileType
        {
            Enabled = true,
            FileTypeId = 1,
            FileTypeFilePrefix = "test",
            SplunkQuery = "test query {latest}, {earliest}, {hour}"
        };

        _splunkClient = new SplunkClient
        {
            HostName = "fake.splunk.com",
            HostPort = 8089,
            BaseUrl = "/services/search/jobs",
            QueryParameters = "search={0}"
        };

        _uriList = new List<UriRequest>
        {
            new() { Request = new Uri("https://example.com"), EarliestDate = Now, LatestDate = Now }
        };

        _mockExtractResponse = new ExtractResponse
        {
            ExtractResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
            ExtractResponseStream = Stream.Null,
            ExtractRequestDetails = new Extract(),
            FilePath = "test.csv",
            UriRequest = new UriRequest()
        };

        // Common Mocks
        _mockConfigurationService.Setup(x => x.GetFileType(It.IsAny<FileTypes>()))
            .ReturnsAsync(_fileType);

        _mockConfigurationService.Setup(x => x.GetSplunkClientConfiguration())
            .ReturnsAsync(_splunkClient);

        _mockSplunkService.Setup(x => x.DownloadCSVDateRangeAsync(It.IsAny<FileType>(), It.IsAny<UriRequest>(), true))
            .Callback(() => _testOutputHelper.WriteLine("DownloadCSVDateRangeAsync called"))
            .ReturnsAsync(_mockExtractResponse);

        _mockSplunkService.Setup(x =>
                x.ExecuteBatchDownloadFromSplunk(It.IsAny<FileType>(), It.IsAny<UriRequest>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);
    }

    #region StartBatchForToday

    [Fact]
    public async Task StartBatchDownloadForTodayAsync_ShouldProcessUrisAndReturnCount()
    {
        // Act
        var result = await _batchService.StartBatchDownloadForTodayAsync(FileTypes.asidlookup);

        // Assert
        result.Should().Be(24); // One for each hour
        _mockConfigurationService.Verify(x => x.GetFileType(It.IsAny<FileTypes>()), Times.Once);

        _mockSplunkService.Verify(
            x => x.ExecuteBatchDownloadFromSplunk(It.IsAny<FileType>(), It.IsAny<UriRequest>(), true),
            Times.Exactly(24));
    }

    [Fact]
    public async Task StartBatchDownLoadForToday_ShouldCallRemovePreviousDownloads()
    {
        //Arrange
        var expectedProcedureName = "Import.RemovePreviousDownload";

        // Act
        await _batchService.StartBatchDownloadForTodayAsync(FileTypes.asidlookup);

        // Assert
        // RemovePreviousDownload executes the remove stored procedure
        _mockDataService.Verify(x => x.ExecuteStoredProcedure(expectedProcedureName, It.IsAny<DynamicParameters>()),
            Times.Once);
    }

    #endregion

    #region StartBatchDownloadForDateRange

    [Fact]
    public async Task StartBatchDownloadAsync_ShouldReturnReturnProcessCount()
    {
        // Arrange
        var startDate = "2023-01-01";
        var endDate = "2023-01-03";

        // Act
        var result = await _batchService.StartBatchDownloadAsync(FileTypes.asidlookup, startDate, endDate);

        // Assert
        result.Should().Be(72); // 3 days * 24 hours
    }

    [Fact]
    public async Task StartBatchDownloadAsync_ShouldThrowException_WhenStartDateIsAfterEndDate()
    {
        // Arrange
        var startDate = "2023-01-03";
        var endDate = "2023-01-01";

        // Act
        var action = async () => await _batchService.StartBatchDownloadAsync(FileTypes.asidlookup, startDate, endDate);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>().WithMessage("Start date cannot be later than end date");
        _fakeLogger.Collector.LatestRecord.Should().NotBeNull();
        _fakeLogger.Collector.LatestRecord.Message.Should().Be("Start date cannot be later than end date");
        _fakeLogger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
        _fakeLogger.Collector.Count.Should().Be(1);
    }

    [Fact]
    public async Task StartBatchDownLoad_Should_CallRemovePreviousDownloads()
    {
        //Arrange
        var expectedProcedureName = "Import.RemovePreviousDownload";
        var start = DateTime.Now.AddDays(-2).ToString();
        var end = DateTime.Now.AddDays(1).ToString();

        // Act
        await _batchService.StartBatchDownloadAsync(FileTypes.asidlookup, start, end);

        // Assert
        _mockDataService.Verify(x => x.ExecuteStoredProcedure(expectedProcedureName, It.IsAny<DynamicParameters>()),
            Times.Once);
    }

    [Fact]
    public async Task GetBatchDownloadAsync_Should_HandleNullOrEmptyDateInputs()
    {
        // Act

        await _batchService.Invoking(x => x.StartBatchDownloadAsync(FileTypes.asidlookup, null, null))
            .Should()
            .ThrowAsync<ArgumentException>().WithMessage("Start and end dates are required for batch download");

        _fakeLogger.Collector.LatestRecord.Should().NotBeNull();
        _fakeLogger.Collector.LatestRecord.Message.Should().Be("Start and end dates are required for batch download");
        _fakeLogger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
        _fakeLogger.Collector.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetBatchDownloadAsync_Should_HandleGenericExceptions_LoggingError()
    {
        // Act
        var startDate = "2023-01-01";
        var endDate = "2023-01-03";


        _mockSplunkService.Setup(x =>
                x.ExecuteBatchDownloadFromSplunk(It.IsAny<FileType>(), It.IsAny<UriRequest>(), false))
            .Throws(new Exception("Something went wrong"));

        await _batchService.Invoking(x =>
                x.StartBatchDownloadAsync(FileTypes.asidlookup, startDate, endDate))
            .Should()
            .ThrowAsync<Exception>().WithMessage("Something went wrong");

        _fakeLogger.Collector.LatestRecord.Should().NotBeNull();
        _fakeLogger.Collector.LatestRecord.Message.Should()
            .Be("An error occurred during batch download when processing urls");
        _fakeLogger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    #endregion


    [Fact]
    public async Task GetBatchDownloadUriList_ShouldGenerateCorrectUris()
    {
        // Arrange

        var faker = new Faker();
        var fakeDates = faker.Make(3, () => faker.Date.Past(1)).ToList(); // 3 random past dates

        // Fake FileType with a sample SplunkQuery template
        var fakeFileType = new FileType
        {
            SplunkQuery = "index=main | earliest={earliest} latest={latest} hour={hour}"
        };

        // Act
        var result = await _batchService.GetBatchDownloadUriList(fakeFileType, fakeDates);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(fakeDates.Count * 24); // 24 URIs per date

        // Validate a sample URI format
        var sampleUriRequest = result.First();
        sampleUriRequest.Request.Should().NotBeNull();
        sampleUriRequest.EarliestDate = fakeDates.First().AddDays(-2);
        sampleUriRequest.LatestDate = fakeDates.First().AddDays(-1);
        sampleUriRequest.Request.AbsoluteUri.Should().Contain("fake.splunk.com");
        sampleUriRequest.Request.Query.Should().Contain("search=");
    }

    [Fact]
    public async Task RemovePreviousDownloads_Calls_DataService_ExecuteStoreProcedureCorrectly()
    {
        // Arrange
        var startDate = DateTime.Now.AddDays(-4);
        var endDate = DateTime.Now.AddDays(-2);

        var expectedProcedureName = "Import.RemovePreviousDownload";
        var expectedParameters = new DynamicParameters();
        expectedParameters.Add("@StartDate", startDate);
        expectedParameters.Add("@EndDate", endDate);
        expectedParameters.Add("@FileTypeId", _fileType.FileTypeId);


        // Act
        await _batchService.RemovePreviousDownloads(_fileType, startDate, endDate);

        // Assert
        _mockDataService.Verify(x => x.ExecuteStoredProcedure(
                expectedProcedureName,
                It.Is<DynamicParameters>(p =>
                    p.Get<DateTime>("@StartDate") == startDate.AddDays(-2) &&
                    p.Get<DateTime>("@EndDate") == endDate.AddDays(-1) &&
                    p.Get<int>("@FileTypeId") == _fileType.FileTypeId
                )),
            Times.Once);
    }
}