using Functions.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;

namespace Functions
{
    public class PurgeErrorLogByTrigger(ILoggingService loggingService)
    {
        [Function("PurgeErrorLogByTrigger")]
        public async Task PurgeErrorLog(
            [TimerTrigger("%PurgeErrorLogByTriggerSchedule%", RunOnStartup = false)]
            TimerInfo myTimer)
        {
            await loggingService.PurgeErrorLog();
        }
    }
}