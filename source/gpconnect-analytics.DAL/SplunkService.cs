using gpconnect_analytics.DAL.Interfaces;
using gpconnect_analytics.DTO.Request;
using gpconnect_analytics.DTO.Response.Configuration;
using gpconnect_analytics.DTO.Response.Splunk;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL
{
    public class SplunkService : ISplunkService
    {
        private readonly IConfigurationService _configurationService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SplunkService> _logger;
        private SplunkClient _splunkClient;
        private FilePathConstants _filePathConstants;
        private Extract _extract;

        public SplunkService(IConfigurationService configurationService, IHttpClientFactory httpClientFactory, ILogger<SplunkService> logger)
        {
            _configurationService = configurationService;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _extract = new Extract();
        }

        public async Task<ExtractResponse> DownloadCSVDateRangeAsync(FileType fileType, UriRequest uriRequest, bool isToday)
        {
            try
            {
                _filePathConstants = await _configurationService.GetFilePathConstants();
                var splunkInstance = await _configurationService.GetSplunkInstance(Helpers.SplunkInstances.cloud);

                _extract.Override = true;
                _extract.QueryFromDate = uriRequest.EarliestDate;
                _extract.QueryToDate = uriRequest.LatestDate;
                _extract.QueryHour = uriRequest.Hour;

                var filePath = ConstructFilePath(splunkInstance, fileType, isToday, true);
                var extractResponse = await GetSearchResultFromRequestUri(uriRequest);

                extractResponse.FilePath = filePath;
                extractResponse.ExtractRequestDetails = _extract;

                return extractResponse;
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

        private async Task<ExtractResponse> GetSearchResultFromRequestUri(UriRequest uriRequest)
        {
            var extractResponseMessage = new ExtractResponse
            {
                ExtractResponseMessage = new HttpResponseMessage()
            };
            try
            {
                _splunkClient = await _configurationService.GetSplunkClientConfiguration();
                var apiTokenExpiry = HasApiTokenExpired(_splunkClient.ApiToken);

                if (!apiTokenExpiry.Item1)
                {
                    var client = _httpClientFactory.CreateClient("SplunkApiClient");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _splunkClient.ApiToken);
                    client.Timeout = new TimeSpan(0, 0, _splunkClient.QueryTimeout);

                    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uriRequest.Request);
                    var response = await client.SendAsync(httpRequestMessage);
                    var responseStream = await response.Content.ReadAsStreamAsync();

                    extractResponseMessage.ExtractResponseStream = responseStream;
                    extractResponseMessage.ExtractResponseMessage = response;
                    extractResponseMessage.ExtractRequestDetails = _extract;
                    extractResponseMessage.UriRequest = uriRequest;
                }
                else
                {
                    extractResponseMessage.ExtractResponseMessage.ReasonPhrase = $"The authentication token has expired because it is valid up to {apiTokenExpiry.Item2}";
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

        private string ConstructFilePath(SplunkInstance splunkInstance, FileType fileType, bool isToday, bool setDateAsMidnight = false)
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
            filePathString.Append($"{_extract.QueryFromDate.ToString(Helpers.DateFormatConstants.FilePathQueryDate)}T{_extract.QueryHour.ToString(Helpers.DateFormatConstants.FilePathQueryHour)}");
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append($"{_extract.QueryToDate.ToString(Helpers.DateFormatConstants.FilePathQueryDate)}T{_extract.QueryHour.ToString(Helpers.DateFormatConstants.FilePathQueryHour)}");
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(splunkInstance.Source);
            filePathString.Append(_filePathConstants.ComponentSeparator);
            if (!isToday)
            {
                filePathString.Append(setDateAsMidnight ? DateTime.Today.ToString(Helpers.DateFormatConstants.FilePathNowDate) : DateTime.UtcNow.ToString(Helpers.DateFormatConstants.FilePathNowDate));
            }
            else
            {
                filePathString.Append(DateTime.Today.AddDays(1).AddSeconds(-1).ToString(Helpers.DateFormatConstants.FilePathNowDate));
            }
            filePathString.Append(_filePathConstants.FileExtension);
            return filePathString.ToString();
        }

        private (bool, DateTime) HasApiTokenExpired(string apiToken)
        {
            var jwtToken = new JwtSecurityToken(apiToken);
            return (DateTime.UtcNow > jwtToken.ValidTo, jwtToken.ValidTo);
        }
    }
}
