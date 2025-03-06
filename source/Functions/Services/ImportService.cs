using System.Data;
using Core.DTOs.Response.Configuration;
using Core.DTOs.Response.Queue;
using Core.DTOs.Response.Splunk;
using Core.Helpers;
using Core.Services.Interfaces;
using Dapper;
using Functions.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Functions.Services
{
    public class ImportService(
        IConfigurationService configurationService,
        IDataService dataService,
        IBlobService blobService,
        ILogger<ImportService> logger,
        IFileService fileService)
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
            var fileAddedCount =
                await fileService.ApiReaderAddFile(fileType.FileTypeId, filePath, true);

            await blobService.AddMessageToBlobQueue(fileAddedCount, fileType.FileTypeId, filePath,
                true);
        }

        public async Task AddObjectFileMessage(FileType fileType, ExtractResponse extractResponse)
        {
            if (extractResponse.ExtractResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var uploadedBlob = await blobService.AddObjectToBlob(extractResponse);
                if (uploadedBlob != null)
                {
                    var fileAddedCount =
                        await fileService.ApiReaderAddFile(fileType.FileTypeId, extractResponse.FilePath, true);

                    await blobService.AddMessageToBlobQueue(fileAddedCount, fileType.FileTypeId,
                        extractResponse.FilePath,
                        true);
                }
            }
            else
            {
                logger?.LogWarning(extractResponse?.ExtractResponseMessage.ToString());
            }
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