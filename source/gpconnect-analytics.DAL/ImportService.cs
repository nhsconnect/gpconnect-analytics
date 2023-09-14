using Dapper;
using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.DTO.Response.Configuration;
using gpconnect_analytics.DTO.Response.Queue;
using gpconnect_analytics.DTO.Response.Splunk;
using gpconnect_analytics.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL
{
    public class ImportService : IImportService
    {
        private readonly ILogger<ImportService> _logger;
        private readonly IDataService _dataService;
        private readonly IBlobService _blobService;
        private readonly IConfigurationService _configurationService;

        public ImportService(IConfigurationService configurationService, IDataService dataService, IBlobService blobService, ILogger<ImportService> logger)
        {
            _logger = logger;
            _configurationService = configurationService;
            _dataService = dataService;
            _blobService = blobService;
        }

        public async Task<IActionResult> AddDownloadedFileManually(HttpRequest req)
        {
            var fileTypes = (FileTypes?)Enum.Parse(typeof(FileTypes), req.Query["FileType"].ToString());
            if (fileTypes != null)
            {
                var fileType = await _configurationService.GetFileType((FileTypes)fileTypes);
                var filePath = req.Query["FilePath"].ToString();
                await AddFileMessage(fileType, new ExtractResponse() { FilePath = filePath });
                return new OkObjectResult($"Import of {filePath} complete");
            }
            return new BadRequestObjectResult("Bad request");
        }

        public async Task AddObjectFileMessage(FileType fileType, ExtractResponse extractResponse)
        {
            switch (extractResponse?.ExtractResponseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var uploadedBlob = await _blobService.AddObjectToBlob(extractResponse);
                    if (uploadedBlob != null)
                    {
                        await AddFileMessage(fileType, extractResponse);
                    }
                    break;
                default:
                    _logger?.LogWarning(extractResponse?.ExtractResponseMessage.ToString());
                    break;
                    throw new Exception($"Splunk has returned the following HTTP status code {extractResponse?.ExtractResponseMessage.StatusCode}");
            }
        }

        public async Task AddFileMessage(FileType fileType, ExtractResponse extractResponse)
        {
            var fileAddedCount = await AddFile(fileType.FileTypeId, extractResponse.FilePath, true);
            await _blobService.AddMessageToBlobQueue(fileAddedCount, fileType.FileTypeId, extractResponse.FilePath, true);
        }

        public async Task<int> AddFile(int fileTypeId, string filePath, bool overrideFile)
        {
            var procedureName = "ApiReader.AddFile";
            var parameters = new DynamicParameters();
            parameters.Add("@FileTypeId", fileTypeId);
            parameters.Add("@FilePath", filePath);
            parameters.Add("@Override", overrideFile);
            var result = await _dataService.ExecuteStoredProcedure(procedureName, parameters);
            return result;
        }

        public async Task InstallData(Message queueItem)
        {
            bool moreFilesToInstall = true;            
            var procedureName = "Import.InstallNextFile";
            var parameters = new DynamicParameters();
            parameters.Add("@FileTypeId", queueItem.FileTypeId);
            if(queueItem.Override)
            {
                parameters.Add("@Override", queueItem.Override, dbType: DbType.Boolean, direction: ParameterDirection.Input);
            }
            parameters.Add("@MoreFilesToInstall", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            while (moreFilesToInstall)
            {
                _logger.LogInformation($"Installing file into database", parameters);
                var result = await _dataService.ExecuteStoredProcedureWithOutputParameters(procedureName, parameters);
                moreFilesToInstall = result.Get<bool>("@MoreFilesToInstall");
                _logger.LogInformation($"More files to install? {moreFilesToInstall}");
            };
        }
    }
}
