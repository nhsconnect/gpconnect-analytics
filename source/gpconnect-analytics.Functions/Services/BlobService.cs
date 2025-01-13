using System.Text;
using System.Text.Json;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using Core.DTOs.Response.Configuration;
using Core.DTOs.Response.Queue;
using Core.DTOs.Response.Splunk;
using Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace function_app.Services
{
    public class BlobService : IBlobService
    {
        private readonly ILogger<BlobService> _logger;
        private readonly BlobStorage _blobStorageConfiguration;
        private readonly QueueClient _queueClient;
        private readonly BlobServiceClient _blobServiceClient;

        public BlobService(IConfigurationService configurationService, ILogger<BlobService> logger)
        {
            _logger = logger;
            _blobStorageConfiguration = configurationService.GetBlobStorageConfiguration().Result;
            _blobServiceClient = new BlobServiceClient(_blobStorageConfiguration.ConnectionString);
            _queueClient = new QueueClient(_blobStorageConfiguration.ConnectionString,
                _blobStorageConfiguration.QueueName);
        }

        public async Task<BlobContentInfo?> AddObjectToBlob(ExtractResponse extractResponse)
        {
            _logger.LogInformation($"Adding object to blob storage", extractResponse);

            try
            {
                var containerClient =
                    _blobServiceClient.GetBlobContainerClient(_blobStorageConfiguration.ContainerName);

                if (!await containerClient.ExistsAsync()) return null;

                var blobClient = containerClient.GetBlobClient(extractResponse.FilePath);
                var response =
                    await blobClient.UploadAsync(extractResponse.ExtractResponseStream, overwrite: true);

                return response;
            }
            catch (RequestFailedException requestFailedException)
            {
                _logger.LogError(requestFailedException, "The container does not exist");
                throw;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "An error occurred while trying to add a blob to the storage");
                throw;
            }
        }

        public async Task AddMessageToBlobQueue(int fileAddedCount, int fileTypeId, string blobName,
            bool overrideEntry = false)
        {
            try
            {
                if ((await _queueClient.ExistsAsync()) && fileAddedCount == 1)
                {
                    var message = new Message
                    {
                        FileTypeId = fileTypeId,
                        BlobName = blobName,
                        Override = overrideEntry
                    };

                    var messageText = JsonSerializer.Serialize(message);
                    _logger.LogInformation($"Adding message to blob queue", message);
                    await _queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(messageText)));
                }
            }
            catch (RequestFailedException requestFailedException)
            {
                _logger.LogError(requestFailedException, "The queue does not exist");
                throw;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "An error occurred while trying to add a message to the queue");
                throw;
            }
        }
    }
}