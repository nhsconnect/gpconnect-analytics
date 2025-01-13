using System.Net;
using function_app.Services.Interfaces;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace function_app.Functions
{
    public class GetDataFromApiManual(IImportService importService)
    {
        [Function("GetDataFromApiManual")]
        public async Task<HttpResponseData> AddDownloadedFile(
            [HttpTrigger(AuthorizationLevel.Function, "GET", Route = null)]
            HttpRequestData req, ILogger log)
        {
            var response = req.CreateResponse();
            response.Headers.Add("Content-Type", "application/text");
            try
            {
                var filePath = req.Query["FilePath"];
                if (string.IsNullOrEmpty(filePath))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    await response.WriteStringAsync("Filepath missing");
                    return response;
                }

                await importService.AddDownloadedFileManually(filePath);
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync("Successfully added files");
                return response;
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Error adding downloaded file: {ex.Message}");
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync("Something went wrong - see error logs for more deails");
                return response;
            }
        }
    }
}