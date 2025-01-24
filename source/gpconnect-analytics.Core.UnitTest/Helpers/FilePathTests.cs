using Core.Helpers;
using FluentAssertions;
using Xunit;

namespace gpconnect_analytics.Test;

public class FilePathTests
{
    [Fact]
    public void FilePathValue_Returns_Value_WhenEmptyIsProvidedToConstructor()
    {
        // Arrange
        var filePathAttribute = new FilePathAttribute("test_file_path");

        // Act
        var filePath = filePathAttribute.FilePath;

        // Assert
        filePath.Should().Be("test_file_path");
    }

    [Fact]
    public void FilePathValue_Returns_Empty_WhenEmptyIsProvidedToConstructor()
    {
        // Arrange
        var filePathAttribute = new FilePathAttribute("");

        // Act
        var filePath = filePathAttribute.FilePath;

        // Assert
        filePath.Should().Be("");
    }

    [Fact]
    public void FilePathValue_Returns_Null_WhenNullIsProvidedToConstructor()
    {
        // Arrange
        var filePathAttribute = new FilePathAttribute(null!);

        // Act
        var filePath = filePathAttribute.FilePath;

        // Assert
        filePath.Should().Be(null);
    }
}