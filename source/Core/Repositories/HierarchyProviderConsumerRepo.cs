using System.Data;
using Core.DTOs.Request;
using Core.Helpers;
using Core.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Core.Repositories;

public class HierarchyProviderConsumerRepo(
    ICoreConfigurationService configurationService,
    IDapperWrapper dapperWrapper,
    IConnectionFactory connectionFactory,
    ILogger logger)
    : IHierarchyProviderConsumerRepo
{
    public async Task<int> InsertHierarchyProviderConsumers(List<OrganisationHierarchyProvider> providers)
    {
        var connectionString = configurationService.GetConnectionString(ConnectionStrings.GpConnectAnalytics);
        await using var connection = connectionFactory.CreateConnection(connectionString);  

        // Explicitly open the connection
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

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

        try
        {
            await dapperWrapper.ExecuteAsync(connection, sql, providers, transaction);
            await transaction.CommitAsync();
            return providers.Count;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error inserting hierarchy provider consumers");
            return 0;
        }
    }
}