using System.Reflection;
using Core.Helpers;
using FakeItEasy;
using FluentAssertions;

namespace gpconnect_analytics.Test;

[TestFixture]
public class ApplicationHelperTests
{
    [SetUp]
    public void Setup()
    {
        // Reset the environment variable before each test
        Environment.SetEnvironmentVariable("BUILD_TAG", null);
    }

    [Test]
    public void GetAssemblyVersionInternal_ShouldReturnAssemblyFullName_WhenBuildTagIsNull()
    {
        // Arrange
        Environment.SetEnvironmentVariable("BUILD_TAG", null);
        var fakeAssembly = A.Fake<Func<Assembly>>();
        A.CallTo(() => fakeAssembly.Invoke()).Returns(typeof(ApplicationHelper).Assembly);

        // Act
        var result = ApplicationHelper.ApplicationVersion.GetAssemblyVersionInternal(fakeAssembly);

        // Assert
        result.Should().Be(typeof(ApplicationHelper).Assembly.GetName().FullName);
    }

    [Test]
    public void GetAssemblyVersion_ShouldReturnAssemblyFullName_WhenBuildTagIsNull()
    {
        // Arrange
        Environment.SetEnvironmentVariable("BUILD_TAG", null);

        // Act
        var result = ApplicationHelper.ApplicationVersion.GetAssemblyVersion();

        // Assert
        result.Should().Be(typeof(ApplicationHelper).Assembly.GetName().FullName);
    }

    [Test]
    public void GetAssemblyVersionInternal_ShouldReturnAssemblyFullName_WhenBuildTagIsEmpty()
    {
        // Arrange
        Environment.SetEnvironmentVariable("BUILD_TAG", "");
        var fakeAssembly = A.Fake<Func<Assembly>>();
        A.CallTo(() => fakeAssembly.Invoke()).Returns(typeof(ApplicationHelper).Assembly);

        // Act
        var result = ApplicationHelper.ApplicationVersion.GetAssemblyVersionInternal(fakeAssembly);

        // Assert
        result.Should().Be(typeof(ApplicationHelper).Assembly.GetName().FullName);
    }

    [Test]
    public void GetAssemblyVersionInternal_ShouldReturnAssemblyFullName_WhenBuildTagIsWhitespace()
    {
        // Arrange
        Environment.SetEnvironmentVariable("BUILD_TAG", "   ");
        var fakeAssembly = A.Fake<Func<Assembly>>();
        A.CallTo(() => fakeAssembly.Invoke()).Returns(typeof(ApplicationHelper).Assembly);

        // Act
        var result = ApplicationHelper.ApplicationVersion.GetAssemblyVersionInternal(fakeAssembly);

        // Assert
        result.Should().Be(typeof(ApplicationHelper).Assembly.GetName().FullName);
    }

    [Test]
    public void GetAssemblyVersionInternal_ShouldReturnBuildTag_WhenBuildTagIsSet()
    {
        // Arrange
        string expectedBuildTag = "1.0.0-Build123";
        Environment.SetEnvironmentVariable("BUILD_TAG", expectedBuildTag);
        var fakeAssembly = A.Fake<Func<Assembly>>();

        // Act
        var result = ApplicationHelper.ApplicationVersion.GetAssemblyVersionInternal(fakeAssembly);

        // Assert
        result.Should().Be(expectedBuildTag);
    }

    [Test]
    public void GetAssemblyVersionInternal_ShouldHandleNullAssembly()
    {
        // Arrange
        Environment.SetEnvironmentVariable("BUILD_TAG", null);
        var fakeAssembly = A.Fake<Func<Assembly>>();
        A.CallTo(() => fakeAssembly.Invoke()).Returns(null);

        // Act
        var result = ApplicationHelper.ApplicationVersion.GetAssemblyVersionInternal(fakeAssembly);

        // Assert
        result.Should().BeNull();
    }
}