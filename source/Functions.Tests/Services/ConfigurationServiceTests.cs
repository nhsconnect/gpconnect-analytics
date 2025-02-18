using Bogus;
using Core.DTOs.Response.Configuration;
using Core.Helpers;
using Core.Services.Interfaces;
using Dapper;
using FluentAssertions;
using Functions.Services;
using Microsoft.Extensions.Logging.Testing;
using Moq;

namespace Functions.Tests.Services
{
    public class ConfigurationServiceTests
    {
        private readonly Mock<IDataService> _mockDataService;
        private readonly FakeLogger<ConfigurationService> _fakeLogger;
        private readonly ConfigurationService _configurationService;

        public ConfigurationServiceTests()
        {
            _mockDataService = new Mock<IDataService>();
            _fakeLogger = new FakeLogger<ConfigurationService>();

            _configurationService = new ConfigurationService(_mockDataService.Object, _fakeLogger);
        }

        private static BlobStorage GenerateBlobStorage() =>
            new Faker<BlobStorage>()
                .RuleFor(b => b.ConnectionString, f => f.Internet.Url())
                .RuleFor(b => b.ContainerName, f => f.Random.Word())
                .RuleFor(b => b.QueueName, f => f.Random.Word())
                .Generate();

        private static FilePathConstants GenerateFilePathConstants() =>
            new Faker<FilePathConstants>()
                .RuleFor(f => f.PathSeparator, "/")
                .RuleFor(f => f.ProjectNameFilePrefix, "proj_")
                .RuleFor(f => f.ComponentSeparator, "_")
                .RuleFor(f => f.FileExtension, ".csv")
                .Generate();

        private static List<FileType> GenerateFileTypes(int count = 3) =>
            new Faker<FileType>()
                .RuleFor(f => f.FileTypeFilePrefix, f => f.Random.Word())
                .RuleFor(f => f.DirectoryName, f => f.System.DirectoryPath())
                .RuleFor(f => f.Enabled, true)
                .Generate(count);

        private static SplunkClient GenerateSplunkClient() =>
            new Faker<SplunkClient>()
                .RuleFor(s => s.ApiToken, f => f.Random.AlphaNumeric(32))
                .RuleFor(s => s.QueryTimeout, f => f.Random.Int(10, 60))
                .Generate();

        private static List<SplunkInstance> GenerateSplunkInstances(int count = 3) => new List<SplunkInstance>()
        {
            new SplunkInstance()
            {
                Source = SplunkInstances.cloud.ToString(),
                SourceGroup = "fakeGroup1",
            },
            new SplunkInstance()
            {
                Source = SplunkInstances.spineb.ToString(),
                SourceGroup = "fakeGroup2",
            },
            new SplunkInstance()
            {
                Source = SplunkInstances.spinea.ToString(),
                SourceGroup = "fakeGroup3",
            },
        };

        #region GetBlobStorageConfiguration Tests

