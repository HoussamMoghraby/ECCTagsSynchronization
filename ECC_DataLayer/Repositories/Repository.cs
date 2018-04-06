using Dapper;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECC_DataLayer.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        public async Task<IEnumerable<T>> GetAsync(string query, object arguments)
        {
            using (var connection = await ConnectionFactory.GetConnectionAsync())
                return await connection.QueryAsync<T>(query, arguments, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpdateAsync(string query, object arguments)
        {
            using (var connection = await ConnectionFactory.GetConnectionAsync())
                return await connection.ExecuteAsync(query, arguments, commandType: CommandType.StoredProcedure);
        }

        public async Task<T> GetSingleOrDefaultAsync(string query, object arguments)
        {
            using (var connection = await ConnectionFactory.GetConnectionAsync())
            {
                var result = await connection.QueryAsync<T>(query, arguments, commandType: CommandType.StoredProcedure);

                return result.FirstOrDefault();
            }
        }

        public async Task<int> ExecuteScalarAsync(string query, object arguments)
        {
            using (var connection = await ConnectionFactory.GetConnectionAsync())
                return await connection.ExecuteScalarAsync<int>(query, arguments, commandType: CommandType.StoredProcedure);
        }

        public async Task<string> ExecuteScalarAsyncStr(string query, object arguments)
        {
            using (var connection = await ConnectionFactory.GetConnectionAsync())
                return await connection.ExecuteScalarAsync<string>(query, arguments, commandType: CommandType.StoredProcedure);
        }

        public async Task<Guid> ExecuteScalarAsyncGuid(string query, object arguments)
        {
            using (var connection = await ConnectionFactory.GetConnectionAsync())
                return await connection.ExecuteScalarAsync<Guid>(query, arguments, commandType: CommandType.StoredProcedure);
        }

        public T GetSingleOrDefault(string query, object arguments)
        {
            using (var connection = new OracleConnection(ConnectionFactory.ConnectionString()))
            {
                var result = connection.Query<T>(query, arguments, commandType: CommandType.StoredProcedure);

                return result.FirstOrDefault();
            }
        }
    }
}
