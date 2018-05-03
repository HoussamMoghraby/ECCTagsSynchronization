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
    public class TagValueCheckerStore : DataStore
    {
        private Repository<PITagValueCheckDataModel> _tagCheckerRepo = new Repository<PITagValueCheckDataModel>();

        public TagValueCheckerStore()
        {
            _tagCheckerRepo = new Repository<PITagValueCheckDataModel>();
        }

        public async Task<IEnumerable<PITagValueCheckDataModel>> GetTagsForValueChecking()
        {
            var result = await _tagCheckerRepo.GetAsync(QueryReader.ReadQuery("GetCreatedTagsForValueChecking"), new { });
            return result;
        }

        public async Task<int> UpdateTagMatchingStatus(long id, char eccMatchingFlag = 'Y', string remark = null)
        {
            string _query = string.Format(QueryReader.ReadQuery("UpdateTagMatchingStatus"), eccMatchingFlag,
                (!string.IsNullOrEmpty(remark)) ? string.Format(" ,t.pi_tag_flow_rem = '{0}'", remark) : null,
                id);
            var result = await _tagCheckerRepo.ExecuteScalarAsync(_query, new { });
            return result;
        }

    }
}
