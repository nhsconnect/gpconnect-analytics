using gpconnect_analytics.DAL.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace gpconnect_analytics.Functions
{
    public class PurgeErrorLogByTrigger
    {
        private readonly ILoggingService _loggingService;

        public PurgeErrorLogByTrigger(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        [FunctionName("PurgeErrorLogByTrigger")]
        public async Task PurgeErrorLog([TimerTrigger("%PurgeErrorLogByTriggerSchedule%", RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            await _loggingService.PurgeErrorLog();
        }        
    }
}
