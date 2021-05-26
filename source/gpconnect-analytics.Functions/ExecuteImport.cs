using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.DTO.Response.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace gpconnect_analytics.Functions
{
    public class ExecuteImport
    {
        private readonly ILogger<ExecuteImport> _logger;
        private readonly IImportService _importService;
        private readonly IEmailService _emailService;

        public ExecuteImport(ILogger<ExecuteImport> logger, IImportService importService, IEmailService emailService)
        {
            _logger = logger;
            _importService = importService;
            _emailService = emailService;
        }

        [FunctionName("ExecuteImport")]
        public async Task Run([QueueTrigger("%QueueName%")] Message queueItem, ILogger log)
        {
            try
            {
                await _importService.InstallData(queueItem);
            }
            catch (Exception exc)
            {
                await _emailService.SendProcessErrorEmail(exc);
                _logger?.LogError(exc, $"An error has occurred while attempting to execute an Azure function");
                throw;
            }

        }
    }
}
