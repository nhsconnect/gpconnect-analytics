using System.Net;
using Core.Helpers;
using Functions.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Functions
{
    public class GetDataFromApiByDateRange(IBatchService batchService, ILogger log)
    {
        [Function("GetDataFromApiByDateRangeSspTrans")]
        public async Task<HttpResponseData> GetDataFromSspTransByDateRange(
            [HttpTrigger(AuthorizationLevel.Function, "GET", Route = null)]
            HttpRequestData req)
        {
            try
            {
                var startDate = req.Query["StartDate"];
                var endDate = req.Query["EndDate"];
                var rows = await batchService.StartBatchDownloadAsync(FileTypes.ssptrans, startDate, endDate);


                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync($"Batch Download successful: {rows} requests processed");
                return response;
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error starting batch download for SSP Trans");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("Failed to download - see logs");
                return response;
            }
        }

        [Function("GetDataFromApiByDateRangeMeshTrans")]
        public async Task<HttpResponseData> GetDataFromMeshTransByDateRange(
            [HttpTrigger(AuthorizationLevel.Function, "GET", Route = null)]
            HttpRequestData req)
        {
            try
            {
                var startDate = req.Query["StartDate"];
                var endDate = req.Query["EndDate"];
                var processed = await batchService.StartBatchDownloadAsync(FileTypes.meshtrans, startDate, endDate);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync($"Batch Download successful: {processed} requests processed");
                return response;
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error starting batch download for Mesh Trans");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("Failed to download - see logs");
                return response;
            }
        }

        [Function("GetDataFromApiByDateRangeAsidLookup")]
        public async Task<HttpResponseData> GetDataFromAsidLookupByDateRange(
            [HttpTrigger(AuthorizationLevel.Function, "GET", Route = null)]
            HttpRequestData req)
        {
            try
            {
                var startDate = req.Query["StartDate"];
                var endDate = req.Query["EndDate"];
                var processed = await batchService.StartBatchDownloadAsync(FileTypes.asidlookup, startDate, endDate);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync($"Batch Download successful: {processed} requests processed");
                return response;
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error starting batch download for Asid Lookup");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("Failed to download - see logs");
                return response;
            }
        }
    }
}