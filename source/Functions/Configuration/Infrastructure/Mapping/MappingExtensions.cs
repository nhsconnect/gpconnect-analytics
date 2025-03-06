using Core.Mapping;
using Dapper.FluentMap;

namespace Functions.Configuration.Infrastructure.Mapping
{
    public static class MappingExtensions
    {
        public static void ConfigureMappingServices()
        {
            FluentMapper.Initialize(config => { config.AddMap(new SplunkInstanceMap()); });
        }
    }
}