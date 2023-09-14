using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace gpconnect_analytics.Functions
{
    public class GetDataFromApiByTrigger
    {
        private readonly IBatchService _batchService;

        public GetDataFromApiByTrigger(IBatchService batchService)
        {
            _batchService = batchService;
        }

        [FunctionName("GetDataFromApiByTriggerAsidLookup")]
        public async Task GetDataFromAsidLookup([TimerTrigger("%GetDataFromApiByTriggerAsidLookupSchedule%", RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            await _batchService.StartBatchDownloadForTodayAsync(FileTypes.asidlookup);
        }

        [FunctionName("GetDataFromApiByTriggerSspTrans")]
        public async Task GetDataFromSspTrans([TimerTrigger("%GetDataFromApiByTriggerSspTransSchedule%", RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            await _batchService.StartBatchDownloadForTodayAsync(FileTypes.ssptrans);
        }

        [FunctionName("GetDataFromApiByTriggerMeshTrans")]
        public async Task GetDataFromMeshTrans([TimerTrigger("%GetDataFromApiByTriggerMeshTransSchedule%", RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            await _batchService.StartBatchDownloadForTodayAsync(FileTypes.meshtrans);
        }        
    }
}
