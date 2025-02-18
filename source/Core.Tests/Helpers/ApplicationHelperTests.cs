using System.Reflection;
using Core.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace Core.Tests.Helpers;

public class ApplicationHelperTests
{
    public ApplicationHelperTests()
    {
        // Reset the environment variable before each test
        Environment.SetEnvironmentVariable("BUILD_TAG", null);
    }

    [Fact]
    public void GetAssemblyVersionInternal_ShouldReturnAssemblyFullName_WhenBuildTagIsNull()
    {
        // Arrange
        Environment.SetEnvironmentVariable("BUILD_TAG", null);
        var mockAssembly = new Mock<Func<Assembly>>();
        mockAssembly.Setup(x => x.Invoke()).Returns(typeof(ApplicationHelper).Assembly);

        // Act
        var result = ApplicationHelper.ApplicationVersion.GetAssemblyVersionInternal(mockAssembly.Object);

        // Assert
        result.Should().Be(typeof(ApplicationHelper).Assembly.GetName().FullName);
    }

    [Fact]
    public void GetAssemblyVersion_ShouldReturnAssemblyFullName_WhenBuildTagIsNull()
    {
        // Arrange
        Environment.SetEnvironmentVariable("BUILD_TAG", null);

        // Act
        var result = ApplicationHelper.ApplicationVersion.GetAssemblyVersion();

        // Assert
        result.Should().Be(typeof(ApplicationHelper).Assembly.GetName().FullName);
    }

    [Fact]
    public void GetAssemblyVersionInternal_ShouldReturnAssemblyFullName_WhenBuildTagIsEmpty()
    {
        // Arrange
        Environment.SetEnvironmentVariable("BUILD_TAG", "");
        var mockAssembly = new Mock<Func<Assembly>>();
        mockAssembly.Setup(x => x.Invoke()).Returns(typeof(ApplicationHelper).Assembly);

        // Act
        var result = ApplicationHelper.ApplicationVersion.GetAssemblyVersionInternal(mockAssembly.Object);

        // Assert
        result.Should().Be(typeof(ApplicationHelper).Assembly.GetName().FullName);
    }

    [Fact]
    public void GetAssemblyVersionInternal_ShouldReturnAssemblyFullName_WhenBuildTagIsWhitespace()
    {
        // Arrange
        Environment.SetEnvironmentVariable("BUILD_TAG", "   ");
        var mockAssembly = new Mock<Func<Assembly>>();
        mockAssembly.Setup(x => x.Invoke()).Returns(typeof(ApplicationHelper).Assembly);

        // Act
        var result = ApplicationHelper.ApplicationVersion.GetAssemblyVersionInternal(mockAssembly.Object);

        // Assert
        result.Should().Be(typeof(ApplicationHelper).Assembly.GetName().FullName);
    }

    [Fact]
    public void GetAssemblyVersionInternal_ShouldReturnBuildTag_WhenBuildTagIsSet()
    {
        // Arrange
        string expectedBuildTag = "1.0.0-Build123";
        Environment.SetEnvironmentVariable("BUILD_TAG", expectedBuildTag);
        var mockAssembly = new Mock<Func<Assembly>>();

        // Act
        var result = ApplicationHelper.ApplicationVersion.GetAssemblyVersionInternal(mockAssembly.Object);

        // Assert
        result.Should().Be(expectedBuildTag);
    }

    [Fact]
    public void GetAssemblyVersionInternal_ShouldHandleNullAssembly()
    {
        // Arrange
        Environment.SetEnvironmentVariable("BUILD_TAG", null);
        var mockAssembly = new Mock<Func<Assembly>>();
        mockAssembly.Setup(x => x.Invoke()).Returns((Assembly)null);

        // Act
        var result = ApplicationHelper.ApplicationVersion.GetAssemblyVersionInternal(mockAssembly.Object);

        // Assert
        result.Should().BeNull();
    }
}