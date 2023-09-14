using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.DTO.Response.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace gpconnect_analytics.Functions
{
    public class ExecuteImportByTrigger
    {
        private readonly IImportService _importService;

        public ExecuteImportByTrigger(IImportService importService)
        {
            _importService = importService;
        }

        [FunctionName("ExecuteImportByTrigger")]
        public async Task Run([QueueTrigger("%QueueName%")] Message queueItem, ILogger log)
        {
            await _importService.InstallData(queueItem);
        }
    }
}
