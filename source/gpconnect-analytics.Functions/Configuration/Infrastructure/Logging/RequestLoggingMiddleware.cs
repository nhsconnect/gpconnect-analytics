using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace gpconnect_appointment_checker.Configuration
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            finally
            {                
            }
        }
    }

}
