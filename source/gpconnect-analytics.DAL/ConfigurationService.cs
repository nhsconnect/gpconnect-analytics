using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.DTO.Response.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IDataService _dataService;

        public ConfigurationService(IDataService dataService)
        {
            _dataService = dataService;
        }

        public async Task<BlobStorage> GetBlobStorageConfiguration()
        {
            var result = await _dataService.ExecuteStoredProcedure<BlobStorage>("[Configuration].[GetBlobStorageConfiguration]");
            return result.FirstOrDefault();
        }

        public async Task<Email> GetEmailConfiguration()
        {
            var result = await _dataService.ExecuteStoredProcedure<Email>("[Configuration].[GetEmailConfiguration]");
            return result.FirstOrDefault();
        }

        public async Task<FilePathConstants> GetFilePathConstants()
        {
            var result = await _dataService.ExecuteStoredProcedure<FilePathConstants>("[Configuration].[GetFilePathConstants]");
            return result.FirstOrDefault();
        }

        public async Task<List<FileType>> GetFileTypes()
        {
            var result = await _dataService.ExecuteStoredProcedure<FileType>("[Configuration].[GetFileTypes]");
            return result;
        }

        public async Task<SplunkClient> GetSplunkClientConfiguration()
        {
            var result = await _dataService.ExecuteStoredProcedure<SplunkClient>("[Configuration].[GetSplunkClientConfiguration]");
            return result.FirstOrDefault();
        }

        public async Task<List<SplunkInstance>> GetSplunkInstances()
        {
            var result = await _dataService.ExecuteStoredProcedure<SplunkInstance>("[Configuration].[GetSplunkInstances]");
            return result;
        }
    }
}
