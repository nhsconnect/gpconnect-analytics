using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.DTO.Response.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly ILogger<ConfigurationService> _logger; 
        private readonly IDataService _dataService;

        public ConfigurationService(IDataService dataService, ILogger<ConfigurationService> logger)
        {
            _logger = logger;
            _dataService = dataService;
        }

        public async Task<BlobStorage> GetBlobStorageConfiguration()
        {            
            var result = await _dataService.ExecuteStoredProcedure<BlobStorage>("[Configuration].[GetBlobStorageConfiguration]");
            _logger.LogInformation($"Loading blob storage configuration", result.FirstOrDefault());
            return result.FirstOrDefault();
        }

        public async Task<Email> GetEmailConfiguration()
        {
            var result = await _dataService.ExecuteStoredProcedure<Email>("[Configuration].[GetEmailConfiguration]");
            _logger.LogInformation($"Loading email configuration", result.FirstOrDefault());
            return result.FirstOrDefault();
        }

        public async Task<FilePathConstants> GetFilePathConstants()
        {
            var result = await _dataService.ExecuteStoredProcedure<FilePathConstants>("[Configuration].[GetFilePathConstants]");
            _logger.LogInformation($"Loading file path constants", result.FirstOrDefault());
            return result.FirstOrDefault();
        }

        public async Task<List<FileType>> GetFileTypes()
        {
            var result = await _dataService.ExecuteStoredProcedure<FileType>("[Configuration].[GetFileTypes]");
            _logger.LogInformation($"Loading file types", result);
            return result;
        }

        public async Task<SplunkClient> GetSplunkClientConfiguration()
        {
            var result = await _dataService.ExecuteStoredProcedure<SplunkClient>("[Configuration].[GetSplunkClientConfiguration]");
            _logger.LogInformation($"Loading splunk client configuration", result.FirstOrDefault());
            return result.FirstOrDefault();
        }

        public async Task<List<SplunkInstance>> GetSplunkInstances()
        {
            var result = await _dataService.ExecuteStoredProcedure<SplunkInstance>("[Configuration].[GetSplunkInstances]");
            _logger.LogInformation($"Loading splunk instances", result);
            return result;
        }
    }
}
