using System.Web;
using Core.DTOs.Request;
using Core.DTOs.Response.Configuration;
using Core.Helpers;
using Core.Services.Interfaces;
using Dapper;
using Functions.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Functions.Services
{
    public class BatchService(
        IConfigurationService configurationService,
        ISplunkService splunkService,
        ILogger<BatchService> logger,
        IDataService dataService)
        : IBatchService
    {
        private SplunkClient _splunkClient;

        public async Task<int> StartBatchDownloadForTodayAsync(FileTypes fileTypes)
        {
            var dateInScope = DateTime.Today.AddDays(1);
            var fileType = await configurationService.GetFileType(fileTypes);
            var uriList =
                await GetBatchDownloadUriList(fileType, DateTimeHelper.EachDay(dateInScope, dateInScope).ToList());

            await RemovePreviousDownloads(fileType, dateInScope, dateInScope);

            await ProcessUrls(fileType, uriList, true);
            return uriList.Count;
        }

        public async Task<int> StartBatchDownloadAsync(FileTypes fileTypes, string? startDate, string? endDate)
        {
            if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
            {
                logger.LogError("Start and end dates are required for batch download");
                throw new ArgumentException("Start and end dates are required for batch download");
            }

            var start = DateTime.TryParse(startDate, out DateTime parsedStart)
                ? parsedStart
                : DateTime.Today;
            var end = DateTime.TryParse(endDate, out DateTime parsedEnd)
                ? parsedEnd
                : DateTime.Today;

            if (parsedEnd >= parsedStart)
            {
                var fileType = await configurationService.GetFileType(fileTypes);
                var uriList =
                    await GetBatchDownloadUriList(fileType, DateTimeHelper.EachDay(start, end).ToList());

                await RemovePreviousDownloads(fileType, start, end);

                try
                {
                    await ProcessUrls(fileType, uriList, false);
                    return uriList.Count;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred during batch download when processing urls");
                    throw;
                }
            }

            logger.LogError("Start date cannot be later than end date");
            throw new ArgumentException("Start date cannot be later than end date");
        }

        private async Task ProcessUrls(FileType fileType, List<UriRequest> uriList, bool isToday)
        {
            var downloadTasks = new List<Task>();

            // Create and start all download tasks
            for (var i = 0; i < uriList.Count; i++)
            {
                var requestUri = uriList[i];
                downloadTasks.Add(splunkService.ExecuteBatchDownloadFromSplunk(fileType, requestUri, isToday));
            }

            // Wait for tasks to complete
            while (downloadTasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(downloadTasks);
                downloadTasks.Remove(finishedTask);
            }
        }

        public async Task<List<UriRequest>> GetBatchDownloadUriList(FileType fileType, List<DateTime> dateTimeList)
        {
            var uriList = new List<UriRequest>();
            _splunkClient = await configurationService.GetSplunkClientConfiguration();

            foreach (var dateTime in dateTimeList)
            {
                var earliestDate = dateTime.AddDays(-2);
                var latestDate = dateTime.AddDays(-1);

                for (var i = 0; i < 24; i++)
                {
                    var splunkQuery = fileType.SplunkQuery;
                    var hour = TimeSpan.Zero.Add(TimeSpan.FromHours(i));

                    splunkQuery = splunkQuery.Replace("{earliest}",
                        earliestDate.ToString(DateFormatConstants.SplunkQueryDate));
                    splunkQuery = splunkQuery.Replace("{latest}",
                        latestDate.ToString(DateFormatConstants.SplunkQueryDate));
                    splunkQuery = splunkQuery.Replace("{hour}",
                        hour.ToString(DateFormatConstants.SplunkQueryHour));

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
            await dataService.ExecuteStoredProcedure(procedureName, parameters);
        }
    }
}