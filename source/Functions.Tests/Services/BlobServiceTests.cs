using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using Bogus;
using Core.DTOs.Response.Configuration;
using Core.DTOs.Response.Splunk;
using Core.Services.Interfaces;
using FluentAssertions;
using Functions.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Functions.Tests.Services
{
    public class BlobServiceTests
    {
        private readonly Mock<IConfigurationService> _mockConfigService;
        private readonly Mock<ILogger<BlobService>> _mockLogger;
        private readonly Mock<QueueClient> _mockQueueClient;
        private readonly Mock<BlobServiceClient> _mockBlobServiceClient;
        private readonly Mock<BlobContainerClient> _mockContainerClient;
        private readonly Mock<BlobClient> _mockBlobClient;
        private readonly Faker _faker;

        public BlobServiceTests()
        {
            _faker = new Faker();
            _mockConfigService = new Mock<IConfigurationService>();
            _mockLogger = new Mock<ILogger<BlobService>>();
            _mockQueueClient = new Mock<QueueClient>();
            _mockBlobServiceClient = new Mock<BlobServiceClient>();
            _mockContainerClient = new Mock<BlobContainerClient>();
            _mockBlobClient = new Mock<BlobClient>();
        }

        private BlobService CreateBlobService(
            BlobStorage blobStorageConfig = null,
            QueueClient queueClient = null)
        {
            string connectionString =
                "DefaultEndpointsProtocol=https;" +
                "AccountName=mystorageaccount;" +
                "AccountKey=myAccountKey;" +
                "EndpointSuffix=core.windows.net";

            blobStorageConfig ??= new BlobStorage
            {
                ConnectionString = connectionString,
                ContainerName = _faker.Random.Word(),
                QueueName = _faker.Random.Word()
            };

            _mockConfigService
                .Setup(x => x.GetBlobStorageConfiguration())
                .ReturnsAsync(blobStorageConfig);

            var service = new BlobService(
                _mockConfigService.Object,
                _mockLogger.Object,
                queueClient ?? _mockQueueClient.Object);

            // Replace private BlobServiceClient with mock
            var serviceField = typeof(BlobService)
                .GetField("_blobServiceClient",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            serviceField.SetValue(service, _mockBlobServiceClient.Object);

            return service;
        }

        [Fact]
        public async Task AddObjectToBlob_WhenContainerExists_ShouldUploadSuccessfully()
        {
            // Arrange
            var extractResponse = new ExtractResponse
            {
                FilePath = _faker.System.FileName(),
                ExtractResponseStream = new MemoryStream(Encoding.UTF8.GetBytes(_faker.Lorem.Paragraph()))
            };

            var mockBlobContentInfo = new Mock<BlobContentInfo>();
            var mockResponse = new Mock<Response<BlobContentInfo>>();
            mockResponse.Setup(x => x.Value).Returns(mockBlobContentInfo.Object);

            _mockBlobServiceClient
                .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(_mockContainerClient.Object);

            _mockContainerClient
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(true, new Mock<Response>().Object));

            _mockContainerClient
                .Setup(x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(_mockBlobClient.Object);

            _mockBlobClient
                .Setup(x => x.UploadAsync(
                    It.IsAny<Stream>(),
                    It.Is<bool>(o => o == true),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            var service = CreateBlobService();

            // Act
            var result = await service.AddObjectToBlob(extractResponse);

            // Assert
            result.Should().Be(mockBlobContentInfo.Object);
        }

        [Fact]
        public async Task AddObjectToBlob_WhenContainerDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var extractResponse = new ExtractResponse
            {
                FilePath = _faker.System.FileName(),
                ExtractResponseStream = new MemoryStream(Encoding.UTF8.GetBytes(_faker.Lorem.Paragraph()))
            };

            _mockBlobServiceClient
                .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(_mockContainerClient.Object);

            _mockContainerClient
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(false, Mock.Of<Response>()));

            var service = CreateBlobService();

            // Act
            var result = await service.AddObjectToBlob(extractResponse);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddMessageToBlobQueue_WhenQueueExists_ShouldSendMessage()
        {
            // Arrange
            var fileTypeId = _faker.Random.Int(1, 100);
            var blobName = _faker.System.FileName();

            _mockQueueClient
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

            var service = CreateBlobService();

            // Act
            await service.AddMessageToBlobQueue(1, fileTypeId, blobName);

            // Assert
            _mockQueueClient.Verify(x => x.SendMessageAsync(It.IsAny<string>()), Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Adding message to blob queue")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task AddMessageToBlobQueue_WhenFileCountNotOne_ShouldNotSendMessage()
        {
            // Arrange
            var fileTypeId = _faker.Random.Int(1, 100);
            var blobName = _faker.System.FileName();

            _mockQueueClient
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

            var service = CreateBlobService();

            // Act
            await service.AddMessageToBlobQueue(2, fileTypeId, blobName);

            // Assert
            _mockQueueClient.Verify(
                x => x.SendMessageAsync(It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AddObjectToBlob_WhenRequestFailedExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            var extractResponse = new ExtractResponse
            {
                FilePath = _faker.System.FileName(),
                ExtractResponseStream = new MemoryStream(Encoding.UTF8.GetBytes(_faker.Lorem.Paragraph()))
            };

            var requestFailedException = new RequestFailedException("Container does not exist");

            _mockBlobServiceClient
                .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(_mockContainerClient.Object);

            _mockContainerClient
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(requestFailedException);

            var service = CreateBlobService();

            // Act & Assert
            await Assert.ThrowsAsync<RequestFailedException>(() =>
                service.AddObjectToBlob(extractResponse));

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("The container does not exist")),
                    requestFailedException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task AddObjectToBlob_ShouldLogError_AndThrow_OnGenericException()
        {
            // Arrange
            var extractResponse = new ExtractResponse
            {
                FilePath = _faker.System.FileName(),
                ExtractResponseStream = new MemoryStream(Encoding.UTF8.GetBytes(_faker.Lorem.Paragraph()))
            };

            var genericException = new Exception("Something bad happened");

            _mockBlobServiceClient
                .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(_mockContainerClient.Object);

            _mockContainerClient
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(genericException);

            var service = CreateBlobService();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                service.AddObjectToBlob(extractResponse));

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) =>
                        o.ToString().Contains("An error occurred while trying to add a blob to the storage")),
                    genericException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task AddMessageToBlobQueue_WhenQueueDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var fileTypeId = _faker.Random.Int(1, 100);
            var blobName = _faker.System.FileName();

            var requestFailedException = new RequestFailedException("Queue does not exist");

            _mockQueueClient
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(requestFailedException);

            var service = CreateBlobService();

            // Act & Assert
            await Assert.ThrowsAsync<RequestFailedException>(() =>
                service.AddMessageToBlobQueue(1, fileTypeId, blobName));

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("The queue does not exist")),
                    requestFailedException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task AddMessageToBlobQueue_ShouldThrowError_AndLogMessage_OnGenericException()
        {
            // Arrange
            var fileTypeId = _faker.Random.Int(1, 100);
            var blobName = _faker.System.FileName();

            var genericException = new Exception("Something bad happened");

            _mockQueueClient
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(genericException);

            var service = CreateBlobService();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                service.AddMessageToBlobQueue(1, fileTypeId, blobName));

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) =>
                        o.ToString().Contains("An error occurred while trying to add a message to the queue")),
                    genericException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}