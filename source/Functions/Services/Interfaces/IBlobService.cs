using Azure.Storage.Blobs.Models;
using Core.DTOs.Response.Splunk;

namespace Functions.Services.Interfaces
{
    public interface IBlobService
    {
        Task AddMessageToBlobQueue(int fileAddedCount, int fileTypeId, string blobName, bool overrideEntry = false);
        Task<BlobContentInfo?> AddObjectToBlob(ExtractResponse extractResponse);
    }
}