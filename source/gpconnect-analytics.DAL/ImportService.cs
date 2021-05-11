using Dapper;
using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.DTO.Response.Configuration;
using gpconnect_analytics.DTO.Response.Import;
using gpconnect_analytics.DTO.Response.Queue;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL
{
    public class ImportService : IImportService
    {
        private readonly IDataService _dataService;
        private readonly IConfigurationService _configurationService;
        private readonly List<FileType> _fileTypes;

        public ImportService(IDataService dataService, IConfigurationService configurationService)
        {
            _dataService = dataService;
            _configurationService = configurationService;
            _fileTypes = _configurationService.GetFileTypes().Result;
        }

        public async Task<int> AddFile(string filePath)
        {
            var directoryName = filePath.Split(new char[] { '\\' })[0];
            var fileTypeId = _fileTypes.FirstOrDefault(x => x.DirectoryName == directoryName).FileTypeId;

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
            parameters.Add("@MoreFilesToInstall", dbType: System.Data.DbType.Boolean, direction: System.Data.ParameterDirection.Output);

            while (moreFilesToInstall)
            {
                var result = await _dataService.ExecuteStoredProcedure<NextFile>(procedureName, parameters);
                moreFilesToInstall = result.FirstOrDefault().MoreFilesToInstall;
            };
        }
    }
}
