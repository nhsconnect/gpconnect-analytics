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
    public class GetDataFromApiByDateRange
    {
        private readonly IBatchService _batchService;

        public GetDataFromApiByDateRange(IBatchService batchService)
        {
            _batchService = batchService;
        }

        [FunctionName("GetDataFromApiByDateRangeSspTrans")]
        public async Task<IActionResult> GetDataFromSspTransByDateRange([HttpTrigger(AuthorizationLevel.Function, "GET", Route = null)] HttpRequest req, ILogger log)
        {
            return await _batchService.StartBatchDownloadAsync(req, FileTypes.ssptrans);
        }

        [FunctionName("GetDataFromApiByDateRangeMeshTrans")]
        public async Task<IActionResult> GetDataFromMeshTransByDateRange([HttpTrigger(AuthorizationLevel.Function, "GET", Route = null)] HttpRequest req, ILogger log)
        {
            return await _batchService.StartBatchDownloadAsync(req, FileTypes.meshtrans);
        }

        [FunctionName("GetDataFromApiByDateRangeAsidLookup")]
        public async Task<IActionResult> GetDataFromAsidLookupByDateRange([HttpTrigger(AuthorizationLevel.Function, "GET", Route = null)] HttpRequest req, ILogger log)
        {
            return await _batchService.StartBatchDownloadAsync(req, FileTypes.asidlookup);
        }
    }
}
