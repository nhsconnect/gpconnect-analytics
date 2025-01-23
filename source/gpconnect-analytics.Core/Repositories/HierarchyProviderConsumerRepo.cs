using Core.DTOs.Request;
using Core.Helpers;
using Core.Services.Interfaces;
using Microsoft.Data.SqlClient;

namespace Core.Repositories;

public class HierarchyProviderConsumerRepo(
    ICoreConfigurationService configurationService,
    IDapperWrapper dapperWrapper,
    IConnectionFactory connectionFactory)
    : IHierarchyProviderConsumerRepo
{
    public async Task InsertHierarchyProviderConsumers(List<OrganisationHierarchyProvider> providers)
    {
        var connectionString = configurationService.GetConnectionString(ConnectionStrings.GpConnectAnalytics);
        await using var connection = connectionFactory.CreateConnection(connectionString);
        await using var transaction = await connection.BeginTransactionAsync();

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

        await dapperWrapper.ExecuteAsync(connection, sql, providers,
            transaction);
    }
}