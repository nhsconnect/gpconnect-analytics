using Azure.Storage.Blobs.Models;
using gpconnect_analytics.DTO.Response.Splunk;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL.Interfaces
{
    public interface IBlobService
    {
        Task AddMessageToBlobQueue(int fileAddedCount, int fileTypeId, string blobName);
        Task<BlobContentInfo> AddObjectToBlob(ExtractResponse extractResponse);
    }
}
