using ECC_DataLayer.DataModels;
using ECC_DataLayer.Helpers;
using ECC_DataLayer.Repositories;
using ECC_DataLayer.Stores.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECC_DataLayer.Stores
{
    public class AreaSearcherStore : DataStore
    {
        private Repository<AreaPIServer> _areaRepo = new Repository<AreaPIServer>();

        public AreaSearcherStore()
        {
            _areaRepo = new Repository<AreaPIServer>();
        }

        public async Task<IEnumerable<AreaPIServer>> GetAreasPIServers()
        {
            try
            {
                return await _areaRepo.GetAsync(QueryReader.ReadQuery("GetAreasPIServers"), new { });
            }
            catch (Exception e)
            {
                Logger.Error("Area Searcher Service", e);
                throw e;
            }
        }

        public async Task<int> InsertAreaTags(PITagDataModel tag)
        {
            try
            {
                string _query = string.Format(QueryReader.ReadQuery("InsertAreaTags"),
                    tag.AREA_PI_TAG_NAME,
                    tag.PI_TAG_DESCRIPTOR,
                    tag.SRC_PI_SERVER_CD,
                    tag.AREA_POINT_ID,

                    tag.ENGUNITS,
                    tag.ECCPI_DIGITAL_SET,
                    tag.POINTTYPE,
                    tag.LOCATION2,
                    tag.LOCATION3,
                    tag.LOCATION5,
                    tag.USERINT1,
                    tag.USERINT2,
                    tag.USERREAL1,
                    tag.USERREAL2,
                    tag.COMPRESSING,
                    tag.COMPDEV,
                    tag.COMPMAX,
                    tag.COMPMIN,
                    tag.COMPDEVPERCENT,
                    tag.EXCDEV,
                    tag.EXCMAX,
                    tag.EXCMIN,
                    tag.EXCDEVPERCENT,
                    tag.SPAN,
                    tag.STEP,
                    tag.TYPICALVALUE,
                    tag.ZERO);
                var result = await _repo.ExecuteScalarAsync(_query, new { });
                //Logger.Info("Area Searcher Service", "InsertAreaTags() successful");
                return 1;
            }
            catch (Exception e)
            {
                Logger.Error("Area Searcher Service", e);
                await Commit();
                return 0;
            }
        }

        public async Task<int> UpdatePIServerLastPullDate(string piServerCode)
        {
            try
            {
                string _query = string.Format(QueryReader.ReadQuery("UpdatePIServerLastPullDate"), piServerCode);
                var result = await _repo.ExecuteScalarAsync(_query, new { });
                //Logger.Info("Area Searcher Service", "UpdatePIServerLastPullDate() successful");
                await Commit();
                return 1;
            }
            catch (Exception e)
            {
                Logger.Error("Area Searcher Service", e);
                return 0;
            }
        }
    }
}
