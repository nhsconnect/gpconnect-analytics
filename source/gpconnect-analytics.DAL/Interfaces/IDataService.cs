using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL.Interfaces
{
    public interface IDataService
    {
        Task<List<T>> ExecuteStoredProcedure<T>(string procedureName, DynamicParameters parameters = null) where T : class;
        Task<int> ExecuteStoredProcedure(string procedureName, DynamicParameters parameters);
    }
}
