using System.Net;
using Core.Helpers;
using Functions.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Functions
{
    public class GetDataFromApiToday(IBatchService batchService, ILogger log)
    {
        [Function("GetDataFromApiTodaySspTrans")]
        public async Task<HttpResponseData> GetDataFromSspTransByDateRange(
            [HttpTrigger(AuthorizationLevel.Function, "GET", Route = null)]
            HttpRequestData req)
        {
            var response = req.CreateResponse();
            response.Headers.Add("Content-Type", "application/text");


            var affectedCount = 0;
            try
            {
                affectedCount = await batchService.StartBatchDownloadForTodayAsync(FileTypes.ssptrans);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred during batch download when processing urls");
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync(
                    $"An error occurred whilst processing batch download - see logs for more details");

                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            await response.WriteStringAsync($"Processed {affectedCount} requests");
            return response;
        }

        [Function("GetDataFromApiTodayMeshTrans")]
        public async Task<HttpResponseData> GetDataFromMeshTransByDateRange(
            [HttpTrigger(AuthorizationLevel.Function, "GET", Route = null)]
            HttpRequestData req)
        {
            var response = req.CreateResponse();
            response.Headers.Add("Content-Type", "application/text");


            var affectedCount = 0;
            try
            {
                affectedCount = await batchService.StartBatchDownloadForTodayAsync(FileTypes.meshtrans);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred during batch download when processing urls");
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync(
                    $"An error occurred whilst processing batch download - see logs for more details");

                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            await response.WriteStringAsync($"Processed {affectedCount} requests");
            return response;
        }

        [Function("GetDataFromApiTodayAsidLookup")]
        public async Task<HttpResponseData> GetDataFromAsidLookupByDateRange(
            [HttpTrigger(AuthorizationLevel.Function, "GET", Route = null)]
            HttpRequestData req)
        {
            var response = req.CreateResponse();
            response.Headers.Add("Content-Type", "application/text");
            var affectedCount = 0;
            try
            {
                affectedCount = await batchService.StartBatchDownloadForTodayAsync(FileTypes.asidlookup);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred during batch download when processing urls");
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync(
                    $"An error occurred whilst processing batch download - see logs for more details");


                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            await response.WriteStringAsync($"Processed {affectedCount} requests");
            return response;
        }
    }
}