using System;

namespace gpconnect_analytics.DTO.Request
{
    public class UriRequest
    {
        public Uri Request { get; set; }
        public DateTime EarliestDate { get; set; }
        public DateTime LatestDate { get; set; }
        public TimeSpan Hour { get; set; }
    }
}
