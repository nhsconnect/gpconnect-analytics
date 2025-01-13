using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using Core.DTOs.Request;
using Core.DTOs.Response.Configuration;
using Core.DTOs.Response.Splunk;
using Core.Helpers;
using Core.Services.Interfaces;
using function_app.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace function_app.Services
{
    public class SplunkService(
        IConfigurationService configurationService,
        IHttpClientFactory httpClientFactory,
        ILogger<SplunkService> logger)
        : ISplunkService
    {
        private SplunkClient _splunkClient;
        private FilePathConstants _filePathConstants;
        private Extract _extract = new();

        public async Task<ExtractResponse> DownloadCSVDateRangeAsync(FileType fileType, UriRequest uriRequest,
            bool isToday)
        {
            try
            {
                _filePathConstants = await configurationService.GetFilePathConstants();
                var splunkInstance = await configurationService.GetSplunkInstance(SplunkInstances.cloud);

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
                logger.LogError(timeoutException, "A timeout error has occurred");
                throw;
            }
            catch (Exception exc)
            {
                logger.LogError(exc, "An error occurred in trying to execute a GET request");
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
                _splunkClient = await configurationService.GetSplunkClientConfiguration();
                var apiTokenExpiry = HasApiTokenExpired(_splunkClient.ApiToken);

                if (!apiTokenExpiry.Item1)
                {
                    var client = httpClientFactory.CreateClient("SplunkApiClient");
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
                        $"The authentication token has expired because it is valid up to {apiTokenExpiry.Item2}";
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

        private string ConstructFilePath(SplunkInstance splunkInstance, FileType fileType, bool isToday,
            bool setDateAsMidnight = false)
        {
            var filePathString = new StringBuilder();
            filePathString.Append(fileType.DirectoryName);
            filePathString.Append(_filePathConstants.PathSeparator);
            filePathString.Append(splunkInstance.Source);
            filePathString.Append(_filePathConstants.PathSeparator);
            filePathString.Append(
                _extract.QueryFromDate.ToString(DateFormatConstants.FilePathQueryDateYearMonth));
            filePathString.Append(_filePathConstants.PathSeparator);
            filePathString.Append(_filePathConstants.ProjectNameFilePrefix);
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(fileType.FileTypeFilePrefix);
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(
                $"{_extract.QueryFromDate.ToString(DateFormatConstants.FilePathQueryDate)}T{_extract.QueryHour.ToString(DateFormatConstants.FilePathQueryHour)}");
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(
                $"{_extract.QueryToDate.ToString(DateFormatConstants.FilePathQueryDate)}T{_extract.QueryHour.ToString(DateFormatConstants.FilePathQueryHour)}");
            filePathString.Append(_filePathConstants.ComponentSeparator);
            filePathString.Append(splunkInstance.Source);
            filePathString.Append(_filePathConstants.ComponentSeparator);
            if (!isToday)
            {
                filePathString.Append(setDateAsMidnight
                    ? DateTime.Today.ToString(DateFormatConstants.FilePathNowDate)
                    : DateTime.UtcNow.ToString(DateFormatConstants.FilePathNowDate));
            }
            else
            {
                filePathString.Append(DateTime.Today.AddDays(1).AddSeconds(-1)
                    .ToString(DateFormatConstants.FilePathNowDate));
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