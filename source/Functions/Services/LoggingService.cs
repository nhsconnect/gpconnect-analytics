using Core.Services.Interfaces;
using Functions.Services.Interfaces;

namespace Functions.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly IDataService _dataService;

        public LoggingService(IDataService dataService)
        {
            _dataService = dataService;
        }

        public async Task PurgeErrorLog()
        {
            var procedureName = "Logging.PurgeErrorLog";
            await _dataService.ExecuteStoredProcedure(procedureName);
        }
    }
}