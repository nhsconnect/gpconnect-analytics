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
        Task<FileType> GetFileType(FileTypes fileTypes);
        Task<SplunkClient> GetSplunkClientConfiguration();
        Task<SplunkInstance> GetSplunkInstance(Helpers.SplunkInstances splunkInstance);
    }
}
