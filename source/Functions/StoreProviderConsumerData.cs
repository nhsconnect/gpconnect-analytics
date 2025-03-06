using System.Net;
using System.Text.Json;
using Core.DTOs.Request;
using Core.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Functions
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
                var errorLengthResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                errorLengthResponse.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await errorLengthResponse.WriteStringAsync("Request body length is 0");
                return errorLengthResponse;
            }

            List<OrganisationHierarchyProvider> records = null;
            try
            {
                records = JsonSerializer.Deserialize<List<OrganisationHierarchyProvider>>(requestBody) ?? throw new
                    InvalidOperationException("Unable to deserialize input");
            }
            catch (JsonException ex)
            {
                logger.LogError("Failed to deserialize request body: {exceptionMessage}", ex.Message);
                var serializeErrorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                serializeErrorResponse.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await serializeErrorResponse.WriteStringAsync("Invalid json input");
                return serializeErrorResponse;
            }


            logger.LogInformation($"Attempting to save {records.Count} items into databases");
            var count = await repository.InsertHierarchyProviderConsumers(records);

            if (count > 0)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.OK);
                errorResponse.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await errorResponse.WriteStringAsync("Storing of items successful");
                return errorResponse;
            }

            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            logger.LogInformation($"No items of {records.Count} were saved to the database");

            await response.WriteStringAsync("Failed to save to the database - see logs for more information");
            return response;
        }
    }
}