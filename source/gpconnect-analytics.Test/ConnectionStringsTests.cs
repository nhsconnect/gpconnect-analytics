using Core.Helpers;
using FluentAssertions;

namespace gpconnect_analytics.Test;

[TestFixture]
public class ConnectionStringsTests
{
    [Test]
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