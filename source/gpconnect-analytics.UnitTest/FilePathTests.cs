using Core.Helpers;
using FluentAssertions;

namespace gpconnect_analytics.Test;

[TestFixture]
public class FilePathTests
{
    [Test]
    public void FilePathValue_Returns_Value_WhenEmptyIsProvidedToConstructor()
    {
        // Arrange
        var filePathAttribute = new FilePathAttribute("test_file_path");

        // Act
        var filePath = filePathAttribute.FilePath;

        // Assert
        filePath.Should().Be("test_file_path");
    }

    [Test]
    public void FilePathValue_Returns_Empty_WhenEmptyIsProvidedToConstructor()
    {
        // Arrange
        var filePathAttribute = new FilePathAttribute("");

        // Act
        var filePath = filePathAttribute.FilePath;

        // Assert
        filePath.Should().Be("");
    }

    [Test]
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