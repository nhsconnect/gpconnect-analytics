using function_app.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace function_app.Functions
{
    public class PurgeErrorLogByTrigger(ILoggingService loggingService)
    {
        [Function("PurgeErrorLogByTrigger")]
        public async Task PurgeErrorLog(
            [TimerTrigger("%PurgeErrorLogByTriggerSchedule%", RunOnStartup = false)]
            TimerInfo myTimer, ILogger log)
        {
            await loggingService.PurgeErrorLog();
        }
    }
}