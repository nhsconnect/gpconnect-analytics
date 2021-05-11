using gpconnect_analytics.DTO.Response.Configuration;
using Dapper.FluentMap.Mapping;

namespace gpconnect_analytics.DAL.Mapping
{
    public class SplunkInstanceMap : EntityMap<SplunkInstance>
    {
        public SplunkInstanceMap()
        {
            Map(p => p.Source).ToColumn("SplunkInstance");
            Map(p => p.SourceGroup).ToColumn("SplunkInstanceGroup");
        }
    }
}
