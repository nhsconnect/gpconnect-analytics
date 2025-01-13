using Core.DTOs.Request;
using Core.DTOs.Response.Configuration;
using Core.Helpers;
using Microsoft.Azure.Functions.Worker.Http;

namespace function_app.Services.Interfaces
{
    public interface IBatchService
    {
        Task<List<UriRequest>> GetBatchDownloadUriList(FileType fileType, List<DateTime> dateTimeList);
        Task RemovePreviousDownloads(FileType fileType, DateTime startDate, DateTime endDate);
        Task<int> StartBatchDownloadForTodayAsync(FileTypes fileTypes);
        Task<int> StartBatchDownloadAsync(FileTypes fileTypes, string? startDate, string? endDate);
    }
}