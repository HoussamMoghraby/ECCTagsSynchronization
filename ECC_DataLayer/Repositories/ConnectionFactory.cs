using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECC_DataLayer.Repositories
{
    public class ConnectionFactory
    {
        public static async Task<IDbConnection> GetConnectionAsync()
        {
            var connection = new OracleConnection(ConnectionString());
            await connection.OpenAsync();
            return connection;
        }

        public static IDbConnection GetConnection()
        {
            var connection = new OracleConnection(ConnectionString());
            connection.Open();
            return connection;
        }

        public static string ConnectionString()
        {
            return Settings1.Default.ECCDBConnectionString;
        }
    }
}
