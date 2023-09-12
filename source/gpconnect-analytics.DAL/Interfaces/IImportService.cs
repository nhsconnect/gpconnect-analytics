using gpconnect_analytics.DTO.Response.Configuration;
using gpconnect_analytics.DTO.Response.Queue;
using gpconnect_analytics.DTO.Response.Splunk;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL.Interfaces
{
    public interface IImportService
    {
        Task InstallData(Message message);
        Task<int> AddFile(int fileTypeId, string filePath, bool overrideFile);
        Task<IActionResult> AddDownloadedFileManually(HttpRequest req);
        Task AddObjectFileMessage(FileType fileType, ExtractResponse extractResponse);
    }
}
