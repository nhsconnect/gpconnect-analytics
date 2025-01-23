using System.Data;
using Dapper;

namespace Core.Repositories;

public interface IDapperWrapper
{
    Task<int> ExecuteAsync(IDbConnection connection, string sql, object param = null,
        IDbTransaction transaction = null);

    Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql, object param = null,
        IDbTransaction transaction = null);

    Task<int> ExecuteStoredProcedureAsync<T>(IDbConnection connection, string procedureName,
        object parameters, int commandTimeout = 30);

    Task<IEnumerable<T>> QueryStoredProcedureAsync<T>(IDbConnection connection, string procedureName,
        object parameters, int commandTimeout = 30);
}

public class DapperWrapper : IDapperWrapper
{
    public async Task<int> ExecuteAsync(IDbConnection connection, string sql, object param = null,
        IDbTransaction transaction = null)
    {
        return await connection.ExecuteAsync(sql, param, transaction);
    }

    public async Task<int> ExecuteStoredProcedureAsync<T>(IDbConnection connection, string procedureName,
        object parameters, int commandTimeout = 30)
    {
        try
        {
            return await connection.ExecuteAsync(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: commandTimeout);
        }
        catch (Exception ex)
        {
            // More detailed error logging
            throw new Exception($"Error executing stored procedure: {procedureName}", ex);
        }
    }

    public async Task<IEnumerable<T>> QueryStoredProcedureAsync<T>(IDbConnection connection, string procedureName,
        object parameters, int commandTimeout = 30)
    {
        try
        {
            return await connection.QueryAsync<T>(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: commandTimeout);
        }
        catch (Exception ex)
        {
            // Handle exception (logging, rethrowing, etc.)
            throw new Exception("Error executing stored procedure.", ex);
        }
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql, object param = null,
        IDbTransaction transaction = null)
    {
        return await connection.QueryAsync<T>(sql, param, transaction);
    }
}