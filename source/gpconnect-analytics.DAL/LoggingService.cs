using gpconnect_analytics.DAL.Interfaces;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL
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
