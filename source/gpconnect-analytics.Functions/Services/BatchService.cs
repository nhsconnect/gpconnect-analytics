using System.Net;
using System.Web;
using Core.DTOs.Request;
using Core.DTOs.Response.Configuration;
using Core.Helpers;
using Core.Services.Interfaces;
using Dapper;
using function_app.Services.Interfaces;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace function_app.Services
{
    public class BatchService(
        IConfigurationService configurationService,
        IImportService importService,
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
            else
            {
                logger.LogError("Start date cannot be later than end date");
                throw new ArgumentException("Start date cannot be later than end date");
            }
        }

        private async Task ProcessUrls(FileType fileType,
            List<UriRequest> uriList, bool isToday)
        {
            for (var i = 0; i < uriList.Count; i++)
            {
                var downloadTasksQuery =
                    from requestUri in uriList.Skip(i).Take(1)
                    select ExecuteBatchDownloadFromSplunk(fileType, requestUri, isToday);

                var downloadTasks = downloadTasksQuery.ToList();

                while (downloadTasks.Count != 0)
                {
                    var finishedTask = await Task.WhenAny(downloadTasks);
                    downloadTasks.Remove(finishedTask);
                }

                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }

        private async Task ExecuteBatchDownloadFromSplunk(FileType fileType, UriRequest uriRequest, bool isToday)
        {
            try
            {
                if (FileTypeEnabled(fileType))
                {
                    var extractResponse = await splunkService.DownloadCSVDateRangeAsync(fileType, uriRequest, isToday);
                    await importService.AddObjectFileMessage(fileType, extractResponse);
                }
                else
                {
                    logger?.LogWarning(
                        $"Filetype {fileType.FileTypeFilePrefix} is not enabled. Please check if this is correct");
                }
            }
            catch (Exception exc)
            {
                logger?.LogError(exc, $"An error has occurred while attempting to execute an Azure function");
                throw;
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

        private bool FileTypeEnabled(FileType fileType)
        {
            return (fileType != null && fileType.Enabled);
        }
    }
}