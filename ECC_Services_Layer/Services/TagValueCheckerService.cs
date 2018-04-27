﻿using ECC_AFServices_Layer.Services.Abstract;
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
        private static AFTime _pointValueTime = new AFTime(DateTime.Today);

        public async Task<bool> StartAsync()
        {
            try
            {
                //Get the tags created from oracle database
                IEnumerable<PITagValueCheckDataModel> tags = await _tagValueCheckerStore.GetTagsForValueChecking();
                if (tags != null && tags.Count() > 0)
                {
                    //For each tag, get the area tag value at today midnight and from ECCPI server as well

                    //Values check on ECC Server
                    IEnumerable<PIPoint> queryResult = FindPointsByName(_eccPIServerName, tags, queryECCServer: true);
                    foreach (var result in queryResult)
                    {
                        AFValue _recordedValue = GetPointRecordedValue(result);
                        AssignRecordedValueToOriginalTagsCollection(tags: tags, tagName: result.Name, recordedValue: _recordedValue);
                    }
                    //Values check on Area Servers
                    var grouppedTags = tags.GroupBy(t => t.PI_SERVER_NAME);
                    foreach (var group in grouppedTags)
                    {
                        try
                        {
                            IEnumerable<PIPoint> _areaQueryResult = FindPointsByName(group.Key, group, queryECCServer: false);
                            foreach (var result in _areaQueryResult)
                            {
                                AFValue _recordedValue = GetPointRecordedValue(result);
                                AssignRecordedValueToOriginalTagsCollection(tags: tags, tagName: result.Name, recordedValue: _recordedValue, isECCServerValue: false);
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Error("ECCPITagValueChecker", e);
                        }
                    }

                    //Update the tags status in oracle
                    await UpdateCheckedTagsAsync(tags);
                }
            }
            catch (Exception e)
            {
                Logger.Error("ECCPITagValueChecker", e);
                return false;
            }

            return true;
        }


        private void AssignRecordedValueToOriginalTagsCollection(IEnumerable<PITagValueCheckDataModel> tags, string tagName, AFValue recordedValue, bool isECCServerValue = true)
        {
            tags.Where(t => (t.ECCPI_TAG_NAME == tagName && isECCServerValue == true) || (t.AREA_PI_TAG_NAME == tagName && isECCServerValue == false)).ForEach(t =>
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

        private IEnumerable<PIPoint> FindPointsByName(string serverName, IEnumerable<PITagValueCheckDataModel> tags, bool queryECCServer = true)
        {
            //if (serverName == "GPDPISRV")
            //    serverName = "ECC-PISRV1"; //TODO: Remove when done testing locally
            PIServer _piServer = PIAFUtils.GetPIServer(serverName);
            string _query = string.Empty;
            foreach (var tag in tags)
            {
                //TODO: Fix naming here when done testing locally to AREA_PI_TAG_NAME when queryECCServer = false
                string tagName = (queryECCServer == true) ? tag.ECCPI_TAG_NAME : tag.AREA_PI_TAG_NAME;
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