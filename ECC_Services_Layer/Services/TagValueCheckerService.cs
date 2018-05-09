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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ECC_AFServices_Layer.Services
{
    public class TagValueCheckerService : ECCServiceBase, IECCService
    {
        private TagValueCheckerStore _tagValueCheckerStore = new TagValueCheckerStore();
        private string _eccPIServerName = ConfigurationSettings.AppSettings.Get("ECC_PI_ServerName");
        private static AFTime _pointValueTime;


        private void SetReadingTime()
        {
            DateTime _readingTime = DateTime.Now.AddMinutes(-30);
            _readingTime = _readingTime.AddSeconds(-_readingTime.Second);
            _pointValueTime = new AFTime(_readingTime);
        }
        public async Task<bool> StartAsync()
        {
            SetReadingTime();
            try
            {
                //Get the tags created from oracle database
                Logger.Info(ServiceName, "Getting unchecked tags from database");
                IEnumerable<PITagValueCheckDataModel> tags = (await _tagValueCheckerStore.GetTagsForValueChecking());
                if (tags != null && tags.Count() > 0)
                {
                    //For each tag, get the area tag value at today midnight and from ECCPI server as well

                    //Values check on ECC Server
                    Logger.Info(ServiceName, "Querrying ECC PI server for tags by name");
                    IEnumerable<PIPoint> queryResult = FindPointsByName(_eccPIServerName, tags, queryECCServer: true);
                    Logger.Info(ServiceName, "Reading recorded value of each tag from ECC PI Server");
                    foreach (var result in queryResult)
                    {
                        AFValue _recordedValue = GetPointRecordedValue(result);
                        AssignRecordedValueToOriginalTagsCollection(tags: tags, tagName: result.Name, recordedValue: _recordedValue);
                    }
                    Logger.Info(ServiceName, "Done Reading");
                    //Values check on Area Servers
                    var grouppedTags = tags.GroupBy(t => t.PI_SERVER_NAME);

                    foreach (var group in grouppedTags)
                    {
                        try
                        {
                            IEnumerable<PIPoint> _areaQueryResult = FindPointsByName(group.Key, group, queryECCServer: false);
                            Logger.Info(ServiceName, string.Format("Reading {0} tags values", group.Key));
                            foreach (var result in _areaQueryResult)
                            {
                                AFValue _recordedValue = GetPointRecordedValue(result);
                                AssignRecordedValueToOriginalTagsCollection(tags: tags, tagName: result.Name, recordedValue: _recordedValue, isECCServerValue: false);
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Error(ServiceName, e);
                        }
                    }

                    //Update the tags status in oracle
                    await UpdateCheckedTagsAsync(tags);
                }
            }
            catch (Exception e)
            {
                Logger.Error(ServiceName, e);
                return false;
            }

            return true;
        }


        private void AssignRecordedValueToOriginalTagsCollection(IEnumerable<PITagValueCheckDataModel> tags, string tagName, AFValue recordedValue, bool isECCServerValue = true)
        {
            tags.Where(t => (t.ECCPI_EXST_TAG_NAME == tagName && isECCServerValue == true) || (t.AREA_PI_TAG_NAME == tagName && isECCServerValue == false)).ForEach(t =>
              {
                  dynamic _value = (recordedValue.IsGood) ? recordedValue.Value : recordedValue.Value.ToString();
                  if (isECCServerValue == true)
                      t.ECCServerValue = _value;
                  else
                      t.AreaServerValue = _value;
              });
            //return tags;
        }

        private async Task UpdateCheckedTagsAsync(IEnumerable<PITagValueCheckDataModel> tags)
        {
            Logger.Info(ServiceName, "Validating values collected");
            var checkedTags = tags.Where(t => (t.AreaServerValue != null && t.ECCServerValue != null));
            foreach (var tag in checkedTags)
            {
                //if the values are matching then update the status of the tag in oracle database ; otherwise => flag it as not flowing;
                string _matchingRemark = null;
                bool _isValuesMatching = false;
                var isECCNumber = Regex.IsMatch(tag.ECCServerValue.ToString(), @"\d");
                var isAreaNumber = Regex.IsMatch(tag.AreaServerValue.ToString(), @"\d");
                if (isECCNumber && isAreaNumber)
                    _isValuesMatching = Math.Round(tag.ECCServerValue, 2) == Math.Round(tag.AreaServerValue, 2);
                else if (!isECCNumber && !isAreaNumber)
                {
                    _isValuesMatching = tag.ECCServerValue.ToString() == tag.AreaServerValue.ToString();
                    if (_isValuesMatching)
                        _matchingRemark = string.Format("Matching with no value: {0}", tag.ECCServerValue.ToString());
                }
                char _matchingStatus = _isValuesMatching ? 'Y' : 'N';

                tag.IsValuesMatching = _isValuesMatching;
                tag.MatchingValuesRemark = _matchingRemark;
                var updateStatus = await _tagValueCheckerStore.UpdateTagMatchingStatus(tag.EAWFT_NUM, eccMatchingFlag: _matchingStatus, remark: _matchingRemark);
            }
            await _tagValueCheckerStore.Commit();
            Logger.Info(ServiceName, string.Format("{0} Tags Matched", checkedTags.Where(t => t.IsValuesMatching == true).Count()));
            Logger.Info(ServiceName, string.Format("{0} Tags Not Matched", checkedTags.Where(t => t.IsValuesMatching == false).Count()));
        }

        private IEnumerable<PIPoint> FindPointsByName(string serverName, IEnumerable<PITagValueCheckDataModel> tags, bool queryECCServer = true)
        {
            //if (serverName == "GPDPISRV")
            //    serverName = "ECC-PISRV1"; //TODO: Remove when done testing locally
            PIServer _piServer = PIAFUtils.GetPIServer(serverName);
            string _query = string.Empty;
            foreach (var tag in tags)
            {
                //TODO: Fix naming here when done testing locally to AREA_PI_TAG_NAME when queryECCServer = false
                string tagName = (queryECCServer == true) ? tag.ECCPI_EXST_TAG_NAME : tag.AREA_PI_TAG_NAME;
                if (string.IsNullOrEmpty(_query))
                    _query = "name:=\"" + tagName + "\"";
                else
                    _query += " OR name:=\"" + tagName + "\"";
            }
            //Find the tags on server
            IEnumerable<PIPoint> queryResult = PIPoint.FindPIPoints(_piServer, query: _query, searchNameAndDescriptor: false);
            return queryResult;
        }


        private AFValue GetPointRecordedValue(PIPoint tag)
        {
            AFValue _recordedValue = tag.RecordedValue(_pointValueTime, AFRetrievalMode.Auto);
            return _recordedValue;
        }
    }
}
