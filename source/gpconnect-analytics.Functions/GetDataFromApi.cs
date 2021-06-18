using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.DTO.Response.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gpconnect_analytics.Functions
{
    public class GetDataFromApi
    {
        private readonly ILogger<GetDataFromApi> _logger;
        private readonly IBlobService _blobService;
        private readonly IImportService _importService;
        private readonly ISplunkService _splunkService;
        private readonly IConfigurationService _configurationService;
        private readonly List<FileType> _fileTypes;

        public GetDataFromApi(ILogger<GetDataFromApi> logger, IBlobService blobService, IImportService importService, ISplunkService splunkService, IConfigurationService configurationService)
        {
            _logger = logger;
            _importService = importService;
            _splunkService = splunkService;
            _blobService = blobService;
            _configurationService = configurationService;
            if (_configurationService != null)
            {
                _fileTypes = _configurationService.GetFileTypes().Result;
            }
        }

        [FunctionName("PostPulse")]
        public void PostPulse([TimerTrigger("0 */1 * * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Chris: {DateTime.UtcNow}");
        }

        [FunctionName("GetDataFromAsidLookup")]
        public async Task GetDataFromAsidLookup([TimerTrigger("0 0 0 1-7 * MON", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            var fileType = _fileTypes.FirstOrDefault(x => x.FileTypeFilePrefix == Helpers.FileTypes.asidlookup.ToString());
            await ExecuteDownloadFromSplunk(fileType);
        }

        [FunctionName("GetDataFromSspTrans")]
        public async Task GetDataFromSspTrans([TimerTrigger("0 0 1 * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            var fileType = _fileTypes.FirstOrDefault(x => x.FileTypeFilePrefix == Helpers.FileTypes.ssptrans.ToString());
            await ExecuteDownloadFromSplunk(fileType);
        }

        private async Task ExecuteDownloadFromSplunk(FileType fileType)
        {
            try
            {
                if (FileTypeEnabled(fileType))
                {
                    var result = await _splunkService.DownloadCSV(fileType);
                    switch (result?.ExtractResponseMessage.StatusCode)
                    {
                        case System.Net.HttpStatusCode.OK:

                            var uploadedBlob = await _blobService.AddObjectToBlob(result);
                            if (uploadedBlob != null)
                            {
                                var fileAddedCount = await _importService.AddFile(fileType.FileTypeId, result.FilePath);
                                await _blobService.AddMessageToBlobQueue(fileAddedCount, fileType.FileTypeId, result.FilePath);
                            }
                            break;
                        default:
                            _logger?.LogWarning(result?.ExtractResponseMessage.ToString());
                            break;
                    }
                }
                else
                {
                    _logger?.LogWarning($"Filetype {fileType.FileTypeFilePrefix} is not enabled. Please check if this is correct");
                }
            }
            catch (Exception exc)
            {
                _logger?.LogError(exc, $"An error has occurred while attempting to execute an Azure function");
                throw;
            }
        }

        private bool FileTypeEnabled(FileType fileType)
        {
            return (fileType != null && fileType.Enabled);
        }
    }
}
