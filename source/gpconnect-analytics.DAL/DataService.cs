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
        private readonly string _connectionString;

        public DataService(ILogger<DataService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString(ConnectionStrings.GpConnectAnalytics);
        }

        public async Task<List<T>> ExecuteStoredProcedure<T>(string procedureName, DynamicParameters parameters) where T : class
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                try
                {
                    sqlConnection.InfoMessage += SqlConnection_InfoMessage;
                    _logger.LogInformation($"Executing stored procedure {procedureName}", parameters);
                    var results = await sqlConnection.QueryAsync<T>(procedureName, parameters, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 600);
                    return results.AsList();
                }
                catch (Exception exc)
                {
                    _logger?.LogError(exc, $"An error has occurred while attempting to execute the function {procedureName}");
                    throw;
                }
            }
        }

        public async Task<DynamicParameters> ExecuteStoredProcedureWithOutputParameters(string procedureName, DynamicParameters parameters)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                try
                {
                    sqlConnection.InfoMessage += SqlConnection_InfoMessage;
                    _logger.LogInformation($"Executing stored procedure {procedureName}", parameters);
                    await SqlMapper.ExecuteAsync(sqlConnection, procedureName, parameters, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 600);
                    return parameters;
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
            
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                try
                {
                    sqlConnection.InfoMessage += SqlConnection_InfoMessage;
                    _logger.LogInformation($"Executing stored procedure {procedureName}", parameters);
                    var result = await sqlConnection.ExecuteAsync(procedureName, parameters, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 600);
                    return result;
                }
                catch (Exception exc)
                {
                    _logger?.LogError(exc, $"An error has occurred while attempting to execute the function {procedureName}");
                    throw;
                }
            }
        }

        private void SqlConnection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            _logger?.LogInformation(e.Message);
        }
    }
}
