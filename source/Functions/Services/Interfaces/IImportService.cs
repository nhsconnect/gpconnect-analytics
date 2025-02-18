using Core.DTOs.Response.Configuration;
using Core.DTOs.Response.Queue;
using Core.DTOs.Response.Splunk;

namespace Functions.Services.Interfaces
{
    public interface IImportService
    {
        Task InstallData(Message message);
        Task AddDownloadedFileManually(string filePath);
        Task AddObjectFileMessage(FileType fileType, ExtractResponse extractResponse);
    }
}