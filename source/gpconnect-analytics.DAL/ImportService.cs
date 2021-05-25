using Dapper;
using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.DTO.Response.Import;
using gpconnect_analytics.DTO.Response.Queue;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL
{
    public class ImportService : IImportService
    {
        private readonly ILogger<ImportService> _logger;
        private readonly IDataService _dataService;

        public ImportService(IDataService dataService, ILogger<ImportService> logger)
        {
            _logger = logger;
            _dataService = dataService;
        }

        public async Task<int> AddFile(int fileTypeId, string filePath)
        {
            var procedureName = "ApiReader.AddFile";
            var parameters = new DynamicParameters();
            parameters.Add("@FileTypeId", fileTypeId);
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
