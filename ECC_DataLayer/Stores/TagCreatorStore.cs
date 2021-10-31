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
        private Repository<PointSourceDataModel> _pointSourceRepo = new Repository<PointSourceDataModel>();

        public TagCreatorStore()
        {
            _tagCreatorRepo = new Repository<PITagDataModel>();
            _pointSourceRepo = new Repository<PointSourceDataModel>();
        }


        public async Task<IEnumerable<PITagDataModel>> GetTagsForCreation()
        {
            var result = await _tagCreatorRepo.GetAsync(QueryReader.ReadQuery("GetNonCreatedTags"), new { });
            return result;
        }


        public async Task<int> UpdateCreatedTag(long id, int? eccPointId, string remark, char eccCreationFlag = 'Y', DateTime? creationDate = null)
        {
            //creationDate = creationDate.HasValue ? creationDate : null;

            string _query = string.Format(QueryReader.ReadQuery("UpdateCreatedTag"),
                eccCreationFlag,
                (eccPointId.HasValue) ? string.Format(" ECCPI_POINT_ID = {0}, ", eccPointId.Value) : "",
                remark,
                (creationDate.HasValue) ? string.Format(" , ECCPI_TAG_CRE_DT = to_date('{0}', 'mm/dd/yyyy hh24:mi:ss') ", creationDate.Value.ToString("MM/dd/yyyy HH:mm:ss")) : "",
                (eccCreationFlag == 'Y') ? " , eccpi_exst_tag_name = eccpi_tag_name " : "",
                id);
            var result = await _tagCreatorRepo.ExecuteScalarAsync(_query, new { });
            return result;
        }

        public async Task<int> UpdateModifiedTag(long id, int? eccPointId, string remark, char eccCreationFlag = 'Y', DateTime? modifiedDate = null)
        {
            string _query = string.Format(QueryReader.ReadQuery("UpdateModifiedTag"),
                eccCreationFlag,
                (eccPointId.HasValue) ? string.Format(" ECCPI_POINT_ID = {0}, ", eccPointId.Value) : "",
                remark,
                (eccCreationFlag == 'Y') ? " , eccpi_exst_tag_name = eccpi_tag_name " : "",
                id);
            var result = await _tagCreatorRepo.ExecuteScalarAsync(_query, new { });
            return result;
        }


        public async Task<int> UpdateExistingTag(long id, string existingTagName, bool renamedInPIServerFlag = false)
        {
            string _query = string.Format(QueryReader.ReadQuery("UpdateExistingTag"),
                existingTagName,
                "Y",
                "Instrument tag already exist",
                (renamedInPIServerFlag == true) ? " ,ECCPI_TAG_HAS_REN_FLG = 'Y' " : null,
                id
                );
            var result = await _tagCreatorRepo.ExecuteScalarAsync(_query, new { });
            return result;
        }


        public async Task<PointSourceDataModel> GetServerPointSource(string serverCode)
        {
            string _query = string.Format(QueryReader.ReadQuery("GetServerPointSource"), serverCode);
            var result = await _pointSourceRepo.GetAsync(_query, new { });
            return result.FirstOrDefault();
        }


        public async Task<int> UpdatePointSource(long id, long numberOfTags)
        {
            string _query = string.Format(QueryReader.ReadQuery("UpdatePointSource"),
                numberOfTags,
                id);
            var result = await _tagCreatorRepo.ExecuteScalarAsync(_query, new { });
            return result;
        }


    }
}
