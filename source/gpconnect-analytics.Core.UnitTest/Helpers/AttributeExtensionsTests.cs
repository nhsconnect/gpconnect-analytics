using Core.Helpers;
using FluentAssertions;
using Xunit;

namespace gpconnect_analytics.Test
{
    public class AttributeExtensionsTests
    {
        [Fact]
        public void GetFileType_ShouldReturnDocument_ForDocumentPath()
        {
            // Arrange
            const string filePath = "/asid-lookup-data/report.docx";

            // Act
            var fileType = filePath.GetFileType<FileTypes>();

            // Assert
            fileType.Should().Be(FileTypes.asidlookup);
        }

        [Fact]
        public void GetFileType_ShouldReturnImage_ForImagePath()
        {
            // Arrange
            const string filePath = "/ssp-transactions/file1.csv";

            // Act
            var fileType = filePath.GetFileType<FileTypes>();

            // Assert
            fileType.Should().Be(FileTypes.ssptrans);
        }

        [Fact]
        public void GetFileType_ShouldReturnVideo_ForVideoPath()
        {
            // Arrange
            const string filePath = "/mesh-transactions/file1.csv";

            // Act
            var fileType = filePath.GetFileType<FileTypes>();

            // Assert
            fileType.Should().Be(FileTypes.meshtrans);
        }

        [Fact]
        public void GetFileType_ShouldReturnNull_ForUnknownPath()
        {
            // Arrange
            const string filePath = "/unknown/path/file.txt";

            // Act
            var fileType = filePath.GetFileType<FileTypes>();

            // Assert
            fileType.Should().BeNull();
        }
    }
}