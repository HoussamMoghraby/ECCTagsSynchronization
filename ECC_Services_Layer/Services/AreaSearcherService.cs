using ECC_AFServices_Layer.Helpers;
using ECC_AFServices_Layer.Services.Abstract;
using ECC_DataLayer.DataModels;
using ECC_DataLayer.Helpers;
using ECC_DataLayer.Stores;
using ECC_PIAFServices_Layer.Helpers;
using OSIsoft.AF.PI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace ECC_PIAFServices_Layer.Services
{
    public class AreaSearcherService : ECCServiceBase, IECCService
    {
        private AreaSearcherStore _areaStore = new AreaSearcherStore();
        private string _eccPIServerName = ConfigurationSettings.AppSettings.Get("ECC_PI_ServerName");

        public async Task<bool> StartAsync()
        {
            try
            {
                //Get the tag masks from ECCPI_AF_WELL_EQP_TAG_MASKS table that were not processed
                IEnumerable<AreaPIServer> areas = await _areaStore.GetAreasPIServers();
                //Get all the tags created on each server area the area last pull date
                foreach (var area in areas)
                {
                    try
                    {
                        PIServer piServer = PIAFUtils.GetPIServer(area.PI_SERVER_NAME); //TODO: return the area.PI_SERVER_NAME param
                        string _query = string.Format("CreationDate:>\"{0}\" OR ChangeDate:>\"{0}\"", (area.PI_LAST_TAG_PULL_DT.HasValue ? area.PI_LAST_TAG_PULL_DT.Value.ToString("yyyy-MM-dd HH:mm:ss") : DateTime.Today.ToString("yyyy-MM-dd 00:00:00")));

                        IEnumerable<PIPoint> _piPoints = QueryPIPoints(piServer, _query);
                        await InsertPIPointsAsync(_piPoints, sourcePIServerCode: area.PI_SERVER_CD);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(ServiceName, e);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error(ServiceName, e);
                return false;
            }
        }


        private IEnumerable<PIPoint> QueryPIPoints(PIServer piServer, string querySyntax)
        {
            IEnumerable<PIPoint> queryResult = PIPoint.FindPIPoints(piServer, query: querySyntax, searchNameAndDescriptor: false, attributeNames: new List<string>() { PICommonPointAttributes.Descriptor });
            return queryResult;
        }


        private async Task InsertPIPointsAsync(IEnumerable<PIPoint> points, string sourcePIServerCode)
        {
            int _tagsInserted = 0;
            foreach (var point in points)
            {
                //Store the results found in the ECCPI_AF_WELL_FOUND_TAGS table
                //TODO: change the tag descriptor
                if (!string.IsNullOrEmpty(point.Name))
                {
                    var _tag = point.MapToPITagDataModel(sourcePIServerCode);
                    var insertPITag = await _areaStore.InsertAreaTags(_tag);

                    if (insertPITag == 1)
                        _tagsInserted++;
                }
            }

            Logger.Info(ServiceName, string.Format("{0} was inserted for area {1}", _tagsInserted, sourcePIServerCode));
            //Commit oracle changes after each batch of requests
            await _areaStore.Commit();

            //update the last pull date in ECCPI_SERVERS_LIST
            var updateServiceLastPullDate = await _areaStore.UpdatePIServerLastPullDate(sourcePIServerCode);
        }
    }
}
