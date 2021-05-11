using gpconnect_analytics.DTO.Response.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL.Interfaces
{
    public interface IConfigurationService
    {
        Task<BlobStorage> GetBlobStorageConfiguration();
        Task<Email> GetEmailConfiguration();
        Task<FilePathConstants> GetFilePathConstants();
        Task<List<FileType>> GetFileTypes();
        Task<SplunkClient> GetSplunkClientConfiguration();
        Task<List<SplunkInstance>> GetSplunkInstances();
    }
}
