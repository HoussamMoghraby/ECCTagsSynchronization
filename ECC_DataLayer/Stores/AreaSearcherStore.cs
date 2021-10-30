﻿using ECC_DataLayer.DataModels;
using ECC_DataLayer.Helpers;
using ECC_DataLayer.Repositories;
using ECC_DataLayer.Stores.Abstract;
using System;
using System.Collections.Generic;
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
            return await _areaRepo.GetAsync(QueryReader.ReadQuery("GetAreasPIServers"), new { });
        }

        public async Task<int> MergeAreaTags(PITagDataModel tag)
        {
            try
            {
                string _query = string.Format(QueryReader.ReadQuery("MergeAreaTags"),
                    ResolveQueryParam(tag.AREA_PI_TAG_NAME),
                    ResolveQueryParam(tag.PI_TAG_DESCRIPTOR),
                    ResolveQueryParam(tag.SRC_PI_SERVER_CD),
                    ResolveQueryParam(tag.AREA_POINT_ID),

                    ResolveQueryParam(tag.ENGUNITS),
                    ResolveQueryParam(tag.AREA_DIGITAL_SET),
                    ResolveQueryParam(tag.POINTTYPE),
                    ResolveQueryParam(tag.LOCATION2),
                    ResolveQueryParam(tag.LOCATION3),
                    ResolveQueryParam(tag.LOCATION5),
                    ResolveQueryParam(tag.USERINT1),
                    ResolveQueryParam(tag.USERINT2),
                    ResolveQueryParam(tag.USERREAL1),
                    ResolveQueryParam(tag.USERREAL2),
                    ResolveQueryParam(tag.COMPRESSING),
                    ResolveQueryParam(tag.COMPDEV),
                    ResolveQueryParam(tag.COMPMAX),
                    ResolveQueryParam(tag.COMPMIN),
                    ResolveQueryParam(tag.COMPDEVPERCENT),
                    ResolveQueryParam(tag.EXCDEV),
                    ResolveQueryParam(tag.EXCMAX),
                    ResolveQueryParam(tag.EXCMIN),
                    ResolveQueryParam(tag.EXCDEVPERCENT),
                    ResolveQueryParam(tag.SPAN),
                    ResolveQueryParam(tag.STEP),
                    ResolveQueryParam(tag.TYPICALVALUE),
                    ResolveQueryParam(tag.ZERO));
                var result = await _repo.ExecuteScalarAsync(_query, new { });
                Logger.Info("AreaSearcherService", $"MergeAreaTags() result{result}");
                return 1;
            }
            catch (Exception e)
            {
                Logger.Error("AreaSearcherService", e);
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
                Logger.Info("AreaSearcherService", $"UpdatePIServerLastPullDate() result{result}");
                await Commit();
                return 1;
            }
            catch (Exception e)
            {
                Logger.Error("AreaSearcherService", e);
                return 0;
            }
        }
    }
}
