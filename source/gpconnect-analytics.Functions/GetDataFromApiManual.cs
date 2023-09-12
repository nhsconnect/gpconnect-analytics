using gpconnect_analytics.DAL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace gpconnect_analytics.Functions
{
    public class GetDataFromApiManual
    {
        private readonly IImportService _importService;

        public GetDataFromApiManual(IImportService importService)
        {
            _importService = importService;
        }

        [FunctionName("GetDataFromApiManual")]
        public async Task<IActionResult> AddDownloadedFile([HttpTrigger(AuthorizationLevel.Function, "GET", Route = null)] HttpRequest req, ILogger log)
        {
            return await _importService.AddDownloadedFileManually(req);
        }
    }
}
