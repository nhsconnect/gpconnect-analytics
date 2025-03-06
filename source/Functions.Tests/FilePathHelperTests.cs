using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using Core;
using Core.DTOs.Response.Configuration;
using Core.DTOs.Response.Splunk;
using Core.Helpers;
using Core.Services.Interfaces;
using System.Text;
using FluentAssertions;
using Functions.HelperClasses;
using Functions.Tests.TestHelpers;

namespace Functions.Tests
{
    public class FilePathHelperTests
    {
        private readonly Mock<IConfigurationService> _mockConfigurationService;
        private readonly Mock<ITimeProvider> _mockTimeProvider;
        private readonly Extract _mockExtract;
        private readonly FilePathHelper _filePathHelper;

        public FilePathHelperTests()
        {
            _mockConfigurationService = new Mock<IConfigurationService>();
            _mockTimeProvider = new Mock<ITimeProvider>();

            _mockTimeProvider.Setup(x => x.CurrentDate()).Returns(new DateTime(2025, 12, 25, 0, 0, 0));
            _mockTimeProvider.Setup(x => x.UtcDateTime()).Returns(new DateTime(2025, 12, 25, 11, 11, 0));

            _mockExtract = new Extract
            {
                ExtractRequired = false,
                QueryFromDate = new DateTime(2025, 1, 1, 0, 0, 0),
                QueryToDate = new DateTime(2025, 1, 2, 0, 0, 0),
                QueryHour = TimeSpan.Zero,
                Override = false
            };

            _filePathHelper = new FilePathHelper(
                _mockConfigurationService.Object,
                _mockTimeProvider.Object,
                _mockExtract
            );
        }

        [Fact]
        public async Task ConstructFilePath_WhenIsTodayTrue_MidnightIsFalse_ReturnsExpectedFilePath()
        {
            // Arrange
            var splunkInstance = new SplunkInstance { Source = "source" };
            var fileType = new FileType { DirectoryName = "dir", FileTypeFilePrefix = "filePrefix" };

            var filePathConstants = ConfigurationHelpers.GenerateFilePathConstants();

            _mockConfigurationService.Setup(x => x.GetFilePathConstants())
                .ReturnsAsync(filePathConstants);

            // Act
            var result = await _filePathHelper.ConstructFilePath(splunkInstance, fileType, true);

            // Assert
            var expectedFilePath =
                "dir/source/2025-01/proj__filePrefix_20250101T000000_20250102T000000_source_20251225T235959.csv";
            result.Should().Be(expectedFilePath);
        }

        [Fact]
        public async Task ConstructFilePath_WhenIsTodayFalseAndSetDateAsMidnightTrue_ReturnsExpectedFilePath()
        {
            // Arrange
            var splunkInstance = new SplunkInstance { Source = "source" };
            var fileType = new FileType { DirectoryName = "dir", FileTypeFilePrefix = "filePrefix" };

            var filePathConstants = ConfigurationHelpers.GenerateFilePathConstants();

            _mockConfigurationService.Setup(x => x.GetFilePathConstants())
                .ReturnsAsync(filePathConstants);


            // Act
            var result = await _filePathHelper.ConstructFilePath(splunkInstance, fileType, false, true); //midnight true

            // Assert
            var expectedFilePath =
                "dir/source/2025-01/proj__filePrefix_20250101T000000_20250102T000000_source_20251225T000000.csv";
            result.Should().Be(expectedFilePath);
        }

        [Fact]
        public async Task ConstructFilePath_WhenIsTodayFalseAndSetDateAsMidnightFalse_ReturnsExpectedFilePath()
        {
            // Arrange
            var splunkInstance = new SplunkInstance { Source = "source" };
            var fileType = new FileType { DirectoryName = "dir", FileTypeFilePrefix = "filePrefix" };

            // expectations
            var directory = "dir";
            var source = "source";
            var dateFolder = "2025-01";
            var filePrefix = "filePrefix";
            var queryDateFrom = "20250101T000000";
            var queryDateTo = "20250102T000000";
            var date = "20251225T111100";

            var filePathConstants = ConfigurationHelpers.GenerateFilePathConstants();

            _mockConfigurationService.Setup(x => x.GetFilePathConstants())
                .ReturnsAsync(filePathConstants);

            // Act
            var result = await _filePathHelper.ConstructFilePath(splunkInstance, fileType, false);

            // Assert
            var expectedFilePath =
                $"{directory}/{source}/{dateFolder}/{filePathConstants.ProjectNameFilePrefix}_{filePrefix}_{queryDateFrom}_{queryDateTo}_{source}_{date}{filePathConstants.FileExtension}";
            result.Should().Be(expectedFilePath);
        }
    }
}