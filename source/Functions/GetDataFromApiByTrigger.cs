using Core.Helpers;
using Functions.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Functions
{
    public class GetDataFromApiByTrigger(IBatchService batchService, ILogger log)
    {
        [Function("GetDataFromApiByTriggerAsidLookup")]
        public async Task GetDataFromAsidLookup(
            [TimerTrigger("%GetDataFromApiByTriggerAsidLookupSchedule%", RunOnStartup = false)]
            TimerInfo myTimer)
        {
            await batchService.StartBatchDownloadForTodayAsync(FileTypes.asidlookup);
        }

        [Function("GetDataFromApiByTriggerSspTrans")]
        public async Task GetDataFromSspTrans(
            [TimerTrigger("%GetDataFromApiByTriggerSspTransSchedule%", RunOnStartup = false)]
            TimerInfo myTimer)
        {
            await batchService.StartBatchDownloadForTodayAsync(FileTypes.ssptrans);
        }

        [Function("GetDataFromApiByTriggerMeshTrans")]
        public async Task GetDataFromMeshTrans(
            [TimerTrigger("%GetDataFromApiByTriggerMeshTransSchedule%", RunOnStartup = false)]
            TimerInfo myTimer)
        {
            await batchService.StartBatchDownloadForTodayAsync(FileTypes.meshtrans);
        }
    }
}