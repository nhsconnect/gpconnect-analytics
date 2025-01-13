using Core.DTOs;
using Core.DTOs.Response.Configuration;
using Core.Helpers;

namespace Core.Services.Interfaces
{
    public interface IConfigurationService
    {
        Task<BlobStorage> GetBlobStorageConfiguration();
        Task<FilePathConstants> GetFilePathConstants();
        Task<List<FileType>> GetFileTypes();
        Task<FileType> GetFileType(FileTypes fileType);
        Task<SplunkClient> GetSplunkClientConfiguration();
        Task<SplunkInstance> GetSplunkInstance(SplunkInstances splunkInstance);
    }
}