using System.Data;
using Core;
using Core.DTOs.Response.Configuration;
using Core.Helpers;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace Functions.Configuration;

public interface IEmailConfigurationProvider
{
    Email? GetEmailConfiguration(IConfiguration configuration);
}

public class EmailConfigurationProvider(IConnectionFactory connectionFactory) : IEmailConfigurationProvider
{
    public Email? GetEmailConfiguration(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStrings.GpConnectAnalytics) ??
                               throw new InvalidOperationException("connection string cannot be null at this point.");

        using var sqlConnection = connectionFactory.CreateConnection(connectionString);

        IEnumerable<Email?> result = sqlConnection.Query<Email>("[Configuration].[GetEmailConfiguration]",
            commandType: CommandType.StoredProcedure);

        return result.FirstOrDefault();
    }
}