using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.DTO.Response.Configuration;
using gpconnect_analytics.DTO.Response.Queue;
using gpconnect_analytics.DTO.Response.Splunk;
using gpconnect_analytics.Helpers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL
{
    public class BlobService : IBlobService
    {
        private readonly ILogger<BlobService> _logger;
        private readonly IConfigurationService _configurationService;
        private BlobStorage _blobStorageConfiguration;
        private readonly QueueClient _queueClient;
        private readonly BlobServiceClient _blobServiceClient;

        public BlobService(IConfigurationService configurationService, ILogger<BlobService> logger)
        {
            _logger = logger;
            _configurationService = configurationService;
            _queueClient = new QueueClient(_blobStorageConfiguration.ConnectionString, _blobStorageConfiguration.QueueName);
            _blobServiceClient = new BlobServiceClient(_blobStorageConfiguration.ConnectionString);
        }

        public async Task<BlobContentInfo> AddObjectToBlob(ExtractResponse extractResponse)
        {
            _logger.LogInformation($"Adding object to blob storage", extractResponse);
            _blobStorageConfiguration = await _configurationService.GetBlobStorageConfiguration();
            var containerClient = _blobServiceClient.GetBlobContainerClient(_blobStorageConfiguration.ContainerName);
            try
            {
                if (await _queueClient.ExistsAsync())
                {
                    var prefixedFilePath = AddFolders(extractResponse.FilePath);
                    var blobClient = containerClient.GetBlobClient(prefixedFilePath);
                    var uploadedBlob = await blobClient.UploadAsync(extractResponse.ExtractResponseStream);
                    return uploadedBlob;
                }
                return null;
            }
            catch (RequestFailedException requestFailedException)
            {
                _logger.LogError(requestFailedException, "The queue does not exist");
                throw;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "An error occurred while trying to add a blob to the queue");
                throw;
            }
        }

        private string AddFolders(string filePath)
        {
            _logger.LogInformation($"Adding folder to blob storage", filePath);
            return filePath;
        }

        public async Task AddMessageToBlobQueue(int fileAddedCount, int fileTypeId)
        {
            _blobStorageConfiguration = await _configurationService.GetBlobStorageConfiguration();

            try
            {
                if (await _queueClient.ExistsAsync() && fileAddedCount == 1)
                {
                    var message = new Message
                    {
                        FileTypeId = fileTypeId
                    };

                    var messageText = JsonConvert.SerializeObject(message);
                    _logger.LogInformation($"Adding message to blob queue", message);
                    await _queueClient.SendMessageAsync(messageText.StringToBase64());
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
