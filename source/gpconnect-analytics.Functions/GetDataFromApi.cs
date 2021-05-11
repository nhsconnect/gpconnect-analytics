using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.DTO.Response.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly List<SplunkInstance> _splunkInstances;
        private readonly FilePathConstants _filePathConstants;

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
                _filePathConstants = _configurationService.GetFilePathConstants().Result;
                _splunkInstances = _configurationService.GetSplunkInstances().Result;
            }
        }

        [FunctionName("GetDataFromAsidLookup")]
        public async Task GetDataFromAsidLookup([TimerTrigger("0 0 1 * * MON", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
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
                var splunkInstance = _splunkInstances.FirstOrDefault(x => x.Source == Helpers.SplunkInstances.cloud.ToString());

                if (fileType != null && splunkInstance != null)
                {
                    var result = await _splunkService.DownloadCSV(fileType.FileTypeId);

                    if (result.ExtractResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var filePath = @$"{ConstructFilePath(splunkInstance, fileType, result.ExtractRequestDetails)}";
                        var fileAddedCount = await _importService.AddFile(filePath);
                        await _blobService.AddMessageToBlobQueue(fileAddedCount, fileType.FileTypeId);
                    }
                }
            }
            catch (Exception exc)
            {
                _logger?.LogError(exc, $"An error has occurred while attempting to execute an Azure function");
                throw;
            }
        }

        private string ConstructFilePath(SplunkInstance splunkInstance, FileType fileType, DTO.Response.Splunk.Extract extractRequestDetails)
        {
            var filePathString = new StringBuilder();
            filePathString.Append(fileType.DirectoryName);
            filePathString.Append(_filePathConstants.PathSeparator);
            filePathString.Append(_filePathConstants.ProjectNameFilePrefix);
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(fileType.FileTypeFilePrefix);
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(extractRequestDetails.QueryFromDate);
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(extractRequestDetails.QueryToDate);
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(splunkInstance.Source);
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(DateTimeOffset.UtcNow.ToString());
            filePathString.Append(_filePathConstants.FileExtension);
            return filePathString.ToString();
        }
    }
}
