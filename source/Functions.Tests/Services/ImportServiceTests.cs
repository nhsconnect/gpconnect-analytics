using System.Data;
using Azure.Storage.Blobs.Models;
using Bogus;
using Core.DTOs.Request;
using Core.DTOs.Response.Configuration;
using Core.DTOs.Response.Queue;
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

namespace Functions.Tests.Services;

public class ImportServiceTests
{
    private readonly Mock<IConfigurationService> _mockConfigService;
    private readonly Mock<IDataService> _mockDataService;
    private readonly Mock<IBlobService> _mockBlobService;
    private readonly FakeLogger<ImportService> _fakeLogger;
    private readonly ImportService _importService;
    private readonly Mock<IFileService> _mockFileService;

    public ImportServiceTests()
    {
        _fakeLogger = new FakeLogger<ImportService>();
        _mockConfigService = new Mock<IConfigurationService>();
        _mockDataService = new Mock<IDataService>();
        _mockBlobService = new Mock<IBlobService>();
        _mockFileService = new Mock<IFileService>();


        _importService = new ImportService(
            _mockConfigService.Object,
            _mockDataService.Object,
            _mockBlobService.Object,
            _fakeLogger,
            _mockFileService.Object
        );
    }

    #region AddDownloadFileManually Tests

    [Fact]
    public void AddDownloadFileManually_ShouldAddFileToDb_And_AddToBlobQueue()
    {
        // Arrange
        var fakeFileType = GenerateFileType();
        var filePath = "asid-lookup-data/testfile.csv";
        _mockConfigService.Setup(x => x.GetFileType(FileTypes.asidlookup)).ReturnsAsync(fakeFileType);

        _mockBlobService
            .Setup(x => x.AddMessageToBlobQueue(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<bool>()));


        _mockFileService.Setup(x => x.ApiReaderAddFile(fakeFileType.FileTypeId, filePath, true))
            .ReturnsAsync(1);

        // Act
        var result = _importService.AddDownloadedFileManually(filePath);

        // Assert
        _mockBlobService.Verify(x =>
                x.AddMessageToBlobQueue(
                    1, // mocked file count added
                    fakeFileType.FileTypeId,
                    filePath,
                    true), Times.Once
        );

        // Verify it calls internal AddFileMessage - which in turn calls the stored procedure
        _mockFileService.Verify(x => x.ApiReaderAddFile(fakeFileType.FileTypeId, filePath, true), Times.Once);
    }

    [Fact]
    public async Task AddDownloadFileManually_ShouldThrowArgumentException_WhenFileTypeFromPathNull()
    {
        // Arrange
        var filePath = "non-existant-fileType/testfile.csv";

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _importService.AddDownloadedFileManually(filePath)
        );

