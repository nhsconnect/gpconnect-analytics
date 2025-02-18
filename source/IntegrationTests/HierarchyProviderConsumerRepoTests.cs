using Core;
using Core.DTOs.Request;
using Core.Repositories;
using Core.Services.Interfaces;
using Dapper;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Functions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Testcontainers.MsSql;


namespace IntegrationTests
{
    public class HierarchyProviderConsumerRepoTests : IClassFixture<MsSqlContainerFixture>
    {
        private readonly string _connectionString;
        private readonly HierarchyProviderConsumerRepo _repo;
        private readonly FakeLogger _logger = new();


        public HierarchyProviderConsumerRepoTests(MsSqlContainerFixture fixture)
        {
            _connectionString = fixture.Container.GetConnectionString();
            var mockCoreConfigurationService = new Mock<ICoreConfigurationService>();

            mockCoreConfigurationService.Setup(x => x.GetConnectionString(It.IsAny<string>()))
                .Returns(_connectionString);

            _repo = new HierarchyProviderConsumerRepo(mockCoreConfigurationService.Object, new DapperWrapper(),
                new SqlConnectionFactory(), _logger);
        }

        [Fact]
        public async Task InsertHierarchyProviderConsumers_ShouldInsertMultipleRecordsAndVerifyFirstRecord()
        {
            // Arrange
            var providers = new List<OrganisationHierarchyProvider>
            {
                new OrganisationHierarchyProvider
                {
                    OdsCode = "ABC123",
                    PracticeName = "Test Practice 1",
                    RegisteredPatientCount = 1500,
                    RegionCode = "Region1",
                    RegionName = "Test Region 1",
                    Icb22Name = "ICB Name 1",
                    PcnName = "PCN Name 1",
                    Appointments13000 = 120
                },
                new OrganisationHierarchyProvider
                {
                    OdsCode = "XYZ456",
                    PracticeName = "Test Practice 2",
                    RegisteredPatientCount = 2000,
                    RegionCode = "Region2",
                    RegionName = "Test Region 2",
                    Icb22Name = "ICB Name 2",
                    PcnName = "PCN Name 2",
                    Appointments13000 = 220
                }
            };

            // Act
            var insertResult = await _repo.InsertHierarchyProviderConsumers(providers);

            // Assert

            insertResult.Should().Be(2);

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Verify record count
            const string countQuery = "SELECT COUNT(*) FROM [Data].[HierarchyProviderConsumers]";
            var count = await connection.QuerySingleAsync<int>(countQuery);
            count.Should().Be(2);

            // Verify specific record
            const string selectQuery = "SELECT * FROM [Data].[HierarchyProviderConsumers] WHERE OdsCode = @OdsCode";
            var result = await connection.QuerySingleAsync<OrganisationHierarchyProvider>(selectQuery,
                new { OdsCode = "ABC123" });

            result.OdsCode.Should().Be("ABC123");
            result.PracticeName.Should().Be("Test Practice 1");
            result.RegisteredPatientCount.Should().Be(1500);
            result.RegionCode.Should().Be("Region1");
            result.RegionName.Should().Be("Test Region 1");
            result.Icb22Name.Should().Be("ICB Name 1");
            result.PcnName.Should().Be("PCN Name 1");
            result.Appointments13000.Should().Be(120);
        }

        [Fact]
        public async Task InsertHierarchyProviderConsumers_ShouldLogError_WhenConnectionFails()
        {
            // Arrange
            var providers = new List<OrganisationHierarchyProvider>
            {
                new() { OdsCode = string.Empty, PracticeName = "Test1", RegisteredPatientCount = 100 },
                new() { OdsCode = string.Empty, PracticeName = "Test2", RegisteredPatientCount = 200 } // Duplicate PK
            };


            // Act
            var result = await _repo.InsertHierarchyProviderConsumers(providers);

            // Assert
            result.Should().Be(0); // Should return 0 due to exception
            _logger.Collector.LatestRecord.Message.Should().Be("Error inserting hierarchy provider consumers");
            _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
            _logger.Collector.LatestRecord.Exception?.Message.Should()
                .Contain("Violation of PRIMARY KEY constraint 'PK_Hierarchy");
        }
    }
}