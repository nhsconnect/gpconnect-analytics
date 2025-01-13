using Core.DTOs.Response.Configuration;
using Dapper.FluentMap.Mapping;

namespace Core.Mapping
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