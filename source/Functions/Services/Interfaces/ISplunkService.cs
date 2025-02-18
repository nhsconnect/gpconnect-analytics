using Core.DTOs.Request;
using Core.DTOs.Response.Configuration;
using Core.DTOs.Response.Splunk;

namespace Functions.Services.Interfaces
{
    public interface ISplunkService
    {
        Task<ExtractResponse> DownloadCSVDateRangeAsync(FileType fileType, UriRequest uriRequest, bool isToday);
        Task ExecuteBatchDownloadFromSplunk(FileType fileType, UriRequest uriRequest, bool isToday);
    }
}