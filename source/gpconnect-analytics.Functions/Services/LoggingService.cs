using Core.Services.Interfaces;
using function_app.Services.Interfaces;

namespace function_app.Services
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