        // Assert
        ex.Message.Should().Be("Filepath does not contain vailid file type suffix");
    }

    #endregion


    #region AddObjectFileMessage Tests

    [Fact]
    public async Task AddObjectFileMessage_ShouldAddObjectToBlob_WhenSuccessfulStatusCode()
    {
        //Arrange
        var fileType = GenerateFileType();
        var extractResponse = new ExtractResponse()
        {
            ExtractResponseMessage = new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
            },
            FilePath = "asid-lookup-data/testfile.csv",
        };

        // Act
        await _importService.AddObjectFileMessage(fileType, extractResponse);

        // Assert
        _mockBlobService.Verify(x => x.AddObjectToBlob(extractResponse), Times.Once);
    }

    [Fact]
    public async Task AddObjectFileMessage_LogsWarning_WhenStatusCodeNotOk()
    {
        //Arrange
        var fileType = GenerateFileType();
        var extractResponse = new ExtractResponse()
        {
            ExtractResponseMessage = new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
            },
            FilePath = "asid-lookup-data/testfile.csv",
        };

        // Act
        await _importService.AddObjectFileMessage(fileType, extractResponse);

        // Assert
        _fakeLogger.LatestRecord.Message.Should().Be(extractResponse.ExtractResponseMessage.ToString());
        _fakeLogger.LatestRecord.Level.Should().Be(LogLevel.Warning);
    }

    [Fact]
    public async Task AddObjectFileMessage_DoesNotCallAddObjectToBlob_WhenStatusCodeNotOk()
    {
        //Arrange
        var fileType = GenerateFileType();
        var extractResponse = new ExtractResponse()
        {
            ExtractResponseMessage = new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
            },
            FilePath = "asid-lookup-data/testfile.csv",
        };

        // Act
        await _importService.AddObjectFileMessage(fileType, extractResponse);

        // Assert
        _mockBlobService.Verify(x => x.AddObjectToBlob(extractResponse), Times.Never);
    }

    [Fact]
    public async Task AddObjectFileMessage_ShouldAddFileToDb_And_AddMessageToBlobQueue()
    {
        // Arrange
        var fileType = GenerateFileType();
        var extractResponse = new ExtractResponse()
        {
            ExtractResponseMessage = new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
            },
            FilePath = "asid-lookup-data/testfile.csv",
        };

        _mockBlobService
            .Setup(x => x.AddObjectToBlob(extractResponse))
            .ReturnsAsync(Mock.Of<BlobContentInfo>());

        _mockFileService.Setup(x => x.ApiReaderAddFile(fileType.FileTypeId, extractResponse.FilePath, true))
            .ReturnsAsync(1);

        // Act
        await _importService.AddObjectFileMessage(fileType, extractResponse);

        // Assert
        _mockBlobService.Verify(x =>
                x.AddMessageToBlobQueue(
                    1, // mocked file count added
                    fileType.FileTypeId,
                    extractResponse.FilePath,
                    true), Times.Once
        );

        _mockFileService.Verify(x => x.ApiReaderAddFile(fileType.FileTypeId, extractResponse.FilePath, true),
            Times.Once);
    }

    [Fact]
    public async Task AddObjectFileMessage_ShouldNotAddFileToDb_And_AddMessageToBlobQueue_WhenStatusNotOk()
    {
        // Arrange
        var fileType = GenerateFileType();
        var extractResponse = new ExtractResponse()
        {
            ExtractResponseMessage = new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
            },
            FilePath = "asid-lookup-data/testfile.csv",
        };

        _mockBlobService
            .Setup(x => x.AddObjectToBlob(extractResponse))
            .ReturnsAsync(Mock.Of<BlobContentInfo>());

        _mockFileService.Setup(x => x.ApiReaderAddFile(fileType.FileTypeId, extractResponse.FilePath, true))
            .ReturnsAsync(1);

        // Act
        await _importService.AddObjectFileMessage(fileType, extractResponse);

        // Assert
        _mockBlobService.Verify(x =>
                x.AddMessageToBlobQueue(
                    1, // mocked file count added
                    fileType.FileTypeId,
                    extractResponse.FilePath,
                    true), Times.Never
        );

        _mockFileService.Verify(x => x.ApiReaderAddFile(fileType.FileTypeId, extractResponse.FilePath, true),
            Times.Never);
    }

    #endregion

    #region InstallData Tests

    [Fact]
    public async Task InstallData_ShouldExecuteStoredProcedure_InstallNextFile_WithCorrectParameters()
    {
        // Arrange
        var fileType = GenerateFileType();
        var mockQueueItem = new Message
        {
            FileTypeId = fileType.FileTypeId,
            BlobName = "RandomBlobName",
            Override = true,
        };

        var procedureName = "Import.InstallNextFile";
        var callCount = 0;
        var capturedParams = new List<DynamicParameters>();

        _mockDataService
            .Setup(x => x.ExecuteStoredProcedureWithOutputParameters(procedureName, It.IsAny<DynamicParameters>()))
            .Callback<string, DynamicParameters>((_, parameters) =>
            {
                // Capture a copy of the parameters
                var copiedParams = new DynamicParameters(parameters);
                capturedParams.Add(copiedParams);

                // Simulate stored procedure behavior: first call sets MoreFilesToInstall = true, second = false
                parameters.Add("@MoreFilesToInstall", callCount == 0, DbType.Boolean, ParameterDirection.Output);
                callCount++;
            })
            .ReturnsAsync((string proc, DynamicParameters parameters) => parameters);


        // Act
        await _importService.InstallData(mockQueueItem);

        // Assert
        _mockDataService.Verify(
            x => x.ExecuteStoredProcedureWithOutputParameters(procedureName, It.IsAny<DynamicParameters>()),
            Times.Exactly(2));

        // Validate first call parameters
        Assert.Equal(mockQueueItem.FileTypeId, capturedParams[0].Get<int>("@FileTypeId"));
        Assert.Equal(mockQueueItem.Override, capturedParams[0].Get<bool>("@Override"));

        // Validate second call parameters
        Assert.Equal(mockQueueItem.FileTypeId, capturedParams[1].Get<int>("@FileTypeId"));
        Assert.Equal(mockQueueItem.Override, capturedParams[1].Get<bool>("@Override"));

        // Validate log messages are recorded:
        _fakeLogger.Collector.GetSnapshot()[0].Message.Should().Be("Installing file into database");
        var messages = _fakeLogger.Collector.GetSnapshot();

        var moreFilesMessageTrue = messages
            .Any(x => x.Message.Contains("More files to install? True"));
        moreFilesMessageTrue.Should().BeTrue();

        var moreFilesMessageFalse = messages
            .Any(x => x.Message.Contains("More files to install? False"));

        moreFilesMessageFalse.Should().BeTrue();
    }

    #endregion

    #region Private Methods

    private static FileType GenerateFileType() => new Faker<FileType>()
        .RuleFor(f => f.FileTypeFilePrefix, f => f.Random.Word())
        .RuleFor(f => f.DirectoryName, f => f.System.DirectoryPath())
        .RuleFor(f => f.Enabled, true)
        .Generate();

    #endregion
}