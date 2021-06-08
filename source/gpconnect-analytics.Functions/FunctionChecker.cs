using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;

namespace gpconnect_analytics.Functions
{
    public class FunctionChecker
    {
        [FunctionName("FunctionCheckerEveryMinute")]
        public void FunctionCheckerEveryMinute([TimerTrigger("0 */1 * * * *", RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Function with TimerTrigger ran at {DateTime.UtcNow}");
        }
    }
}
