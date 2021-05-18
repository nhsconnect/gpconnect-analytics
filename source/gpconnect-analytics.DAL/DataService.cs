using Dapper;
using gpconnect_analytics.DAL.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL
{
    public class DataService : IDataService
    {
        private readonly ILogger<DataService> _logger;
        private readonly IConfiguration _configuration;

        public DataService(ILogger<DataService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<List<T>> ExecuteStoredProcedure<T>(string procedureName, DynamicParameters parameters) where T : class
        {
            var connectionString = _configuration.GetConnectionString(ConnectionStrings.GpConnectAnalytics);
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    _logger.LogInformation($"Executing stored procedure {procedureName}", parameters);
                    var results = await sqlConnection.QueryAsync<T>(procedureName, parameters, commandType: System.Data.CommandType.StoredProcedure);
                    return results.AsList();
                }
                catch (Exception exc)
                {
                    _logger?.LogError(exc, $"An error has occurred while attempting to execute the function {procedureName}");
                    throw;
                }
            }
        }

        public async Task<int> ExecuteStoredProcedure(string procedureName, DynamicParameters parameters)
        {
            var connectionString = _configuration.GetConnectionString(ConnectionStrings.GpConnectAnalytics);
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    _logger.LogInformation($"Executing stored procedure {procedureName}", parameters);
                    var result = await sqlConnection.ExecuteAsync(procedureName, parameters, commandType: System.Data.CommandType.StoredProcedure);
                    return result;
                }
                catch (Exception exc)
                {
                    _logger?.LogError(exc, $"An error has occurred while attempting to execute the function {procedureName}");
                    throw;
                }
            }
        }
    }
}
