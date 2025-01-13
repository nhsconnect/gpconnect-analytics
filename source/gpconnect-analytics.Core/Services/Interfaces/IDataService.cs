using Dapper;

namespace Core.Services.Interfaces
{
    public interface IDataService
    {
        Task<List<T>> ExecuteStoredProcedure<T>(string procedureName, DynamicParameters parameters = null)
            where T : class;

        Task<DynamicParameters> ExecuteStoredProcedureWithOutputParameters(string procedureName,
            DynamicParameters parameters);

        Task<int> ExecuteStoredProcedure(string procedureName, DynamicParameters parameters = null);
    }
}