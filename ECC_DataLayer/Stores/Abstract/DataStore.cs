using ECC_DataLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECC_DataLayer.Stores.Abstract
{
    public abstract class DataStore
    {
        public Repository<dynamic> _repo = new Repository<dynamic>();
        public async Task<int> Commit()
        {
            try
            {
                var result = await _repo.ExecuteScalarAsync("COMMIT", new { });
                return 1;
            }
            catch (Exception e)
            {

                return 0;
            }
        }

        public dynamic ResolveQueryParam(dynamic value)
        {
            if (value != null)
                return value;
            else
                return "null";
        }
    }
}