        [Fact]
        public async Task GetBlobStorageConfiguration_ShouldReturnBlobStorage_WhenDataExists()
        {
            var expected = GenerateBlobStorage();
            _mockDataService
                .Setup(x => x.ExecuteQueryStoredProcedure<BlobStorage>("[Configuration].[GetBlobStorageConfiguration]",
                    It.IsAny<DynamicParameters>()))
                .ReturnsAsync(new List<BlobStorage> { expected });

            var result = await _configurationService.GetBlobStorageConfiguration();

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetBlobStorageConfiguration_ShouldReturnNull_WhenNoDataExists()
        {
            _mockDataService
                .Setup(x => x.ExecuteQueryStoredProcedure<BlobStorage>(
                    It.IsAny<string>(),
                    It.IsAny<DynamicParameters>()))
                .ReturnsAsync(new List<BlobStorage>());

            var result = await _configurationService.GetBlobStorageConfiguration();

            result.Should().BeNull();
        }

        #endregion

        #region GetFilePathConstants Tests

        [Fact]
        public async Task GetFilePathConstants_ShouldReturnFilePathConstants_WhenDataExists()
        {
            var expected = GenerateFilePathConstants();
            _mockDataService
                .Setup(x => x.ExecuteQueryStoredProcedure<FilePathConstants>(
                    "[Configuration].[GetFilePathConstants]",
                    It.IsAny<DynamicParameters>()))
                .ReturnsAsync(new List<FilePathConstants> { expected });

            var result = await _configurationService.GetFilePathConstants();

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetFilePathConstants_ShouldReturnNull_WhenNoDataExists()
        {
            _mockDataService
                .Setup(x => x.ExecuteQueryStoredProcedure<FilePathConstants>(
                    "[Configuration].[GetFilePathConstants]",
                    It.IsAny<DynamicParameters>()))
                .ReturnsAsync(new List<FilePathConstants>());

            var result = await _configurationService.GetFilePathConstants();

            result.Should().BeNull();
        }

        #endregion

        #region GetFileTypes Tests

        [Fact]
        public async Task GetFileTypes_ShouldReturnFileTypes_WhenDataExists()
        {
            var expected = GenerateFileTypes();
            _mockDataService
                .Setup(x => x.ExecuteQueryStoredProcedure<FileType>("[Configuration].[GetFileTypes]",
                    It.IsAny<DynamicParameters>()))
                .ReturnsAsync(expected);

            var result = await _configurationService.GetFileTypes();

            result.Should().BeEquivalentTo(expected);
        }


        [Fact]
        public async Task GetFileTypes_ShouldReturnEmptyList_WhenNoDataExists()
        {
            _mockDataService
                .Setup(x => x.ExecuteQueryStoredProcedure<FileType>("[Configuration].[GetFileTypes]",
                    It.IsAny<DynamicParameters>()))
                .ReturnsAsync([]);

            var result = await _configurationService.GetFileTypes();

            result.Should().BeEmpty();
        }

        #endregion

        #region GetFileType

        [Fact]
        public async Task GetFileType_ShouldReturnMatchingFiletTypeFromConfiguration()
        {
            var expectedResult = new FileType()
            {
                FileTypeId = 1,
                DirectoryName = "fakeDir",
                FileTypeFilePrefix = "asidlookup",
                Enabled = true,
            };

            var fakeResults = new List<FileType>()
            {
                new()
                {
                    FileTypeId = 2,
                    DirectoryName = "fakeDir",
                    FileTypeFilePrefix = "ssptrans",
                    Enabled = true,
                },
                new()
                {
                    FileTypeId = 3,
                    DirectoryName = "fakeDir",
                    FileTypeFilePrefix = "meshtrans",
                    Enabled = true,
                },
                expectedResult
            };

            _mockDataService.Setup(x =>
                    x.ExecuteQueryStoredProcedure<FileType>("[Configuration].[GetFileTypes]",
                        It.IsAny<DynamicParameters>()))
                .ReturnsAsync(fakeResults);

            var result = await _configurationService.GetFileType(FileTypes.asidlookup);

            result.Should().BeEquivalentTo(expectedResult);
        }

        #endregion

        #region GetSplunkClientConfiguration Tests

        [Fact]
        public async Task GetSplunkClientConfiguration_ShouldReturnSplunkClient_WhenDataExists()
        {
            var expected = GenerateSplunkClient();
            _mockDataService
                .Setup(x =>
                    x.ExecuteQueryStoredProcedure<SplunkClient>("[Configuration].[GetSplunkClientConfiguration]",
                        It.IsAny<DynamicParameters>()))
                .ReturnsAsync(new List<SplunkClient> { expected });

            var result = await _configurationService.GetSplunkClientConfiguration();

            result.Should().BeEquivalentTo(expected);
        }

        #endregion

        #region GetSplunkInstance Tests

        [Fact]
        public async Task GetSplunkInstance_ShouldReturnMatchingInstance_WhenExists()
        {
            var instances = GenerateSplunkInstances();
            var targetInstance = instances.First();

            _mockDataService
                .Setup(x => x.ExecuteQueryStoredProcedure<SplunkInstance>("[Configuration].[GetSplunkInstances]",
                    It.IsAny<DynamicParameters>()))
                .ReturnsAsync(instances);

            var result =
                await _configurationService.GetSplunkInstance(Enum.Parse<SplunkInstances>(targetInstance.Source));

            result.Should().BeEquivalentTo(targetInstance);
        }

        #endregion
    }
}