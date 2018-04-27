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
    public class TagCreatorStore : DataStore
    {
        private Repository<PITagDataModel> _tagCreatorRepo = new Repository<PITagDataModel>();

        public TagCreatorStore()
        {
            _tagCreatorRepo = new Repository<PITagDataModel>();
        }


        public async Task<IEnumerable<PITagDataModel>> GetTagsForCreation()
        {
            var result = await _tagCreatorRepo.GetAsync(QueryReader.ReadQuery("GetNonCreatedTags"), new { });
            return result;
        }


        public async Task<int> UpdateCreatedTag(long id, int? eccPointId, string remark, char eccCreationFlag = 'Y')
        {
            string _query = string.Format(QueryReader.ReadQuery("UpdateCreatedTag"),
                eccCreationFlag,
                (eccPointId.HasValue) ? string.Format(" ECCPI_POINT_ID = {0}, ", eccPointId.Value) : "",
                remark,
                id);
            var result = await _tagCreatorRepo.ExecuteScalarAsync(_query, new { });
            return result;
        }

    }
}
