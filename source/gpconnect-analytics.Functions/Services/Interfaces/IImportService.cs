using Core.DTOs.Response.Configuration;
using Core.DTOs.Response.Queue;
using Core.DTOs.Response.Splunk;
using Microsoft.Azure.Functions.Worker.Http;

namespace function_app.Services.Interfaces
{
    public interface IImportService
    {
        Task InstallData(Message message);
        Task<int> AddFile(int fileTypeId, string filePath, bool overrideFile);
        Task AddDownloadedFileManually(string filePath);
        Task AddObjectFileMessage(FileType fileType, ExtractResponse extractResponse);
    }
}