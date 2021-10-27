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
        private readonly DbLoggerDetails _dbLoggerDetails;

        public TagAssetMapperStore()
        {
            _tagMapperRepo = new Repository<PITagDataModel>();
            _dbLoggerDetails = new DbLoggerDetails("ECCPITagAssetMapper_Service");
        }

        public async Task<IEnumerable<PITagDataModel>> GetUnmappedTags()
        {
            var result = await _tagMapperRepo.GetAsync(QueryReader.ReadQuery("GetUnmappedTags"), new { });
            Logger.Info("TagAssetMapperStore", $"Function=GetUnmappedTags result={result}");
            return result;
        }

        public async Task<int> UpdateMappedTag(long id, string remark, int areaPointId, string areaPiTagName, string eccpiTagName, string srcPiServerCd, char eccMappingFlag = 'Y', DateTime? mappingDate = null)
        {
            string _query = string.Format(QueryReader.ReadQuery("UpdateMappedTag"), eccMappingFlag, remark,
                (mappingDate.HasValue) ? string.Format(" , ECCPI_AF_MAP_DT = to_date('{0}', 'mm/dd/yyyy hh24:mi:ss') ", mappingDate.Value.ToString("MM/dd/yyyy HH:mm:ss")) : ""
                , id);
            var result = await _tagMapperRepo.ExecuteScalarAsync(_query, new { });
            Logger.Info("TagAssetMapperStore", $"Function=UpdateMappedTag result={result}");
            _dbLoggerDetails.Log(new DbLoggerDetailsDataModel
            {
                EAWFT_NUM = id,
                ECCPI_TAG_NAME = eccpiTagName,
                AREA_PI_TAG_NAME = areaPiTagName,
                SRC_PI_SERVER_CD = srcPiServerCd,
                SVC_MSG = "Update Mapped Tag",
                SVC_MSG_TYP = svcType.Tag,
                SVC_MSG_SEVIRITY = result == 1 ? Severity.Information : Severity.Error,
                AREA_POINT_ID = areaPointId,
                SVC_PROPOSED_REMEDY = result == 1 ? "N" : "Please check your connection to database server"
            });
            return result;
        }
    }
}