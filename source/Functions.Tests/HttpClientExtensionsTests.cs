using System.Security.Authentication;
using FluentAssertions;
using Functions.Configuration.Infrastructure.HttpClient;

namespace Functions.Tests;

public class HttpClientExtensionsTests
{
    [Fact]
    public void ConfigureHttpClient_ShouldSetTimeoutAndAcceptHeader()
    {
        // Arrange
        var options = new HttpClient();

        // Act
        HttpClientExtensions.ConfigureHttpClient(options);

        // Assert
        options.Timeout.Should().Be(new TimeSpan(0, 0, 1, 0));
        options.DefaultRequestHeaders.Accept.Should().ContainSingle(h => h.MediaType == "text/csv");
        options.DefaultRequestHeaders.CacheControl?.NoCache.Should().BeTrue();
    }

    [Fact]
    public void CreateHttpMessageHandler_ShouldReturnHandlerWithCorrectSslProtocols()
    {
        // Act
        var handler = HttpClientExtensions.CreateHttpMessageHandler();

        // Assert
        handler.Should().BeOfType<HttpClientHandler>(); // Verify type
        var httpClientHandler = (HttpClientHandler)handler;

        httpClientHandler.SslProtocols.Should().Be(
            SslProtocols.Tls13 | SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls,
            because: "the handler should support TLS 1.0, 1.1, 1.2, and 1.3"
        );
    }
}