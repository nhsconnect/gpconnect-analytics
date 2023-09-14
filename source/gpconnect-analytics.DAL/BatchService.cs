using Dapper;
using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.DTO.Request;
using gpconnect_analytics.DTO.Response.Configuration;
using gpconnect_analytics.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace gpconnect_analytics.DAL
{
    public class BatchService : IBatchService
    {
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<BatchService> _logger;
        private SplunkClient _splunkClient;
        private readonly IDataService _dataService;
        private readonly ISplunkService _splunkService;
        private readonly IImportService _importService;

        public BatchService(IConfigurationService configurationService, IImportService importService, ISplunkService splunkService, ILogger<BatchService> logger, IDataService dataService)
        {
            _configurationService = configurationService;
            _logger = logger;
            _dataService = dataService;
            _splunkService = splunkService;
            _importService = importService;
        }

        public async Task<IActionResult> StartBatchDownloadForTodayAsync(FileTypes fileTypes)
        {
            var dateInScope = DateTime.Today.AddDays(1);
            var fileType = await _configurationService.GetFileType(fileTypes);
            var uriList = await GetBatchDownloadUriList(fileType, DateTimeHelper.EachDay(dateInScope, dateInScope).ToList());

            await RemovePreviousDownloads(fileType, dateInScope, dateInScope);
            return await ProcessUrls(fileType, uriList, true);
        }

        public async Task<IActionResult> StartBatchDownloadAsync(HttpRequest req, FileTypes fileTypes)
        {
            if (req != null)
            {
                var startDate = DateTime.TryParse(req.Query["StartDate"].ToString(), out DateTime start) ? start : DateTime.Today;
                var endDate = DateTime.TryParse(req.Query["EndDate"].ToString(), out DateTime end) ? end : DateTime.Today;

                if (endDate >= startDate)
                {
                    var fileType = await _configurationService.GetFileType(fileTypes);
                    var uriList = await GetBatchDownloadUriList(fileType, DateTimeHelper.EachDay(startDate, endDate).ToList());

                    await RemovePreviousDownloads(fileType, startDate, endDate);

                    return await ProcessUrls(fileType, uriList, false);
                }
            }
            return new BadRequestObjectResult("Bad request");
        }

        private async Task<IActionResult> ProcessUrls(FileType fileType, List<UriRequest> uriList, bool isToday)
        {
            for (var i = 0; i < uriList.Count; i++)
            {
                var downloadTasksQuery =
                from requestUri in uriList.Skip(i).Take(1)
                select ExecuteBatchDownloadFromSplunk(fileType, requestUri, isToday);

                var downloadTasks = downloadTasksQuery.ToList();

                while (downloadTasks.Any())
                {
                    Task finishedTask = await Task.WhenAny(downloadTasks);
                    downloadTasks.Remove(finishedTask);
                }
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
            return new OkObjectResult($"Batch download complete: {uriList.Count} requests processed");
        }

        private async Task ExecuteBatchDownloadFromSplunk(FileType fileType, UriRequest uriRequest, bool isToday)
        {
            try
            {
                if (FileTypeEnabled(fileType))
                {
                    var extractResponse = await _splunkService.DownloadCSVDateRangeAsync(fileType, uriRequest, isToday);
                    await _importService.AddObjectFileMessage(fileType, extractResponse);
                }
                else
                {
                    _logger?.LogWarning($"Filetype {fileType.FileTypeFilePrefix} is not enabled. Please check if this is correct");
                }
            }
            catch (Exception exc)
            {
                _logger?.LogError(exc, $"An error has occurred while attempting to execute an Azure function");
                throw;
            }
        }

        public async Task<List<UriRequest>> GetBatchDownloadUriList(FileType fileType, List<DateTime> dateTimeList)
        {
            var uriList = new List<UriRequest>();
            _splunkClient = await _configurationService.GetSplunkClientConfiguration();

            foreach (var dateTime in dateTimeList)
            {
                var earliestDate = dateTime.AddDays(-2);
                var latestDate = dateTime.AddDays(-1);

                for (var i = 0; i < 24; i++)
                {
                    var splunkQuery = fileType.SplunkQuery;
                    var hour = TimeSpan.Zero.Add(TimeSpan.FromHours(i));

                    splunkQuery = splunkQuery.Replace("{earliest}", earliestDate.ToString(Helpers.DateFormatConstants.SplunkQueryDate));
                    splunkQuery = splunkQuery.Replace("{latest}", latestDate.ToString(Helpers.DateFormatConstants.SplunkQueryDate));
                    splunkQuery = splunkQuery.Replace("{hour}", hour.ToString(Helpers.DateFormatConstants.SplunkQueryHour));

                    var uriBuilder = new UriBuilder
                    {
                        Scheme = Uri.UriSchemeHttps,
                        Host = _splunkClient.HostName,
                        Port = _splunkClient.HostPort,
                        Path = _splunkClient.BaseUrl,
                        Query = string.Format(_splunkClient.QueryParameters, HttpUtility.UrlEncode(splunkQuery))
                    };

                    uriList.Add(new UriRequest()
                    {
                        Request = uriBuilder.Uri,
                        EarliestDate = earliestDate,
                        LatestDate = latestDate,
                        Hour = hour
                    });
                }
            }
            return uriList;
        }

        public async Task RemovePreviousDownloads(FileType fileType, DateTime startDate, DateTime endDate)
        {
            var procedureName = "Import.RemovePreviousDownload";
            var parameters = new DynamicParameters();
            parameters.Add("@FileTypeId", fileType.FileTypeId);
            parameters.Add("@StartDate", startDate.AddDays(-2));
            parameters.Add("@EndDate", endDate.AddDays(-1));
            await _dataService.ExecuteStoredProcedure(procedureName, parameters);        
        }

        private bool FileTypeEnabled(FileType fileType)
        {
            return (fileType != null && fileType.Enabled);
        }
    }
}
