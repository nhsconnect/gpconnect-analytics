using Dapper;
using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.DTO.Response.Configuration;
using gpconnect_analytics.DTO.Response.Splunk;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
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
        private Extract _extract;

        public SplunkService(IConfigurationService configurationService, IDataService dataService, IHttpClientFactory httpClientFactory, ILogger<SplunkService> logger)
        {
            _configurationService = configurationService;
            _dataService = dataService;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _extract = new Extract
            {
                ExtractRequired = false,
                QueryFromDate = DateTime.Today.AddDays(-2),
                QueryToDate = DateTime.Today.AddDays(-1),
            };
        }

        public async Task<ExtractResponse> DownloadCSV(FileType fileType)
        {
            try
            {
                _filePathConstants = await _configurationService.GetFilePathConstants();
                var splunkInstance = await _configurationService.GetSplunkInstance(Helpers.SplunkInstances.cloud);

                await GetNextExtractDetails(fileType.FileTypeId);
                if (_extract.ExtractRequired)
                {
                    var extractResponseMessage = await GetSearchResults(fileType.SplunkQuery);
                    var filePath = ConstructFilePath(splunkInstance, fileType);
                    extractResponseMessage.FilePath = filePath;
                    extractResponseMessage.ExtractRequestDetails = _extract;

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

        private string ConstructFilePath(SplunkInstance splunkInstance, FileType fileType)
        {
            var filePathString = new StringBuilder();
            filePathString.Append(fileType.DirectoryName);
            filePathString.Append(_filePathConstants.PathSeparator);
            filePathString.Append(splunkInstance.Source);
            filePathString.Append(_filePathConstants.PathSeparator);
            filePathString.Append(_extract.QueryFromDate.ToString(Helpers.DateFormatConstants.FilePathQueryDateYearMonth));
            filePathString.Append(_filePathConstants.PathSeparator);
            filePathString.Append(_filePathConstants.ProjectNameFilePrefix);
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(fileType.FileTypeFilePrefix);
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(_extract.QueryFromDate.ToString(Helpers.DateFormatConstants.FilePathQueryDate));
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(_extract.QueryToDate.ToString(Helpers.DateFormatConstants.FilePathQueryDate));
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(splunkInstance.Source);
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(DateTime.UtcNow.ToString(Helpers.DateFormatConstants.FilePathNowDate));
            filePathString.Append(_filePathConstants.FileExtension);
            return filePathString.ToString();
        }

        private async Task GetNextExtractDetails(int fileTypeId)
        {
            var procedureName = "ApiReader.DetermineNextExtract";
            var parameters = new DynamicParameters();
            parameters.Add("@FileTypeId", fileTypeId);
            parameters.Add("@ExtractRequired", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            parameters.Add("@QueryFromDate", dbType: DbType.DateTime2, direction: ParameterDirection.Output);
            parameters.Add("@QueryToDate", dbType: DbType.DateTime2, direction: ParameterDirection.Output);

            _logger.LogInformation("Determining next extract details", parameters);
            var result = await _dataService.ExecuteStoredProcedureWithOutputParameters(procedureName, parameters);

            if (result.Get<bool>("@ExtractRequired"))
            {
                _extract.ExtractRequired = result.Get<bool>("@ExtractRequired");
                _extract.QueryFromDate = result.Get<DateTime>("@QueryFromDate");
                _extract.QueryToDate = result.Get<DateTime>("@QueryToDate");
            }
        }

        private async Task<ExtractResponse> GetSearchResults(string splunkQuery)
        {
            var extractResponseMessage = new ExtractResponse
            {
                ExtractResponseMessage = new HttpResponseMessage()
            };
            try
            {
                splunkQuery = splunkQuery.Replace("{earliest}", _extract.QueryFromDate.ToString(Helpers.DateFormatConstants.SplunkQueryDate));
                splunkQuery = splunkQuery.Replace("{latest}", _extract.QueryToDate.ToString(Helpers.DateFormatConstants.SplunkQueryDate));

                _splunkClient = await _configurationService.GetSplunkClientConfiguration();
                var apiTokenExpiry = HasApiTokenExpired(_splunkClient.ApiToken);

                if (!apiTokenExpiry.Item1)
                {
                    var client = _httpClientFactory.CreateClient("SplunkApiClient");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _splunkClient.ApiToken);
                    client.Timeout = new TimeSpan(0, 0, _splunkClient.QueryTimeout);

                    var uriBuilder = new UriBuilder
                    {
                        Scheme = Uri.UriSchemeHttps,
                        Host = _splunkClient.HostName,
                        Port = _splunkClient.HostPort,
                        Path = _splunkClient.BaseUrl,
                        Query = string.Format(_splunkClient.QueryParameters, HttpUtility.UrlEncode(splunkQuery))
                    };

                    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);
                    var response = await client.SendAsync(httpRequestMessage);
                    var responseStream = await response.Content.ReadAsStreamAsync();

                    extractResponseMessage.ExtractResponseStream = responseStream;
                    extractResponseMessage.ExtractResponseMessage = response;
                    extractResponseMessage.ExtractRequestDetails = _extract;
                }
                else
                {
                    extractResponseMessage.ExtractResponseMessage.ReasonPhrase = $"The authentication has expired because it is valid up to {apiTokenExpiry.Item2}";
                    extractResponseMessage.ExtractResponseMessage.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                }
            }
            catch (OperationCanceledException operationCancelledException)
            {
                extractResponseMessage.ExtractResponseMessage.ReasonPhrase = operationCancelledException.Message;
                extractResponseMessage.ExtractResponseMessage.StatusCode = System.Net.HttpStatusCode.RequestTimeout;
            }
            catch (Exception exc)
            {
                extractResponseMessage.ExtractResponseMessage.ReasonPhrase = exc.Message;
                extractResponseMessage.ExtractResponseMessage.StatusCode = System.Net.HttpStatusCode.InternalServerError;
            }
            return extractResponseMessage;
        }

        private (bool, DateTime) HasApiTokenExpired(string apiToken)
        {
            var jwtToken = new JwtSecurityToken(apiToken);
            return (DateTime.UtcNow > jwtToken.ValidTo, jwtToken.ValidTo);
        }
    }
}
