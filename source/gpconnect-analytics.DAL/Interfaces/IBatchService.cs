using gpconnect_analytics.DTO.Request;
using gpconnect_analytics.DTO.Response.Configuration;
using gpconnect_analytics.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL.Interfaces
{
    public interface IBatchService
    {
        Task<List<UriRequest>> GetBatchDownloadUriList(FileType fileType, List<DateTime> dateTimeList);
        Task RemovePreviousDownloads(FileType fileType, DateTime startDate, DateTime endDate);
        Task<IActionResult> StartBatchDownloadForTodayAsync(FileTypes fileTypes);
        Task<IActionResult> StartBatchDownloadAsync(HttpRequest req, FileTypes fileTypes);
    }
}
