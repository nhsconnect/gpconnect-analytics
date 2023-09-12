using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace gpconnect_analytics.Functions
{
    public class GetDataFromApiToday
    {
        private readonly IBatchService _batchService;

        public GetDataFromApiToday(IBatchService batchService)
        {
            _batchService = batchService;
        }

        [FunctionName("GetDataFromApiTodaySspTrans")]
        public async Task<IActionResult> GetDataFromSspTransByDateRange([HttpTrigger(AuthorizationLevel.Function, "GET", Route = null)] HttpRequest req, ILogger log)
        {
            return await _batchService.StartBatchDownloadForTodayAsync(FileTypes.ssptrans);
        }

        [FunctionName("GetDataFromApiTodayMeshTrans")]
        public async Task<IActionResult> GetDataFromMeshTransByDateRange([HttpTrigger(AuthorizationLevel.Function, "GET", Route = null)] HttpRequest req, ILogger log)
        {
            return await _batchService.StartBatchDownloadForTodayAsync(FileTypes.meshtrans);
        }

        [FunctionName("GetDataFromApiTodayAsidLookup")]
        public async Task<IActionResult> GetDataFromAsidLookupByDateRange([HttpTrigger(AuthorizationLevel.Function, "GET", Route = null)] HttpRequest req, ILogger log)
        {
            return await _batchService.StartBatchDownloadForTodayAsync(FileTypes.asidlookup);
        }
    }
}
