using Core.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Functions.Services
{
    public class CoreConfigurationService(IConfiguration configuration) : ICoreConfigurationService
    {
        public string GetConnectionString(string name)
        {
            var connectionString = configuration.GetConnectionString(name);
            return connectionString ?? throw new ArgumentException("No connection string with given name");
        }
    }
}