using ECC_AFServices_Layer.Services.Abstract;
using ECC_DataLayer.DataModels;
using ECC_DataLayer.Helpers;
using ECC_DataLayer.Stores;
using ECC_PIAFServices_Layer.Helpers;
using MoreLinq;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.PI;
using OSIsoft.AF.Time;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECC_AFServices_Layer.Services
{
    public class TagValueCheckerService : IECCService
    {
        private TagValueCheckerStore _tagValueCheckerStore = new TagValueCheckerStore();
        private string _eccPIServerName = ConfigurationSettings.AppSettings.Get("ECC_PI_ServerName");
        public async Task<bool> StartAsync()
        {
            AFTime _time = new AFTime(DateTime.Today);
            try
            {
                //Get the tags created from oracle database
                IEnumerable<PITagValueCheckDataModel> tags = await _tagValueCheckerStore.GetTagsForValueChecking();
                if (tags != null && tags.Count() > 0)
                {
                    //For each tag, get the area tag value at today midnight and from ECCPI server as well
                    PIServer _eccPIServer = PIAFUtils.GetPIServer(_eccPIServerName);
                    string _query = string.Empty;
                    foreach (var tag in tags)
                    {
                        if (string.IsNullOrEmpty(_query))
                            _query = "name:=\"" + tag.ECCPI_TAG_NAME + "\"";
                        else
                            _query += " OR name:=\"" + tag.ECCPI_TAG_NAME + "\"";
                    }


                    //Values check on ECC Server
                    IEnumerable<PIPoint> queryResult = PIPoint.FindPIPoints(_eccPIServer, query: _query, searchNameAndDescriptor: false);

                    foreach (var result in queryResult)
                    {
                        AFValue _recordedValue = result.RecordedValue(_time, AFRetrievalMode.Auto);
                        tags.Where(t => t.ECCPI_TAG_NAME == result.Name).ForEach(t =>
                        {
                            t.ECCServerValue = (_recordedValue.IsGood) ? _recordedValue.Value : _recordedValue.Value.ToString();
                        });

                    }

                    //Values check on Area Servers
                    var grouppedTags = tags.GroupBy(t => t.PI_SERVER_NAME);
                    foreach (var group in grouppedTags)
                    {
                        try
                        {
                            string _areaQuery = string.Empty;
                            foreach (var tag in group)
                            {
                                if (string.IsNullOrEmpty(_areaQuery))
                                    _areaQuery = "name:=\"" + tag.AREA_PI_TAG_NAME + "\"";
                                else
                                    _areaQuery += " OR name:=\"" + tag.AREA_PI_TAG_NAME + "\"";
                            }

                            PIServer _areaServer = PIAFUtils.GetPIServer(group.Key);
                            IEnumerable<PIPoint> _areaQueryResult = PIPoint.FindPIPoints(_areaServer, query: _areaQuery, searchNameAndDescriptor: false);
                            foreach (var result in _areaQueryResult)
                            {
                                AFValue _recordedValue = result.RecordedValue(_time, AFRetrievalMode.Auto);
                                tags.Where(t => t.AREA_PI_TAG_NAME == result.Name).ForEach(t =>
                                {
                                    t.AreaServerValue = (_recordedValue.IsGood) ? _recordedValue.Value : _recordedValue.Value.ToString();
                                });
                            }
                        }
                        catch (Exception e)
                        {

                            Logger.Error("ECCPITagValueChecker", e);
                        }
                    }

                    //Update the tags status in oracle
                    var checkedTags = tags.Where(t => (t.AreaServerValue != null && t.ECCServerValue != null));
                    foreach (var tag in checkedTags)
                    {
                        //if the values are matching then update the status of the tag in oracle database ; otherwise => flag it as not flowing;
                        char _matchingStatus = (tag.AreaServerValue == tag.ECCServerValue) ? 'Y' : 'N';
                        var updateStatus = await _tagValueCheckerStore.UpdateTagMatchingStatus(tag.EAWFT_NUM, eccMatchingFlag: _matchingStatus);
                    }
                    await _tagValueCheckerStore.Commit();
                    Logger.Info("ECCPITagValueChecker", string.Format("{0} Tags Matched", checkedTags.Where(t => t.AreaServerValue == t.ECCServerValue).Count()));
                    Logger.Info("ECCPITagValueChecker", string.Format("{0} Tags Not Matched", checkedTags.Where(t => t.AreaServerValue != t.ECCServerValue).Count()));
                }
            }
            catch (Exception e)
            {
                Logger.Error("ECCPITagValueChecker", e);
                return false;
            }

            return true;
        }
    }
}
