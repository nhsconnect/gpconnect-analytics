using System.Data;
using Core.DTOs.Response.Configuration;
using Core.DTOs.Response.Queue;
using Core.DTOs.Response.Splunk;
using Core.Helpers;
using Core.Services.Interfaces;
using Dapper;
using function_app.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace function_app.Services
{
    public class ImportService(
        IConfigurationService configurationService,
        IDataService dataService,
        IBlobService blobService,
        ILogger<ImportService> logger)
        : IImportService
    {
        public async Task AddDownloadedFileManually(string filePath)
        {
            var fileTypeFromPath = filePath.GetFileType<FileTypes>();

            if (fileTypeFromPath == null)
            {
                throw new ArgumentException("Filepath does not contain vailid file type suffix");
            }

            var fileType = await configurationService.GetFileType((FileTypes)fileTypeFromPath);
            await AddFileMessage(fileType, new ExtractResponse() { FilePath = filePath });
        }

        public async Task AddObjectFileMessage(FileType fileType, ExtractResponse extractResponse)
        {
            if (extractResponse.ExtractResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var uploadedBlob = await blobService.AddObjectToBlob(extractResponse);
                if (uploadedBlob != null)
                {
                    await AddFileMessage(fileType, extractResponse);
                }
            }
            else
            {
                logger?.LogWarning(extractResponse?.ExtractResponseMessage.ToString());
            }
        }

        public async Task AddFileMessage(FileType fileType, ExtractResponse extractResponse)
        {
            var fileAddedCount = await AddFile(fileType.FileTypeId, extractResponse.FilePath, true);
            await blobService.AddMessageToBlobQueue(fileAddedCount, fileType.FileTypeId, extractResponse.FilePath,
                true);
        }

        public async Task<int> AddFile(int fileTypeId, string filePath, bool overrideFile)
        {
            const string procedureName = "ApiReader.AddFile";
            var parameters = new DynamicParameters();
            parameters.Add("@FileTypeId", fileTypeId);
            parameters.Add("@FilePath", filePath);
            parameters.Add("@Override", overrideFile);
            var result = await dataService.ExecuteStoredProcedure(procedureName, parameters);
            return result;
        }

        public async Task InstallData(Message queueItem)
        {
            var moreFilesToInstall = true;
            const string procedureName = "Import.InstallNextFile";
            var parameters = new DynamicParameters();
            parameters.Add("@FileTypeId", queueItem.FileTypeId);
            if (queueItem.Override)
            {
                parameters.Add("@Override", queueItem.Override, dbType: DbType.Boolean,
                    direction: ParameterDirection.Input);
            }

            parameters.Add("@MoreFilesToInstall", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            while (moreFilesToInstall)
            {
                logger.LogInformation($"Installing file into database", parameters);
                var result = await dataService.ExecuteStoredProcedureWithOutputParameters(procedureName, parameters);
                moreFilesToInstall = result.Get<bool>("@MoreFilesToInstall");
                logger.LogInformation($"More files to install? {moreFilesToInstall}");
            }
        }
    }
}