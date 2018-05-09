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

        public async Task<bool> StartAsync()
        {
            // Set reading time to be used for comparison
            SetReadingTime();
            try
            {
                Logger.Info(ServiceName, "Getting unchecked tags from database");

                // Get the tags created from oracle database
                IEnumerable<PITagValueCheckDataModel> tags = await _tagValueCheckerStore.GetTagsForValueChecking();
                if (tags != null && tags.Count() > 0)
                {
                    Logger.Info(ServiceName, "Querrying ECC PI server for tags by name");

                    // Find the tags names in the central PI server
                    IEnumerable<PIPoint> queryResult = FindPointsByName(_eccPIServerName, tags, queryECCServer: true);
                    Logger.Info(ServiceName, "Reading recorded value of each tag from ECC PI Server");

                    // Itterate the result
                    foreach (var result in queryResult)
                    {
                        // Get the recorded value at the time
                        AFValue _recordedValue = GetPointRecordedValue(result);
                        // Assign the retreived value to the tags list object
                        AssignRecordedValueToOriginalTagsCollection(tags: tags, tagName: result.Name, recordedValue: _recordedValue);
                    }
                    Logger.Info(ServiceName, "Done Reading");

                    /// Values check on Area Servers

                    // Group the tags by server name
                    var grouppedTags = tags.GroupBy(t => t.PI_SERVER_NAME);

                    // Itterate 
                    foreach (var group in grouppedTags)
                    {
                        try
                        {
                            // Find the tags names in the area PI server
                            IEnumerable<PIPoint> _areaQueryResult = FindPointsByName(group.Key, group, queryECCServer: false);
                            Logger.Info(ServiceName, string.Format("Reading {0} tags values", group.Key));

                            // Itterate the result
                            foreach (var result in _areaQueryResult)
                            {
                                // Get the recorded value at the time
                                AFValue _recordedValue = GetPointRecordedValue(result);
                                // Assign the retreived value to the tags list object
                                AssignRecordedValueToOriginalTagsCollection(tags: tags, tagName: result.Name, recordedValue: _recordedValue, isECCServerValue: false);
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Error(ServiceName, e);
                        }
                    }

                    //Update the tags status in oracle based on the ECC value and area value
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


        /// <summary>
        /// Set reading time to be used for comparison
        /// </summary>
        private void SetReadingTime()
        {
            // Set time 30 mins earlier than now
            DateTime _readingTime = DateTime.Now.AddMinutes(-30);
            _readingTime = _readingTime.AddSeconds(-_readingTime.Second);
            _pointValueTime = new AFTime(_readingTime);
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

        /// <summary>
        /// Checks the values of the tags in ECC and in Area servers and update the status of each one 
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        private async Task UpdateCheckedTagsAsync(IEnumerable<PITagValueCheckDataModel> tags)
        {
            Logger.Info(ServiceName, "Validating values collected");

            // Valid tags are those who has values from main and area servers
            var checkedTags = tags.Where(t => (t.AreaServerValue != null && t.ECCServerValue != null));

            // Itterate result
            foreach (var tag in checkedTags)
            {
                /// if the values are matching then update the status of the tag in oracle database ; otherwise => flag it as not flowing;
                string _matchingRemark = null;
                bool _isValuesMatching = false;

                // Check if the values are numbers
                var isECCNumber = Regex.IsMatch(tag.ECCServerValue.ToString(), @"\d");
                var isAreaNumber = Regex.IsMatch(tag.AreaServerValue.ToString(), @"\d");

                // Values are numbers => then compare the rounded values
                if (isECCNumber && isAreaNumber)
                    _isValuesMatching = Math.Round(tag.ECCServerValue, 2) == Math.Round(tag.AreaServerValue, 2);
                // Not numbers => compare the string values
                else if (!isECCNumber && !isAreaNumber)
                {
                    _isValuesMatching = tag.ECCServerValue.ToString() == tag.AreaServerValue.ToString();

                    // if values are matching => set the matching remark text
                    if (_isValuesMatching)
                        _matchingRemark = string.Format("Matching with no value: {0}", tag.ECCServerValue.ToString());
                }
                char _matchingStatus = _isValuesMatching ? 'Y' : 'N';

                tag.IsValuesMatching = _isValuesMatching;
                tag.MatchingValuesRemark = _matchingRemark;

                // Update the tag status in oracle database
                var updateStatus = await _tagValueCheckerStore.UpdateTagMatchingStatus(tag.EAWFT_NUM, eccMatchingFlag: _matchingStatus, remark: _matchingRemark);
            }
            // Commit changes
            await _tagValueCheckerStore.Commit();

            Logger.Info(ServiceName, string.Format("{0} Tags Matched", checkedTags.Where(t => t.IsValuesMatching == true).Count()));
            Logger.Info(ServiceName, string.Format("{0} Tags Not Matched", checkedTags.Where(t => t.IsValuesMatching == false).Count()));
        }


        /// <summary>
        /// Find the PI points by name in a specific PI server
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="tags"></param>
        /// <param name="queryECCServer"></param>
        /// <returns></returns>
        private IEnumerable<PIPoint> FindPointsByName(string serverName, IEnumerable<PITagValueCheckDataModel> tags, bool queryECCServer = true)
        {
            PIServer _piServer = PIAFUtils.GetPIServer(serverName);
            string _query = string.Empty;
            foreach (var tag in tags)
            {
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
