using ECC_AFServices_Layer.Helpers;
using ECC_AFServices_Layer.Services.Abstract;
using ECC_DataLayer.Helpers;
using ECC_DataLayer.Stores;
using ECC_PIAFServices_Layer.Helpers;
using OSIsoft.AF;
using OSIsoft.AF.PI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using MoreLinq;
using System.Threading.Tasks;
using ECC_DataLayer.DataModels;

namespace ECC_AFServices_Layer.Services
{
    public class TagCreatorService : IECCService
    {
        private TagCreatorStore _tagCreatorStore = new TagCreatorStore();
        private string _eccPIServerName = ConfigurationSettings.AppSettings.Get("ECC_PI_ServerName");
        private PIServer piServer;
        private List<PointSourceDataModel> _areasPointSources = new List<PointSourceDataModel>();

        public TagCreatorService()
        {
            piServer = PIAFUtils.GetPIServer(_eccPIServerName);
        }

        public async Task<bool> StartAsync()
        {
            try
            {
                var tags = await _tagCreatorStore.GetTagsForCreation();
                if (tags != null && tags.Count() > 0)
                {
                    //PIServer piServer = PIAFUtils.GetPIServer(_eccPIServerName);
                    //Collect all the tags basic attributes required for insertion
                    //var _pointsDefinitions = tags.MapToPIPointDefinition(withDescriptor: true);
                    //TODOto be implementedAdd the pointsrouce, location1 and location4 attributes gotten from ECCPI_POINT_SOURCES table (search for point source having the least number of tags)
                    try
                    {
                        tags = tags.DistinctBy(t => t.ECCPI_TAG_NAME);

                        await CheckForExistingTagsAsync(tags);

                        //Insert the tags in bulk
                        await CreateTagsAsync(tags);
                    }
                    catch (Exception e)
                    {
                        Logger.Error("ECCPITagCreator", e);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Logger.Error("ECCPITagCreator", e);
                return false;
            }
        }

        private async Task CreateTagsAsync(IEnumerable<PITagDataModel> tags)
        {
            //Initialy filter the list and screen the already created tags
            tags = tags.Where(t => t.ECCPI_TAG_CRE_FLG != "Y");

            var _pointsDefinitions = await GetPointsDefinitions(tags);

            AFListResults<string, PIPoint> _insertResult = piServer.CreatePIPoints(_pointsDefinitions);

            // TODO: Update the duplicates occurs before the insertion

            //Receive results, and update the status of each inserted tag
            if (_insertResult != null && _insertResult.Results != null && _insertResult.Results.Count() > 0)
            {
                IList<PIPoint> _successResults = _insertResult.Results;
                var successTags = tags.Where(t => _successResults.Where(sr => sr.Name == t.ECCPI_TAG_NAME).FirstOrDefault() != null && t.ECCPI_TAG_NAME == _successResults.Where(sr => sr.Name == t.ECCPI_TAG_NAME).FirstOrDefault().Name);

                if (_insertResult.HasErrors)
                {
                    successTags = successTags.Where(st => !_insertResult.Errors.Select(e => e.Key).Contains(st.ECCPI_TAG_NAME));
                }

                //Fill ECCPI_POINT_ID in successfull tags to be updating in oracle db
                successTags.ForEach(st =>
                {
                    st.ECCPI_POINT_ID = _successResults.Where(sr => sr.Name == st.ECCPI_TAG_NAME).FirstOrDefault().ID;
                });

                foreach (var tag in successTags)
                {
                    var updateStatus = await _tagCreatorStore.UpdateCreatedTag(tag.EAWFT_NUM, tag.ECCPI_POINT_ID, string.Format("Tag Created Successfully in {0}", _eccPIServerName), 'Y');
                }
                Logger.Info("ECCPITagCreator", string.Format("{0} Inserted Tags", (successTags != null) ? successTags.Count() : 0));
                await _tagCreatorStore.Commit();

                //TODO: Update No of Tags
                if (_areasPointSources != null && _areasPointSources.Count() > 0)
                {
                    foreach (var ptSrc in _areasPointSources)
                    {
                        long numOfTags = 0;
                        var ptSrcTags = successTags.GroupBy(st => st.SRC_PI_SERVER_CD).Where(gt => gt.Key == ptSrc.PI_SERVER_CD).FirstOrDefault();
                        if (ptSrcTags != null && ptSrcTags.Count() > 0)
                        {
                            numOfTags = ptSrcTags.Count();
                            var updatePtSrc = await _tagCreatorStore.UpdatePointSource(ptSrc.EPS_NUM, numOfTags);
                        }
                    }
                }

            }

            if (_insertResult.HasErrors)
            {
                //Check each error tag=> if it's already exist update the flag to 'Y' with remark, if it's for another reason then flag to 'Y' with remark too

                //Existing Tags
                var existingTags = tags.Where(t => t.ECCPI_TAG_NAME == _insertResult.Errors.Where(e => e.Key == t.ECCPI_TAG_NAME && e.Value.Message.ToLower().Contains("tag already exists")).FirstOrDefault().Key);

                if (existingTags != null && existingTags.Count() > 0)
                {
                    foreach (var tag in existingTags)
                    {
                        var updateStatus = await _tagCreatorStore.UpdateCreatedTag(tag.EAWFT_NUM, tag.ECCPI_POINT_ID, string.Format("Tag Already Exists in {0}", _eccPIServerName), 'Y');
                    }
                    Logger.Info("ECCPITagCreator", string.Format("{0} Existing Tags", existingTags.Count()));
                    await _tagCreatorStore.Commit();
                }

                //Other error Tags
                var otherErrorTags = tags.Where(t => t.ECCPI_TAG_NAME == _insertResult.Errors.Where(e => e.Key == t.ECCPI_TAG_NAME && !e.Value.Message.ToLower().Contains("tag already exists")).FirstOrDefault().Key);
                if (otherErrorTags != null && otherErrorTags.Count() > 0)
                {
                    foreach (var tag in otherErrorTags)
                    {
                        var updateStatus = await _tagCreatorStore.UpdateCreatedTag(tag.EAWFT_NUM, null, _insertResult.Errors.Where(e => e.Key == tag.ECCPI_TAG_NAME).FirstOrDefault().Value.Message, 'N');
                    }
                    Logger.Info("ECCPITagCreator", string.Format("{0} Errors Upon Creation", otherErrorTags.Count()));
                    await _tagCreatorStore.Commit();
                }
            }
        }

        ///TODO: to be implemented -- Search for instrument tags if they exist in the ECCPI
        /// for existing tags => check if these are flagged to be renamed 
        /// => if yes, rename the tag in ECCPI Server and update the existing tag name in oracle accordinally
        /// => else, update the existing tag name field in oracle database
        private async Task CheckForExistingTagsAsync(IEnumerable<PITagDataModel> tags)
        {
            int _foundTagsNum = 0;
            int _start = 0;
            int _limit = 500;
            //tags = tags.Take(10);
            while (_start <= tags.Count())
            {
                var _iTags = tags.Skip(_start).Take(_limit);

                IList<IEnumerable<PIPointQuery>> _queries = new List<IEnumerable<PIPointQuery>>();
                foreach (var tag in _iTags)
                {
                    List<PIPointQuery> _query = new List<PIPointQuery>();
                    _query.Add(new PIPointQuery(PICommonPointAttributes.InstrumentTag, OSIsoft.AF.Search.AFSearchOperator.Equal, tag.AREA_PI_TAG_NAME));
                    _queries.Add(_query);
                }

                var findExistingTags = await PIPoint.FindPIPointsAsync(piServer, queries: _queries, attributeNames: new List<string>() { PICommonPointAttributes.InstrumentTag });
                foreach (var foundTag in findExistingTags)
                {
                    var originalTag = tags.Where(t => t.AREA_PI_TAG_NAME == foundTag.GetAttribute(PICommonPointAttributes.InstrumentTag).ToString()).FirstOrDefault();
                    if (originalTag != null && originalTag.ECCPI_EXST_TAG_NAME != foundTag.Name)
                    {
                        if (!string.IsNullOrEmpty(originalTag.ECCPI_TAG_REN_RQST_FLG))
                        {
                            switch (originalTag.ECCPI_TAG_REN_RQST_FLG)
                            {
                                case "N":
                                default:
                                    originalTag.ECCPI_EXST_TAG_NAME = foundTag.Name;
                                    //update the record in oracle db
                                    var updateExistingTag = await _tagCreatorStore.UpdateExistingTag(originalTag.EAWFT_NUM, foundTag.Name);
                                    originalTag.ECCPI_TAG_CRE_FLG = "Y";
                                    break;
                                case "Y":
                                    //Rename the tag in PI and update existing tag name accordinaly in oracle 
                                    foundTag.SetAttribute(PICommonPointAttributes.Tag, originalTag.ECCPI_TAG_NAME);
                                    foundTag.SaveAttributes(new string[] { PICommonPointAttributes.Tag });

                                    var updateRenamedExistingTag = await _tagCreatorStore.UpdateExistingTag(originalTag.EAWFT_NUM, originalTag.ECCPI_TAG_NAME, renamedInPIServerFlag: true);
                                    originalTag.ECCPI_TAG_CRE_FLG = "Y";
                                    break;
                            }
                        }
                    }
                }
                _foundTagsNum += findExistingTags.Count();
                _start += _limit;
            }
        }



        public async Task<IDictionary<string, IDictionary<string, object>>> GetPointsDefinitions(IEnumerable<PITagDataModel> tags)
        {

            IEnumerable<string> tagsSourceServersNames = tags.GroupBy(t => t.SRC_PI_SERVER_CD).Select(t => t.Key);
            IDictionary<string, PointSourceDataModel> pointSources = new Dictionary<string, PointSourceDataModel>();
            foreach (var serverKey in tagsSourceServersNames)
            {
                var availablePointSource = await _tagCreatorStore.GetServerPointSource(serverKey.ToString());
                if (availablePointSource != null)
                {
                    pointSources.Add(serverKey, availablePointSource);
                    _areasPointSources.Add(availablePointSource);
                }
            }
            return tags.MapToPIPointDefinition(withDescriptor: true, pointSourceDefinitions: pointSources);
        }

        //public async Task<bool> TestAsync()
        //{
        //    try
        //    {
        //        var tags = await _tagCreatorStore.GetTagsForCreation();
        //        if (tags != null && tags.Count() > 0)
        //        {
        //            PIServer piServer = PIAFUtils.GetPIServer(_eccPIServerName);
        //            //Collect all the tags basic attributes required for insertion
        //            var _pointsDefinitions = tags.MapToPIPointDefinition(withDescriptor: true);
        //            //TODO: to be implemented -- Add the pointsrouce, location1 and location4 attributes gotten from ECCPI_POINT_SOURCES table (search for point source having the least number of tags)
        //            //
        //            try
        //            {
        //                tags = tags.DistinctBy(t => t.ECCPI_TAG_NAME);

        //                //TODO: to be implemented -- Search for instrument tags if they exist in the ECCPI 

        //                int _foundTagsNum = 0;
        //                int _start = 0;
        //                int _limit = 500;
        //                //tags = tags.Take(10);
        //                while (_start <= tags.Count())
        //                {
        //                    var _iTags = tags.Skip(_start).Take(_limit);

        //                    IList<IEnumerable<PIPointQuery>> _queries = new List<IEnumerable<PIPointQuery>>();
        //                    foreach (var tag in _iTags)
        //                    {
        //                        List<PIPointQuery> _query = new List<PIPointQuery>();
        //                        _query.Add(new PIPointQuery(PICommonPointAttributes.InstrumentTag, OSIsoft.AF.Search.AFSearchOperator.Equal, tag.AREA_PI_TAG_NAME));
        //                        _queries.Add(_query);
        //                    }

        //                    var findExistingTags = await PIPoint.FindPIPointsAsync(piServer, queries: _queries, attributeNames: new List<string>() { PICommonPointAttributes.InstrumentTag });
        //                    _foundTagsNum += findExistingTags.Count();
        //                    _start += _limit;
        //                }
        //                //foreach (var tag in tags.Take(1000))
        //                //{
        //                //    if (string.IsNullOrEmpty(_query))
        //                //        _query = string.Format("{0}:={1}", PICommonPointAttributes.InstrumentTag, tag.AREA_PI_TAG_NAME);
        //                //    else
        //                //        _query += string.Format(" OR {0}:={1}", PICommonPointAttributes.InstrumentTag, tag.AREA_PI_TAG_NAME);
        //                //}


        //                //foreach (var res in queryResult)
        //                //{
        //                //    var dd = res;
        //                //}
        //                //Insert the tags in bulk
        //                //AFListResults<string, PIPoint> _insertResult = piServer.CreatePIPoints(_pointsDefinitions);

        //                // TODO: Update the duplicates occurs before the insertion

        //                //Receive results, and update the status of each inserted tag
        //                var ccc = _foundTagsNum;
        //            }
        //            catch (Exception e)
        //            {
        //                Logger.Error("ECCPITagCreator", e);
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        Logger.Error("ECCPITagCreator", e);
        //        return false;
        //    }
        //}
    }
}
