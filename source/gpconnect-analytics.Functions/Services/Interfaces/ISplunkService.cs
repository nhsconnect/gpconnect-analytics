using Core.DTOs.Request;
using Core.DTOs.Response.Configuration;
using Core.DTOs.Response.Splunk;

namespace function_app.Services.Interfaces
{
    public interface ISplunkService
    {
        Task<ExtractResponse> DownloadCSVDateRangeAsync(FileType fileType, UriRequest uriRequest, bool isToday);
    }
}