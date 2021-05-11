using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;

namespace gpconnect_analytics.Configuration.Infrastructure.HttpClient
{
    public static class HttpClientExtensions
    {
        public static System.Net.Http.HttpClient ConfigureHttpClient(System.Net.Http.HttpClient options)
        {
            options.Timeout = new TimeSpan(0, 0, 0, 30);
            options.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/csv"));
            options.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            return options;
        }

        public static HttpMessageHandler CreateHttpMessageHandler()
        {
            var httpClientHandler = new HttpClientHandler
            {
                SslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls
            };
            return httpClientHandler;
        }
    }
}
