using System.IO;
using System.Net.Http;

namespace gpconnect_analytics.DTO.Response.Splunk
{
    public class ExtractResponse
    {
        public HttpResponseMessage ExtractResponseMessage { get; set; }
        public Stream ExtractResponseStream { get; set; }
        public Extract ExtractRequestDetails { get; set; }
        public string FilePath { get; set; }
    }
}
