using Dapper;
using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.DTO.Response.Configuration;
using gpconnect_analytics.DTO.Response.Splunk;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace gpconnect_analytics.DAL
{
    public class SplunkService : ISplunkService
    {
        private readonly IConfigurationService _configurationService;
        private readonly IDataService _dataService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SplunkService> _logger;
        private SplunkClient _splunkClient;
        private FilePathConstants _filePathConstants;
        private List<SplunkInstance> _splunkInstances;

        public SplunkService(IConfigurationService configurationService, IDataService dataService, IHttpClientFactory httpClientFactory, ILogger<SplunkService> logger)
        {
            _configurationService = configurationService;
            _dataService = dataService;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ExtractResponse> DownloadCSV(FileType fileType)
        {
            try
            {
                _filePathConstants = await _configurationService.GetFilePathConstants();
                _splunkClient = await _configurationService.GetSplunkClientConfiguration();
                _splunkInstances = await _configurationService.GetSplunkInstances();

                var splunkInstance = _splunkInstances.FirstOrDefault(x => x.Source == Helpers.SplunkInstances.cloud.ToString());

                var extractDetails = await GetNextExtractDetails(fileType.FileTypeId);
                var extractResponseMessage = new ExtractResponse();
                if (extractDetails != null)
                {
                    var client = _httpClientFactory.CreateClient("SplunkApiClient");
                    var requestUri = ConstructRequestUri(extractDetails);
                    var request = new HttpRequestMessage(HttpMethod.Get, requestUri.Uri);
                    var response = await client.SendAsync(request);

                    var filePath = ConstructFilePath(splunkInstance, fileType, extractDetails);
                    var responseStream = await response.Content.ReadAsStreamAsync();

                    extractResponseMessage.FilePath = filePath; 
                    extractResponseMessage.ExtractResponseStream = responseStream;
                    extractResponseMessage.ExtractResponseMessage = response;
                    extractResponseMessage.ExtractStatusCode = response.StatusCode;
                    extractResponseMessage.ExtractRequestDetails = extractDetails;                    
                }
                else
                {
                    extractResponseMessage.ExtractStatusCode = System.Net.HttpStatusCode.BadRequest;
                }
                return extractResponseMessage;
            }
            catch (TimeoutException timeoutException)
            {
                _logger.LogError(timeoutException, "A timeout error has occurred");
                throw;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "An error occurred in trying to execute a GET request");
                throw;
            }
        }


        private string ConstructFilePath(SplunkInstance splunkInstance, FileType fileType, Extract extractRequestDetails)
        {
            var filePathString = new StringBuilder();
            filePathString.Append(fileType.DirectoryName);
            filePathString.Append(_filePathConstants.PathSeparator);
            filePathString.Append(_filePathConstants.ProjectNameFilePrefix);
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(fileType.FileTypeFilePrefix);
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(extractRequestDetails.QueryFromDate);
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(extractRequestDetails.QueryToDate);
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(splunkInstance.Source);
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(DateTimeOffset.UtcNow.ToString());
            filePathString.Append(_filePathConstants.FileExtension);
            return filePathString.ToString();
        }

        private async Task<Extract> GetNextExtractDetails(int fileTypeId)
        {
            var procedureName = "ApiReader.DetermineNextExtract";
            var parameters = new DynamicParameters();
            parameters.Add("@FileTypeId", fileTypeId);
            parameters.Add("@ExtractRequired", dbType: System.Data.DbType.Boolean, direction: System.Data.ParameterDirection.Output);
            parameters.Add("@QueryFromDate", dbType: System.Data.DbType.DateTime2, direction: System.Data.ParameterDirection.Output);
            parameters.Add("@QueryToDate", dbType: System.Data.DbType.DateTime2, direction: System.Data.ParameterDirection.Output);

            var result = await _dataService.ExecuteStoredProcedure<Extract>(procedureName, parameters);
            return result.FirstOrDefault();
        }

        protected UriBuilder ConstructRequestUri(Extract extractDetails)
        {
            var uriBuilder = new UriBuilder
            {
                Host = _splunkClient.HostName,
                Port = _splunkClient.HostPort
            };
            var query = HttpUtility.ParseQueryString(_splunkClient.QueryParameters);
            query.Add(Uri.EscapeDataString("QueryDateFrom"), extractDetails.QueryFromDate.ToString("dd-MM-yyyy"));
            query.Add(Uri.EscapeDataString("QueryDateTo"), extractDetails.QueryToDate.ToString("dd-MM-yyyy"));
            uriBuilder.Query = query.ToString();
            return uriBuilder;
        }
    }
}
