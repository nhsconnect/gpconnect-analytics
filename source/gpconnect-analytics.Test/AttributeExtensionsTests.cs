using Core.Helpers;

namespace gpconnect_analytics.Test
{
    [TestFixture]
    public class AttributeExtensionsTests
    {
        [Test]
        public void GetFileType_ShouldReturnDocument_ForDocumentPath()
        {
            // Arrange
            const string filePath = "/asid-lookup-data/report.docx";

            // Act
            var fileType = filePath.GetFileType<FileTypes>();

            // Assert
            Assert.That(fileType, Is.EqualTo(FileTypes.asidlookup));
        }

        [Test]
        public void GetFileType_ShouldReturnImage_ForImagePath()
        {
            // Arrange
            const string filePath = "/ssp-transactions/file1.csv";

            // Act
            var fileType = filePath.GetFileType<FileTypes>();

            // Assert
            Assert.That(fileType, Is.EqualTo(FileTypes.ssptrans));
        }

        [Test]
        public void GetFileType_ShouldReturnVideo_ForVideoPath()
        {
            // Arrange
            const string filePath = "/mesh-transactions/file1.csv";

            // Act
            var fileType = filePath.GetFileType<FileTypes>();

            // Assert
            Assert.That(fileType, Is.EqualTo(FileTypes.meshtrans));
        }

        [Test]
        public void GetFileType_ShouldReturnNull_ForUnknownPath()
        {
            // Arrange
            const string filePath = "/unknown/path/file.txt";

            // Act
            var fileType = filePath.GetFileType<FileTypes>();

            // Assert
            Assert.That(fileType, Is.Null);
        }
    }
}