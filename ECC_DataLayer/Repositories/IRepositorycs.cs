using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECC_DataLayer.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAsync(string query, object arguments);
        Task<int> UpdateAsync(string query, object arguments);
        Task<T> GetSingleOrDefaultAsync(string query, object arguments);
        Task<int> ExecuteScalarAsync(string query, object arguments);
        Task<string> ExecuteScalarAsyncStr(string query, object arguments);
        Task<Guid> ExecuteScalarAsyncGuid(string query, object arguments);
        T GetSingleOrDefault(string query, object arguments);
        int InsertAndReturnId(string query);
    }
}
