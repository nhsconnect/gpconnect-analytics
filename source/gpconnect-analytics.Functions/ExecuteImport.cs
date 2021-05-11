using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.DTO.Response.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace gpconnect_analytics.Functions
{
    public class ExecuteImport
    {
        private readonly ILogger<ExecuteImport> _logger;
        private readonly IImportService _importService;

        public ExecuteImport(ILogger<ExecuteImport> logger, IImportService importService)
        {
            _logger = logger;
            _importService = importService;
        }

        [FunctionName("ExecuteImport")]
        public async Task Run([QueueTrigger("%QueueName%")] Message queueItem, ILogger log)
        {
            await _importService.InstallData(queueItem);
        }
    }
}
