using Core.Helpers;
using function_app.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace function_app.Functions
{
    public class GetDataFromApiByTrigger(IBatchService batchService)
    {
        [Function("GetDataFromApiByTriggerAsidLookup")]
        public async Task GetDataFromAsidLookup(
            [TimerTrigger("%GetDataFromApiByTriggerAsidLookupSchedule%", RunOnStartup = false)]
            TimerInfo myTimer,
            ILogger log,
            FunctionContext context)
        {
            
            await batchService.StartBatchDownloadForTodayAsync(FileTypes.asidlookup);
        }

        [Function("GetDataFromApiByTriggerSspTrans")]
        public async Task GetDataFromSspTrans(
            [TimerTrigger("%GetDataFromApiByTriggerSspTransSchedule%", RunOnStartup = false)]
            TimerInfo myTimer,
            ILogger log)
        {
            await batchService.StartBatchDownloadForTodayAsync(FileTypes.ssptrans);
        }

        [Function("GetDataFromApiByTriggerMeshTrans")]
        public async Task GetDataFromMeshTrans(
            [TimerTrigger("%GetDataFromApiByTriggerMeshTransSchedule%", RunOnStartup = false)]
            TimerInfo myTimer,
            ILogger log)
        {
            await batchService.StartBatchDownloadForTodayAsync(FileTypes.meshtrans);
        }
    }
}