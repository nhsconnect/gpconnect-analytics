using Core.Services.Interfaces;
using Dapper;
using Functions.Services.Interfaces;

namespace Functions.Services;

public class FileService(IDataService dataService) : IFileService
{
    public async Task<int> ApiReaderAddFile(int fileTypeId, string filePath, bool overrideFile)
    {
        const string procedureName = "ApiReader.AddFile";
        var parameters = new DynamicParameters();
        parameters.Add("@FileTypeId", fileTypeId);
        parameters.Add("@FilePath", filePath);
        parameters.Add("@Override", overrideFile);
        var result = await dataService.ExecuteStoredProcedure(procedureName, parameters);
        return result;
    }
}