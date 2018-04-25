using ECC_DataLayer.DataModels;
using ECC_DataLayer.Repositories;
using ECC_DataLayer.Stores.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECC_DataLayer.Stores
{
    public class TagAssetMapperStore : DataStore
    {
        private Repository<PITagDataModel> _tagMapperRepo = new Repository<PITagDataModel>();

        public TagAssetMapperStore()
        {
            _tagMapperRepo = new Repository<PITagDataModel>();
        }


        public async Task<IEnumerable<PITagDataModel>> GetUnmappedTags()
        {
            var result = await _tagMapperRepo.GetAsync(QueryReader.ReadQuery("GetUnmappedTags"), new { });
            return result;
        }

        public async Task<int> UpdateMappedTag(long id, string remark, char eccMappingFlag = 'Y')
        {
            string _query = string.Format(QueryReader.ReadQuery("UpdateMappedTag"), eccMappingFlag, remark, id);
            var result = await _tagMapperRepo.ExecuteScalarAsync(_query, new { });
            return result;
        }

    }
}
