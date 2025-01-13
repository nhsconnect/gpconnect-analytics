using Core.DTOs;
using Core.DTOs.Response.Configuration;
using Core.Helpers;
using Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace function_app.Services
{
    public class ConfigurationService(
        IDataService dataService,
        ILogger<ConfigurationService> logger)
        : IConfigurationService
    {
        public async Task<BlobStorage> GetBlobStorageConfiguration()
        {
            var result =
                await dataService.ExecuteStoredProcedure<BlobStorage>("[Configuration].[GetBlobStorageConfiguration]");
            logger.LogInformation("Loading blob storage configuration", result.FirstOrDefault());
            return result.FirstOrDefault();
        }

        public async Task<FilePathConstants> GetFilePathConstants()
        {
            var result =
                await dataService.ExecuteStoredProcedure<FilePathConstants>("[Configuration].[GetFilePathConstants]");
            logger.LogInformation("Loading file path constants", result.FirstOrDefault());
            return result.FirstOrDefault();
        }

        public async Task<List<FileType>> GetFileTypes()
        {
            var result = await dataService.ExecuteStoredProcedure<FileType>("[Configuration].[GetFileTypes]");
            logger.LogInformation("Loading file types", result);
            return result;
        }

        public async Task<FileType> GetFileType(FileTypes fileTypes)
        {
            var result = await dataService.ExecuteStoredProcedure<FileType>("[Configuration].[GetFileTypes]");
            return result.FirstOrDefault(ft => ft.FileTypeFilePrefix == fileTypes.ToString());
        }

        public async Task<SplunkClient> GetSplunkClientConfiguration()
        {
            var result =
                await dataService.ExecuteStoredProcedure<SplunkClient>(
                    "[Configuration].[GetSplunkClientConfiguration]");
            logger.LogInformation("Loading splunk client configuration", result.FirstOrDefault());
            return result.FirstOrDefault();
        }

        public async Task<SplunkInstance> GetSplunkInstance(SplunkInstances splunkInstance)
        {
            var result =
                await dataService.ExecuteStoredProcedure<SplunkInstance>("[Configuration].[GetSplunkInstances]");
            logger.LogInformation("Loading splunk instance", result);
            return result.FirstOrDefault(x => x.Source == splunkInstance.ToString());
        }
    }
}