namespace gpconnect_analytics.DTO.Response.Configuration
{
    public class SplunkClient
    {
        public string SplunkInstance { get; set; }
        public string BaseUrl { get; set; }
        public string QueryParameters { get; set; }
        public string HostName { get; set; }
        public int HostPort { get; set; }
        public int QueryTimeout { get; set; }
        public string ApiToken { get; set; }
    }
}
