using Azure;
using Azure.Storage.Queues;
using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.DTO.Response.Configuration;
using gpconnect_analytics.DTO.Response.Queue;
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

        public BlobService(IConfigurationService configurationService, ILogger<BlobService> logger)
        {
            _logger = logger;
            _configurationService = configurationService;
            _queueClient = new QueueClient(_blobStorageConfiguration.ConnectionString, _blobStorageConfiguration.QueueName);
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
