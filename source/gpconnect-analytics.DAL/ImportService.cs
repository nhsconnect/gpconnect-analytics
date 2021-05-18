using Dapper;
using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.DTO.Response.Configuration;
using gpconnect_analytics.DTO.Response.Import;
using gpconnect_analytics.DTO.Response.Queue;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL
{
    public class ImportService : IImportService
    {
        private readonly ILogger<ImportService> _logger;
        private readonly IDataService _dataService;
        private readonly IConfigurationService _configurationService;
        private List<FileType> _fileTypes;

        public ImportService(IDataService dataService, IConfigurationService configurationService, ILogger<ImportService> logger)
        {
            _logger = logger;
            _dataService = dataService;
            _configurationService = configurationService;
        }

        public async Task<int> AddFile(string filePath)
        {
            _fileTypes = await _configurationService.GetFileTypes();

            var directoryName = filePath.Split(new char[] { '\\' })[0];
            var fileType = _fileTypes.FirstOrDefault(x => x.DirectoryName == directoryName);

            _logger.LogInformation($"Adding file of type {fileType.FileTypeFilePrefix} to database", filePath);
            var procedureName = "ApiReader.AddFile";
            var parameters = new DynamicParameters();
            parameters.Add("@FileTypeId", fileType.FileTypeId);
            parameters.Add("@FilePath", filePath);

            var result = await _dataService.ExecuteStoredProcedure(procedureName, parameters);
            return result;
        }

        public async Task InstallData(Message queueItem)
        {
            bool moreFilesToInstall = true;            
            var procedureName = "Import.InstallNextFile";
            var parameters = new DynamicParameters();
            parameters.Add("@FileTypeId", queueItem.FileTypeId);
            parameters.Add("@MoreFilesToInstall", dbType: System.Data.DbType.Boolean, direction: System.Data.ParameterDirection.Output);

            while (moreFilesToInstall)
            {
                _logger.LogInformation($"Installing file into database", parameters);
                var result = await _dataService.ExecuteStoredProcedure<NextFile>(procedureName, parameters);
                moreFilesToInstall = result.FirstOrDefault().MoreFilesToInstall;
                _logger.LogInformation($"More files to install? {moreFilesToInstall}");
            };
        }
    }
}
