using Core.DTOs.Request;
using Core.Repositories;
using Core.Services.Interfaces;
using Dapper;
using DotNet.Testcontainers.Builders;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
using File = System.IO.File;

namespace gpconnect_analytics.IntegrationTests.RepositoryIntegration
{
    public class HierarchyProviderConsumerRepoTests : IAsyncLifetime
    {
        private readonly MsSqlContainer _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("P@ssword123")
            .WithPortBinding(1443)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
            .Build();

        private HierarchyProviderConsumerRepo? _repo;


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
            await _repo.InsertHierarchyProviderConsumers(providers);

            // Assert
            await using var connection = new SqlConnection(_container.GetConnectionString());
            await connection.OpenAsync();
            const string countQuery = "SELECT COUNT(*) FROM [Data].[HierarchyProviderConsumers]";
            var count = await connection.ExecuteAsync(countQuery);

            count.Should().Be(2);

            const string selectQuery = "SELECT * FROM [Data].[HierarchyProviderConsumers] WHERE OdsCode = @OdsCode";
            var result =
                await connection.QuerySingleAsync<OrganisationHierarchyProvider>(selectQuery,
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

        public async Task InitializeAsync()
        {
            // Create docker container for testing
            await _container.StartAsync();

            var fakeCoreConfigurationService = A.Fake<ICoreConfigurationService>();

            // Set up the GetConnectionString method to return container's connection string
            A.CallTo(() => fakeCoreConfigurationService.GetConnectionString(A<string>.Ignored))
                .Returns(_container.GetConnectionString());

            _repo = new HierarchyProviderConsumerRepo(fakeCoreConfigurationService);

            // create table and schema for test
            await CreateSchemaAndTable();
        }

        public async Task DisposeAsync() => await _container.DisposeAsync();

        private async Task CreateSchemaAndTable()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "AppendixFiles", "SeedingHierarchy.txt");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var fileContent = await File.ReadAllTextAsync(filePath);
            var connection = new SqlConnection(_container.GetConnectionString());
            await connection.OpenAsync();
            await connection.ExecuteAsync(fileContent);
        }
    }
}