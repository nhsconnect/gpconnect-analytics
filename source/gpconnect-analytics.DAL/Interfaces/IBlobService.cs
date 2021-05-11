using System.Threading.Tasks;

namespace gpconnect_analytics.DAL.Interfaces
{
    public interface IBlobService
    {
        Task AddMessageToBlobQueue(int fileAddedCount, int fileTypeId);
    }
}
