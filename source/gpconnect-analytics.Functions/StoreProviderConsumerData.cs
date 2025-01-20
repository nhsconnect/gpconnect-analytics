using System.Net;
using System.Text.Json;
using Core.DTOs.Request;
using Core.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace function_app.Functions
{
    public class StoreProviderConsumerData(
        IHierarchyProviderConsumerRepo repository,
        ILogger<StoreProviderConsumerData> logger)
    {
        [Function("StoreProviderConsumerData")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "StoreProviderConsumerData")]
            HttpRequestData req)
        {
            logger.LogInformation("Processing HTTP request.");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (requestBody.Length == 0)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync("Request body length is 0");
                return response;
            }

            List<OrganisationHierarchyProvider> records = null;
            try
            {
                records = JsonSerializer.Deserialize<List<OrganisationHierarchyProvider>>(requestBody) ?? throw new
                    InvalidOperationException("Unable to deserialize input");
            }
            catch (JsonException ex)
            {
                logger.LogError($"Failed to deserialize request body: {ex.Message}");
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync("Invalid json input");
                return response;
            }


            try
            {
                logger.LogInformation($"Attempting to save {records.Count} items into databases");
                await repository.InsertHierarchyProviderConsumers(records);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync("Storing of items successful");
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to save data to databases: {ex.Message}");

                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync("Failed to save to the database - see logs for more information");
                return response;
            }
        }
    }
}