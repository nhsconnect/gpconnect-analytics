using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using Core;
using Core.DTOs.Request;
using Core.DTOs.Response.Configuration;
using Core.DTOs.Response.Splunk;
using Core.Helpers;
using Core.Services.Interfaces;
using Functions.HelperClasses;
using Functions.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Functions.Services
{
    public class SplunkService : ISplunkService
    {
        private SplunkClient _splunkClient;
        private FilePathConstants _filePathConstants;
        private Extract _extract;

        private FilePathHelper filePathHelper;
        private readonly IConfigurationService _configurationService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IImportService _importService;
        private readonly ILogger<SplunkService> _logger;

        public SplunkService(IConfigurationService configurationService,
            IHttpClientFactory httpClientFactory,
            IImportService importService,
            ILogger<SplunkService> logger,
            ITimeProvider timeProvider)
        {
            _extract = new();
            _configurationService = configurationService;
            _httpClientFactory = httpClientFactory;
            _importService = importService;
            _logger = logger;
            filePathHelper = new FilePathHelper(configurationService, timeProvider, _extract);
        }

        public async Task<ExtractResponse> DownloadCSVDateRangeAsync(FileType fileType, UriRequest uriRequest,
            bool isToday)
        {
            try
            {
                var splunkInstance = await _configurationService.GetSplunkInstance(SplunkInstances.cloud);

                _extract.Override = true;
                _extract.QueryFromDate = uriRequest.EarliestDate;
                _extract.QueryToDate = uriRequest.LatestDate;
                _extract.QueryHour = uriRequest.Hour;

                var filePath = await filePathHelper.ConstructFilePath(splunkInstance, fileType, isToday, true);
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

        public async Task<ExtractResponse> GetSearchResultFromRequestUri(UriRequest uriRequest)
        {
            var extractResponseMessage = new ExtractResponse
            {
                ExtractResponseMessage = new HttpResponseMessage()
            };
            try
            {
                _splunkClient = await _configurationService.GetSplunkClientConfiguration();
                var apiTokenExpiry = HasApiTokenExpired(_splunkClient.ApiToken);

                if (!apiTokenExpiry.Expired)
                {
                    var client = _httpClientFactory.CreateClient("SplunkApiClient");
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _splunkClient.ApiToken);
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
                    extractResponseMessage.ExtractResponseMessage.ReasonPhrase =
                        "The authentication token has expired";
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
                extractResponseMessage.ExtractResponseMessage.StatusCode =
                    System.Net.HttpStatusCode.InternalServerError;
            }

            return extractResponseMessage;
        }


        public async Task ExecuteBatchDownloadFromSplunk(FileType fileType, UriRequest uriRequest, bool isToday)
        {
            try
            {
                if (FileTypeEnabled(fileType))
                {
                    var extractResponse = await DownloadCSVDateRangeAsync(fileType, uriRequest, isToday);
                    await _importService.AddObjectFileMessage(fileType, extractResponse);
                }
                else
                {
                    _logger?.LogWarning(
                        $"Filetype {fileType.FileTypeFilePrefix} is not enabled. Please check if this is correct");
                }
            }
            catch (Exception exc)
            {
                _logger?.LogError(exc, $"An error has occurred while attempting to execute an Azure function");
                throw;
            }
        }

        private (bool Expired, DateTime ValidTo) HasApiTokenExpired(string apiToken)
        {
            var jwtToken = new JwtSecurityToken(apiToken);
            return (DateTime.UtcNow > jwtToken.ValidTo, jwtToken.ValidTo);
        }

        internal static bool FileTypeEnabled(FileType fileType)
        {
            return fileType is { Enabled: true };
        }
    }
}