using Core.Helpers;
using Core.Services.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace function_app.Services
{
    public class DataService(ILogger<DataService> logger, ICoreConfigurationService coreConfigurationService)
        : IDataService
    {
        private readonly string _connectionString =
            coreConfigurationService.GetConnectionString(ConnectionStrings.GpConnectAnalytics);

        public async Task<int> ExecuteRawUpsertSqlAsync(string sqlCommand, object parameters)
        {
            try
            {
                await using var sqlConnection = new SqlConnection(_connectionString);
                await sqlConnection.OpenAsync();
                logger.LogInformation($"Executing raw SQL command");
                var rowsAffected = await sqlConnection.ExecuteAsync(sqlCommand, parameters);
                return rowsAffected;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error has occurred while executing the raw SQL command: {sqlCommand}");
                throw;
            }
        }

        public async Task<List<T>> ExecuteStoredProcedure<T>(string procedureName, DynamicParameters parameters)
            where T : class
        {
            await using var sqlConnection = new SqlConnection(_connectionString);
            try
            {
                sqlConnection.InfoMessage += SqlConnection_InfoMessage;
                logger.LogInformation($"Executing stored procedure {procedureName}", parameters);
                var results = await sqlConnection.QueryAsync<T>(procedureName, parameters,
                    commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
                return results.AsList();
            }
            catch (Exception exc)
            {
                logger.LogError(exc,
                    $"An error has occurred while attempting to execute the function {procedureName}");
                throw;
            }
        }

        public async Task<DynamicParameters> ExecuteStoredProcedureWithOutputParameters(string procedureName,
            DynamicParameters parameters)
        {
            await using var sqlConnection = new SqlConnection(_connectionString);
            try
            {
                sqlConnection.InfoMessage += SqlConnection_InfoMessage;
                logger.LogInformation($"Executing stored procedure {procedureName}", parameters);
                await sqlConnection.ExecuteAsync(procedureName, parameters,
                    commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
                return parameters;
            }
            catch (Exception exc)
            {
                logger?.LogError(exc,
                    $"An error has occurred while attempting to execute the function {procedureName}");
                throw;
            }
        }

        public async Task<int> ExecuteStoredProcedure(string procedureName, DynamicParameters parameters)
        {
            await using var sqlConnection = new SqlConnection(_connectionString);
            try
            {
                sqlConnection.InfoMessage += SqlConnection_InfoMessage;
                logger.LogInformation($"Executing stored procedure {procedureName}", parameters);
                var result = await sqlConnection.ExecuteAsync(procedureName, parameters,
                    commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 0);
                return result;
            }
            catch (Exception exc)
            {
                logger?.LogError(exc,
                    $"An error has occurred while attempting to execute the function {procedureName}");
                throw;
            }
        }

        private void SqlConnection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            logger?.LogInformation(e.Message);
        }
    }
}