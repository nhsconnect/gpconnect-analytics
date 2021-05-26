using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.DTO.Response.Configuration;
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
        private readonly BlobStorage _blobStorageConfiguration;
        private readonly QueueClient _queueClient;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IEmailService _emailService;

        public BlobService(IConfigurationService configurationService, ILogger<BlobService> logger, IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
            _configurationService = configurationService;
            _blobStorageConfiguration = _configurationService.GetBlobStorageConfiguration().Result;
            _blobServiceClient = new BlobServiceClient(_blobStorageConfiguration.ConnectionString);
            _queueClient = new QueueClient(_blobStorageConfiguration.ConnectionString, _blobStorageConfiguration.QueueName);            
        }

        public async Task<BlobContentInfo> AddObjectToBlob(ExtractResponse extractResponse)
        {
            _logger.LogInformation($"Adding object to blob storage", extractResponse);

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_blobStorageConfiguration.ContainerName);
                if (await containerClient.ExistsAsync())
                {   
                    var blobClient = containerClient.GetBlobClient(extractResponse.FilePath);
                    var uploadedBlob = await blobClient.UploadAsync(extractResponse.ExtractResponseStream, overwrite: true);
                    return uploadedBlob;
                }
                return null;
            }
            catch (RequestFailedException requestFailedException)
            {
                await _emailService.SendProcessErrorEmail(requestFailedException);
                _logger.LogError(requestFailedException, "The container does not exist");
                throw;
            }
            catch (Exception exc)
            {
                await _emailService.SendProcessErrorEmail(exc);
                _logger.LogError(exc, "An error occurred while trying to add a blob to the storage");
                throw;
            }
        }

        public async Task AddMessageToBlobQueue(int fileAddedCount, int fileTypeId, string blobName)
        {
            try
            {
                if ((await _queueClient.ExistsAsync()) && fileAddedCount == 1)
                {
                    var message = new DTO.Response.Queue.Message
                    {
                        FileTypeId = fileTypeId,
                        BlobName = blobName
                    };

                    var messageText = JsonConvert.SerializeObject(message);
                    _logger.LogInformation($"Adding message to blob queue", message);
                    await _queueClient.SendMessageAsync(messageText.StringToBase64());
                }
            }
            catch (RequestFailedException requestFailedException)
            {
                await _emailService.SendProcessErrorEmail(requestFailedException);
                _logger.LogError(requestFailedException, "The queue does not exist");
                throw;
            }
            catch (Exception exc)
            {
                await _emailService.SendProcessErrorEmail(exc);
                _logger.LogError(exc, "An error occurred while trying to add a message to the queue");
                throw;
            }
        }
    }
}
