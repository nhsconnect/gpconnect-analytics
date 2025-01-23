using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace Core;

public interface IConnectionFactory
{
    DbConnection CreateConnection(string connectionString);
}