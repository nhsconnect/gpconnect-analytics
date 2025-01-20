using System.Data.SQLite;
using Core.DTOs.Request;
using Core.Helpers;
using Core.Repositories;
using Core.Services.Interfaces;
using Dapper;
using FakeItEasy;
using FluentAssertions;

namespace gpconnect_analytics.Test.RepositoryIntegration
{
    [TestFixture]
    public class HierarchyProviderConsumerRepoTests
    {
        private SQLiteConnection _connection;
        private HierarchyProviderConsumerRepo _repo;
        private ICoreConfigurationService _configurationService;

        [SetUp]
        public void SetUp()
        {
            // Initialize in-memory SQLite connection
            _connection = new SQLiteConnection("Data Source=:memory:;Version=3;");
            _connection.Open();

            // Setup mock configuration service to return SQLite connection string
            _configurationService = A.Fake<ICoreConfigurationService>();
            A.CallTo(() => _configurationService.GetConnectionString(ConnectionStrings.GpConnectAnalytics))
                .Returns("Data Source=:memory:;Version=3;");


            // Create repository with mocked configuration service
            _repo = new HierarchyProviderConsumerRepo(_configurationService);

            // Create the table schema
            const string createTableSql = @"
                CREATE TABLE [Data].[HierarchyProviderConsumers] (
                    OdsCode TEXT,
                    PracticeName TEXT,
                    RegisteredPatientCount INTEGER,
                    RegionCode TEXT,
                    RegionName TEXT,
                    Icb22Name TEXT,
                    PcnName TEXT,
                    Appointments13000 INTEGER
                );";

            _connection.Execute(createTableSql);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up and close the connection
            _connection.Close();
            _connection.Dispose();
        }

        [Test]
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
            await _repo.InsertHierarchyProviderConsumers(providers);

            // Assert
            const string countQuery = "SELECT COUNT(*) FROM [Data].[HierarchyProviderConsumers]";
            var count = await _connection.ExecuteScalarAsync<int>(countQuery);
            count.Should().Be(2);

            const string selectQuery = "SELECT * FROM [Data].[HierarchyProviderConsumers] WHERE OdsCode = @OdsCode";
            var result =
                await _connection.QuerySingleAsync<OrganisationHierarchyProvider>(selectQuery,
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
    }
}