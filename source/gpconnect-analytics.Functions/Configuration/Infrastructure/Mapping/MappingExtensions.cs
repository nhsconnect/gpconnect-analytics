using Dapper.FluentMap;
using gpconnect_analytics.DAL.Mapping;

namespace gpconnect_analytics.Configuration.Infrastructure
{
    public static class MappingExtensions
    {
        public static void ConfigureMappingServices()
        {
            FluentMapper.Initialize(config =>
            {
                config.AddMap(new SplunkInstanceMap());
            });
        }
    }
}
