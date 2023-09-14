using System.Threading.Tasks;

namespace gpconnect_analytics.DAL.Interfaces
{
    public interface ILoggingService
    {
        Task PurgeErrorLog();
    }
}
