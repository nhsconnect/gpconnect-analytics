using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using Core;
using Microsoft.Data.SqlClient;
using Core.DTOs.Request;
using Core.Helpers;
using Core.Repositories;
using Core.Services.Interfaces;
using FakeItEasy;
using gpconnect_analytics.Test.Helpers;
using Xunit;

namespace gpconnect_analytics.Test;

public class HierarchyProviderConsumerRepoTests
{
    [Fact]
    public async Task InsertHierarchyProviderConsumers_ShouldCallExecuteAsyncWithCorrectParameters()
    {
        // Arrange
        var fakeConfigurationService = A.Fake<ICoreConfigurationService>();
        var fakeDapper = A.Fake<IDapperWrapper>();
        const string testConnectionString = "Server=localhost;Database=TestDb;User Id=test;Password=test;";

        // Fake connection string
        A.CallTo(() => fakeConfigurationService.GetConnectionString(ConnectionStrings.GpConnectAnalytics))
            .Returns(testConnectionString);

        var fakeConnectionFactory = A.Fake<IConnectionFactory>();
        var fakeSqlConnection = A.Fake<DbConnection>();
        fakeSqlConnection.ConnectionString = testConnectionString;

        A.CallTo(() => fakeConnectionFactory.CreateConnection(testConnectionString))
            .Returns(fakeSqlConnection);

        // Simulate successful execution in Dapper
        A.CallTo(() => fakeDapper.ExecuteAsync(
                A<DbConnection>._,
                A<string>._,
                A<object>._,
                A<IDbTransaction>._))
            .Returns(1);

        // Create the repo with the fakes
        var repo = new HierarchyProviderConsumerRepo(fakeConfigurationService, fakeDapper, fakeConnectionFactory);

        // Create sample data
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
        await repo.InsertHierarchyProviderConsumers(providers);

        // Assert
        A.CallTo(() => fakeDapper.ExecuteAsync(
                A<DbConnection>.That.Matches(conn => conn.ConnectionString == testConnectionString),
                A<string>.That.Matches(sql => sql.Contains("INSERT INTO [Data].[HierarchyProviderConsumers]")),
                A<List<OrganisationHierarchyProvider>>.That.Matches(param => param.SequenceEqual(providers)),
                A<IDbTransaction>._))
            .MustHaveHappenedOnceExactly();
    }
}