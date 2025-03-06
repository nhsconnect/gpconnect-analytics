using System.Data.Common;
using Core;
using Microsoft.Data.SqlClient;

namespace Functions;

public class SqlConnectionFactory : IConnectionFactory
{
    public DbConnection CreateConnection(string connectionString)
    {
        return new SqlConnection(connectionString);
    }
}