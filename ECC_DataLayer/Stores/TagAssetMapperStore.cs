using ECC_DataLayer.DataModels;
using ECC_DataLayer.Repositories;
using ECC_DataLayer.Stores.Abstract;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECC_DataLayer.Helpers;

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
            Logger.Info("TagAssetMapperStore", $"Function=GetUnmappedTags result={result}");
            return result;
        }

        public async Task<int> UpdateMappedTag(long id, string remark, char eccMappingFlag = 'Y', DateTime? mappingDate = null)
        {
            string _query = string.Format(QueryReader.ReadQuery("UpdateMappedTag"), eccMappingFlag, remark,
                (mappingDate.HasValue) ? string.Format(" , ECCPI_AF_MAP_DT = to_date('{0}', 'mm/dd/yyyy hh24:mi:ss') ", mappingDate.Value.ToString("MM/dd/yyyy HH:mm:ss")) : ""
                , id);
            var result = await _tagMapperRepo.ExecuteScalarAsync(_query, new { });
            Logger.Info("TagAssetMapperStore", $"Function=UpdateMappedTag result={result}");
            return result;
        }
    }
}