using gpconnect_analytics.Configuration.Infrastructure.Logging.Interface;
using NLog;

namespace gpconnect_analytics.Configuration
{
    public class LoggerManager : ILoggerManager
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public void LogDebug(string message)
        {
            logger.Debug(message);
        }
        public void LogError(string message)
        {
            logger.Error(message);
        }
        public void LogInformation(string message)
        {
            logger.Info(message);
        }
        public void LogWarning(string message)
        {
            logger.Warn(message);
        }
    }
}
