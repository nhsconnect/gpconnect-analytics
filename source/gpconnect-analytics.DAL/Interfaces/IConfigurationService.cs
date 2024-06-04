using gpconnect_analytics.DTO.Response.Configuration;
using gpconnect_analytics.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL.Interfaces
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
