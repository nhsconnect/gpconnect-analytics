using System.Net;
using Core.DTOs.Request;
using Core.DTOs.Response.Configuration;
using Core.DTOs.Response.Splunk;
using Core.Helpers;
using Core.Services.Interfaces;
using Dapper;
using FakeItEasy;
using FluentAssertions;
using function_app.Services;
using function_app.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace gpconnect_analytics.Functions.UnitTests.Services;

public class BatchServiceTests
{
    [Fact]
    public async Task StartBatchDownloadForTodayAsync_ShouldProcessUrisAndReturnCount()
    {
        // Arrange
        var mockConfigurationService = A.Fake<IConfigurationService>();
        var mockImportService = A.Fake<IImportService>();
        var mockSplunkService = A.Fake<ISplunkService>();
        var mockLogger = A.Fake<ILogger<BatchService>>();
        var mockDataService = A.Fake<IDataService>();
        var mockExtractResponse = new ExtractResponse()
        {
            ExtractResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
            ExtractResponseStream = Stream.Null,
            ExtractRequestDetails = new Extract(),
            FilePath = "test.csv",
            UriRequest = new UriRequest()
        };

        A.CallTo(() =>
                mockDataService.ExecuteStoredProcedure("Import.RemovePreviousDownload", A<DynamicParameters>.Ignored))
            .Returns(Task.FromResult(1));

        A.CallTo(() => mockSplunkService.DownloadCSVDateRangeAsync(A<FileType>._, A<UriRequest>._, true))
            .Returns(mockExtractResponse);

        A.CallTo(() => mockImportService.AddObjectFileMessage(A<FileType>._, mockExtractResponse))
            .Returns(Task.CompletedTask);

        var batchService = new BatchService(
            mockConfigurationService,
            mockImportService,
            mockSplunkService,
            mockLogger,
            mockDataService
        );

        var splunkQuery = "test query {latest}, {earliest}, {hour}";

        var fileType = new FileType
        {
            Enabled = true,
            FileTypeId = 1,
            FileTypeFilePrefix = "test",
            SplunkQuery = splunkQuery
        };
        var uriList = new List<UriRequest>
        {
            new() { Request = new Uri("https://example.com"), EarliestDate = DateTime.Now, LatestDate = DateTime.Now }
        };

        A.CallTo(() => mockConfigurationService.GetFileType(A<FileTypes>._))
            .Returns(Task.FromResult(fileType));

        A.CallTo(() => mockConfigurationService.GetSplunkClientConfiguration())
            .Returns(Task.FromResult(new SplunkClient
                { HostName = "localhost", HostPort = 8080, BaseUrl = "api", QueryParameters = "test parameters {0}" }));

        // Act
        var result = await batchService.StartBatchDownloadForTodayAsync(FileTypes.asidlookup);

        // Assert
        const int hoursInDay = 24;
        result.Should().Be(hoursInDay);

        A.CallTo(() => mockConfigurationService.GetFileType(A<FileTypes>.That.Matches(x => x == FileTypes.asidlookup)))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() =>
                mockDataService.ExecuteStoredProcedure("Import.RemovePreviousDownload", A<DynamicParameters>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => mockSplunkService.DownloadCSVDateRangeAsync(A<FileType>._, A<UriRequest>._, true))
            .MustHaveHappened(24, Times.Exactly); // One for each hour

        A.CallTo(() => mockImportService.AddObjectFileMessage(A<FileType>._, A<ExtractResponse>._))
            .MustHaveHappened(24, Times.Exactly);
    }
}