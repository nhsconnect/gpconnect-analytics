using Core.DTOs.Request;
using Core.Helpers;
using Core.Services.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Core.Repositories;

public class HierarchyProviderConsumerRepo(ICoreConfigurationService configurationService)
    : IHierarchyProviderConsumerRepo
{
    public async Task InsertHierarchyProviderConsumers(List<OrganisationHierarchyProvider> providers)
    {
        var connectionString = configurationService.GetConnectionString(ConnectionStrings.GpConnectAnalytics);
        await using var connection = new SqlConnection(connectionString);

        const string sql = """
                           
                                       INSERT INTO [Data].[HierarchyProviderConsumers] (
                                           OdsCode,
                                           PracticeName,
                                           RegisteredPatientCount,
                                           RegionCode,
                                           RegionName,
                                           Icb22Name,
                                           PcnName,
                                           Appointments13000
                                       )
                                       VALUES (
                                           @OdsCode,
                                           @PracticeName,
                                           @RegisteredPatientCount,
                                           @RegionCode,
                                           @RegionName,
                                           @Icb22Name,
                                           @PcnName,
                                           @Appointments13000
                                       );
                           """;

        await connection.ExecuteAsync(sql, providers);
    }
}