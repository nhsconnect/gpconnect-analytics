using gpconnect_analytics.DTO.Response.Queue;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL.Interfaces
{
    public interface IImportService
    {
        Task InstallData(Message message);
        Task<int> AddFile(string filePath);
    }
}
