using System.Data.Common;

namespace Core;

public interface IConnectionFactory
{
    DbConnection CreateConnection(string connectionString);
}