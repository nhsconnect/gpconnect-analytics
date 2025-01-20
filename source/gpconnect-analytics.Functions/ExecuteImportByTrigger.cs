using Core.DTOs.Response.Queue;
using function_app.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace function_app.Functions
{
    public class ExecuteImportByTrigger(IImportService importService)
    {
        [Function("ExecuteImportByTrigger")]
        public async Task Run([QueueTrigger("%QueueName%")] Message queueItem, ILogger log)
        {
            await importService.InstallData(queueItem);
        }
    }
}