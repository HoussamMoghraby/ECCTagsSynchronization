using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using ECC_DataLayer.Helpers;

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
            string _dataSource = ConfigurationSettings.AppSettings.Get("ECC_DB_DataSource");
            string _userId = ConfigurationSettings.AppSettings.Get("ECC_DB_UserId");
            string _password = CryptoProvider.Decrypt_Aes(ConfigurationSettings.AppSettings.Get("ECC_DB_Password"));

            string _connectionString = string.Format("Data Source={0};User Id={1};Password={2};", _dataSource, _userId, _password);

            return _connectionString;
        }
    }
}
