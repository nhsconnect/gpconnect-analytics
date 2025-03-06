using Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Core.Tests.Helpers;

public class ConnectionStringsTests
{
    [Fact]
    public void GpConnectAnalytics_ShouldNotBeEmpty()
    {
        // Arrange
        // Act
        var result = ConnectionStrings.GpConnectAnalytics;

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Be("GpConnectAnalytics");
    }
}