using gpconnect_analytics.DTO.Response.Splunk;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL.Interfaces
{
    public interface ISplunkService
    {
        Task<ExtractResponse> DownloadCSV(int fileTypeId);
    }
}
