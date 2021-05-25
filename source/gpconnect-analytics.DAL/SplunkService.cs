using Dapper;
using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.DTO.Response.Configuration;
using gpconnect_analytics.DTO.Response.Splunk;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
                _logger.LogInformation("Downloading CSV from Splunk Cloud API", fileType);
                _filePathConstants = await _configurationService.GetFilePathConstants();

                _splunkInstances = await _configurationService.GetSplunkInstances();
                var splunkInstance = _splunkInstances.FirstOrDefault(x => x.Source == Helpers.SplunkInstances.cloud.ToString());

                var extractDetails = await GetNextExtractDetails(fileType.FileTypeId);
                if (extractDetails != null)
                {
                    var extractResponseMessage = await GetSearchResults(fileType.SplunkQuery, extractDetails);
                    var filePath = ConstructFilePath(splunkInstance, fileType, extractDetails);
                    extractResponseMessage.FilePath = filePath;
                    extractResponseMessage.ExtractRequestDetails = extractDetails;
                    _logger.LogInformation("Splunk Cloud API returned response", extractResponseMessage);

                    return extractResponseMessage;
                }
                return null;
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
            filePathString.Append(splunkInstance.Source);
            filePathString.Append(_filePathConstants.PathSeparator);
            filePathString.Append(extractRequestDetails.QueryFromDate.ToString("yyyy-MM"));
            filePathString.Append(_filePathConstants.PathSeparator);
            filePathString.Append(_filePathConstants.ProjectNameFilePrefix);
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(fileType.FileTypeFilePrefix);
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(extractRequestDetails.QueryFromDate.ToString("yyyyMMddT000000"));
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(extractRequestDetails.QueryToDate.ToString("yyyyMMddT000000"));
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(splunkInstance.Source);
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(DateTime.UtcNow.ToString("yyyyMMddTHHmmss"));
            filePathString.Append(_filePathConstants.FileExtension);
            return filePathString.ToString();
        }

        private async Task<Extract> GetNextExtractDetails(int fileTypeId)
        {
            var procedureName = "ApiReader.DetermineNextExtract";
            var parameters = new DynamicParameters();
            parameters.Add("@FileTypeId", fileTypeId);
            parameters.Add("@ExtractRequired", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            parameters.Add("@QueryFromDate", dbType: DbType.DateTime2, direction: ParameterDirection.Output);
            parameters.Add("@QueryToDate", dbType: DbType.DateTime2, direction: ParameterDirection.Output);

            _logger.LogInformation("Determining next extract details", parameters);
            var result = await _dataService.ExecuteStoredProcedureWithOutputParameters(procedureName, parameters);

            var extract = new Extract
            {
                ExtractRequired = result.Get<bool>("@ExtractRequired"),
                QueryFromDate = result.Get<DateTime>("@QueryFromDate"),
                QueryToDate = result.Get<DateTime>("@QueryToDate")
            };

            return extract;
        }

        private async Task<ExtractResponse> GetSearchResults(string splunkQuery, Extract extractDetails)
        {
            var extractResponseMessage = new ExtractResponse { ExtractResponseMessage = new HttpResponseMessage() };
            try
            {
                if (extractDetails.ExtractRequired)
                {
                    splunkQuery = splunkQuery.Replace("{earliest}", extractDetails.QueryFromDate.ToString("MM/dd/yyyy:HH:mm:ss"));
                    splunkQuery = splunkQuery.Replace("{latest}", extractDetails.QueryToDate.ToString("MM/dd/yyyy:HH:mm:ss"));

                    _splunkClient = await _configurationService.GetSplunkClientConfiguration();

                    if (extractDetails != null)
                    {
                        var client = _httpClientFactory.CreateClient("SplunkApiClient");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _splunkClient.ApiToken);
                        client.Timeout = new TimeSpan(0,0, _splunkClient.QueryTimeout);
                        
                        var uriBuilder = new UriBuilder
                        {
                            Scheme = "https",
                            Host = _splunkClient.HostName,
                            Port = _splunkClient.HostPort,
                            Path = _splunkClient.BaseUrl,
                            Query = string.Format(_splunkClient.QueryParameters, HttpUtility.UrlEncode(splunkQuery))
                        };

                        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);
                        _logger.LogInformation("Sending download request to Splunk Cloud API", httpRequestMessage);
                        var response = await client.SendAsync(httpRequestMessage);

                        var responseStream = await response.Content.ReadAsStreamAsync();
                        _logger.LogInformation("Reading content response stream from Splunk Cloud API", responseStream);

                        extractResponseMessage.ExtractResponseStream = responseStream;
                        extractResponseMessage.ExtractResponseMessage = response;
                        extractResponseMessage.ExtractRequestDetails = extractDetails;
                    }
                }
                else
                {
                    extractResponseMessage.ExtractResponseMessage.ReasonPhrase = "No extract required";
                    extractResponseMessage.ExtractResponseMessage.StatusCode = System.Net.HttpStatusCode.NoContent;
                }
            }
            catch(OperationCanceledException operationCancelledException)
            {
                extractResponseMessage.ExtractResponseMessage.ReasonPhrase = operationCancelledException.Message;
                extractResponseMessage.ExtractResponseMessage.StatusCode = System.Net.HttpStatusCode.RequestTimeout;
            }
            catch (Exception ex)
            {
                extractResponseMessage.ExtractResponseMessage.ReasonPhrase = ex.Message;
                extractResponseMessage.ExtractResponseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
            }
            return extractResponseMessage;
        }
    }
}
