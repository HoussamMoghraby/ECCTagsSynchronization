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
using System.Text;

namespace ECC_AFServices_Layer.Services
{
    public class TagCreatorService : ECCServiceBase, IECCService
    {
        private TagCreatorStore _tagCreatorStore = new TagCreatorStore();
        private string _eccPIServerName = ConfigurationSettings.AppSettings.Get("ECC_PI_ServerName");
        private PIServer piServer;
        private List<PointSourceDataModel> _areasPointSources = new List<PointSourceDataModel>();
        private DbLoggerDetails _dbLoggerDetails = new DbLoggerDetails("ECCPITagCreator_Service");

        /// <summary>
        /// Get all the non-created tags from oracle database and create them in the central PI Server:
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartAsync()
        {
            LogServiceStart();
            piServer = PIAFUtils.GetPIServer(_eccPIServerName);
            try
            {
                // Get the Non-created tags from oracle database
                var tags = await _tagCreatorStore.GetTagsForCreation();
                StringBuilder exceptionMessage = new StringBuilder();
                if (tags != null && tags.Count() > 0)
                {
                    try
                    {
                        // Distinct the repeated tag names to avoid errors when creating the tags in PI server
                        tags = tags.DistinctBy(t => t.ECCPI_TAG_NAME);

                        // Check and handle the tags before creation
                        await CheckForExistingTagsAsync(tags);

                        //Update existing tags in ECC PI Server if changed
                        await CheckForExistingTagsByPointIdAsync(tags);

                        // Create the tags in central PI server
                        await CreateTagsAsync(tags);
                    }
                    catch (Exception e)
                    {
                        exceptionMessage.AppendLine(e.Message);
                        Logger.Error(ServiceName, e);
                    }
                }
                DbLogger._dbLoggerDataModel.REMARKS = "Status: " + Status.Succeed + ((exceptionMessage.Length > 0) ? "; Handled Excpetion: " + exceptionMessage.ToString() : "");
                LogServiceEnd();
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(ServiceName, e);
                DbLogger._dbLoggerDataModel.REMARKS = $"Status: {Status.Fail}; Exception: Message='{e.Message}', InnerException='{e.InnerException}'";
                LogServiceEnd();
                return false;
            }
        }


        private async Task CheckForExistingTagsByPointIdAsync(IEnumerable<PITagDataModel> tags)
        {
            int _foundTagsNum = 0;
            int _start = 0;
            int _limit = 500;
            var _existingTags = tags.Where(t => t.ECCPI_POINT_ID.HasValue); //TODO: to check the re-process flag
            while (_start <= _existingTags.Count())
            {
                var _iTags = _existingTags.Skip(_start).Take(_limit);

                // Construct the query to search the PI server by PointId
                IList<IEnumerable<PIPointQuery>> _queries = new List<IEnumerable<PIPointQuery>>();
                foreach (var tag in _iTags)
                {
                    try
                    {
                        List<PIPointQuery> _query = new List<PIPointQuery>();
                        _query.Add(new PIPointQuery(PICommonPointAttributes.PointID, OSIsoft.AF.Search.AFSearchOperator.Equal, tag.ECCPI_POINT_ID.ToString()));
                        _queries.Add(_query);
                    }
                    catch (Exception ex)
                    {
                        _dbLoggerDetails.Log(new DbLoggerDetailsDataModel
                        {
                            EAWFT_NUM = tag.EAWFT_NUM,
                            ECCPI_TAG_NAME = tag.ECCPI_TAG_NAME,
                            AREA_PI_TAG_NAME = tag.AREA_PI_TAG_NAME,
                            SRC_PI_SERVER_CD = tag.SRC_PI_SERVER_CD,
                            SVC_MSG = ex.Message,
                            SVC_MSG_TYP = svcType.Tag,
                            SVC_MSG_SEVIRITY = Severity.Exception,
                            AREA_POINT_ID = tag.AREA_POINT_ID
                        });
                        throw;
                    }
                }

                // Execute the search query
                var findExistingTags = await PIPoint.FindPIPointsAsync(piServer, queries: _queries, attributeNames: new List<string>() { PICommonPointAttributes.PointID });

                // Itterate the result
                foreach (var foundTag in findExistingTags)
                {
                    // Find the tag with matching instrumenttag name
                    var originalTag = _existingTags.Where(t => t.ECCPI_POINT_ID.Value.ToString() == foundTag.GetAttribute(PICommonPointAttributes.PointID).ToString()).FirstOrDefault();
                    try
                    {
                        // Rename the tag in PI and update existing tag name accordinaly in oracle
                        foundTag.SaveAttributes(new Dictionary<string, object>() {
                        { PICommonPointAttributes.Tag, originalTag.ECCPI_TAG_NAME},
                        { PICommonPointAttributes.Descriptor, originalTag.PI_TAG_DESCRIPTOR},
                        { PICommonPointAttributes.EngineeringUnits, originalTag.ENGUNITS},
                        { PICommonPointAttributes.DigitalSetName, originalTag.ECCPI_DIGITAL_SET},
                        { PICommonPointAttributes.PointType, originalTag.POINTTYPE},
                        { PICommonPointAttributes.Location2, originalTag.LOCATION2},
                        { PICommonPointAttributes.Location3, originalTag.LOCATION3},
                        { PICommonPointAttributes.Location5, originalTag.LOCATION5},
                        { PICommonPointAttributes.UserInt1, originalTag.USERINT1},
                        { PICommonPointAttributes.UserInt2, originalTag.USERINT2},
                        { PICommonPointAttributes.UserReal1, originalTag.USERREAL1},
                        { PICommonPointAttributes.UserReal2, originalTag.USERREAL2},
                        { PICommonPointAttributes.Compressing, originalTag.COMPRESSING},
                        { PICommonPointAttributes.CompressionDeviation, originalTag.COMPDEV},
                        { PICommonPointAttributes.CompressionMaximum, originalTag.COMPMAX},
                        { PICommonPointAttributes.CompressionMinimum, originalTag.COMPMIN},
                        { PICommonPointAttributes.CompressionPercentage, originalTag.COMPDEVPERCENT},
                        { PICommonPointAttributes.ExceptionDeviation, originalTag.EXCDEV},
                        { PICommonPointAttributes.ExceptionMaximum, originalTag.EXCMAX},
                        { PICommonPointAttributes.ExceptionMinimum, originalTag.EXCMIN},
                        { PICommonPointAttributes.ExceptionPercentage, originalTag.EXCDEVPERCENT},
                        { PICommonPointAttributes.Span, originalTag.SPAN},
                        { PICommonPointAttributes.Step, originalTag.STEP},
                        { PICommonPointAttributes.TypicalValue, originalTag.TYPICALVALUE},
                        { PICommonPointAttributes.Zero, originalTag.ZERO}
                        });

                        //TODO: flag the tag as updated in oracle database and flag to stop reprocessing.

                        // Flag the tag as renamed in oracle database
                        var updateModifiedExistingTag = await _tagCreatorStore.UpdateModifiedTag(originalTag.EAWFT_NUM, originalTag.ECCPI_POINT_ID, "Tag updated in ECCPI Server", 'Y');
                        originalTag.ECCPI_TAG_CRE_FLG = "Y";

                        _dbLoggerDetails.Log(new DbLoggerDetailsDataModel
                        {
                            EAWFT_NUM = originalTag.EAWFT_NUM,
                            ECCPI_TAG_NAME = originalTag.ECCPI_TAG_NAME,
                            AREA_PI_TAG_NAME = originalTag.AREA_PI_TAG_NAME,
                            SRC_PI_SERVER_CD = originalTag.SRC_PI_SERVER_CD,
                            SVC_MSG = "Tag updated in ECCPI Server",
                            SVC_MSG_TYP = svcType.Tag,
                            SVC_MSG_SEVIRITY = Severity.Information,
                            AREA_POINT_ID = originalTag.AREA_POINT_ID
                        });
                    }
                    catch (Exception ex)
                    {
                        _dbLoggerDetails.Log(new DbLoggerDetailsDataModel
                        {
                            EAWFT_NUM = originalTag.EAWFT_NUM,
                            ECCPI_TAG_NAME = originalTag.ECCPI_TAG_NAME,
                            AREA_PI_TAG_NAME = originalTag.AREA_PI_TAG_NAME,
                            SRC_PI_SERVER_CD = originalTag.SRC_PI_SERVER_CD,
                            SVC_MSG = ex.Message,
                            SVC_MSG_TYP = svcType.Tag,
                            SVC_MSG_SEVIRITY = Severity.Exception,
                            AREA_POINT_ID = originalTag.AREA_POINT_ID,
                        });
                        //throw;
                    }
                }
                _foundTagsNum += findExistingTags.Count();
                _start += _limit;
            }
        }


        /// <summary>
        /// Create the valid tags in central PI server
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        private async Task CreateTagsAsync(IEnumerable<PITagDataModel> tags)
        {
            //Initialy filter the list and screen the already created tags
            tags = tags.Where(t => t.ECCPI_TAG_CRE_FLG != "Y");

            // Create the PI Points in PI Server
            var _pointsDefinitions = await GetPointsDefinitions(tags);
            AFListResults<string, PIPoint> _insertResult = piServer.CreatePIPoints(_pointsDefinitions);

            DateTime creationDate = DateTime.Now;

            //Receive results, and update the status of each inserted tag
            if (_insertResult != null && _insertResult.Results != null && _insertResult.Results.Count() > 0)
            {
                IList<PIPoint> _successResults = _insertResult.Results;
                var successTags = tags.Where(t => _successResults.Where(sr => sr.Name == t.ECCPI_TAG_NAME).FirstOrDefault() != null && t.ECCPI_TAG_NAME == _successResults.Where(sr => sr.Name == t.ECCPI_TAG_NAME).FirstOrDefault().Name);

                if (_insertResult.HasErrors)
                {
                    successTags = successTags.Where(st => !_insertResult.Errors.Select(e => e.Key).Contains(st.ECCPI_TAG_NAME));
                }

                // Fill ECCPI_POINT_ID in successfull tags to be updating in oracle db
                successTags.ForEach(st =>
                {
                    st.ECCPI_POINT_ID = _successResults.Where(sr => sr.Name == st.ECCPI_TAG_NAME).FirstOrDefault().ID;
                });

                foreach (var tag in successTags)
                {
                    var updateStatus = await _tagCreatorStore.UpdateCreatedTag(tag.EAWFT_NUM, tag.ECCPI_POINT_ID, string.Format("Tag Created Successfully in {0}", _eccPIServerName), 'Y', creationDate);
                }
                Logger.Info(ServiceName, string.Format("{0} Inserted Tags", (successTags != null) ? successTags.Count() : 0));
                await _tagCreatorStore.Commit();

                // Update the number of tags created in the area point source table
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
                    Logger.Info(ServiceName, string.Format("{0} Existing Tags", existingTags.Count()));
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
                    Logger.Info(ServiceName, string.Format("{0} Tags failed to be created, check remarks column for more details)", otherErrorTags.Count()));
                    await _tagCreatorStore.Commit();
                }
            }
        }


        /// <summary>
        /// Search for instrument tags if they exist in the ECCPI
        ///     if yes => rename the tag in ECCPI Server and update the existing tag name in oracle accordinally
        ///     else => update the existing tag name field in oracle database
        /// </summary>
        /// <param name="tags">List of tags to be checked and validated</param>
        /// <returns></returns>
        private async Task CheckForExistingTagsAsync(IEnumerable<PITagDataModel> tags)
        {
            int _foundTagsNum = 0;
            int _start = 0;
            int _limit = 500;
            while (_start <= tags.Count())
            {
                var _iTags = tags.Skip(_start).Take(_limit);

                // Construct the query to search the PI server by instrutment tag name
                IList<IEnumerable<PIPointQuery>> _queries = new List<IEnumerable<PIPointQuery>>();
                foreach (var tag in _iTags)
                {
                    try
                    {
                        List<PIPointQuery> _query = new List<PIPointQuery>();
                        _query.Add(new PIPointQuery(PICommonPointAttributes.InstrumentTag, OSIsoft.AF.Search.AFSearchOperator.Equal, tag.AREA_PI_TAG_NAME));
                        _queries.Add(_query);
                    }
                    catch (Exception ex)
                    {
                        _dbLoggerDetails.Log(new DbLoggerDetailsDataModel
                        {
                            EAWFT_NUM = tag.EAWFT_NUM,
                            ECCPI_TAG_NAME = tag.ECCPI_TAG_NAME,
                            AREA_PI_TAG_NAME = tag.AREA_PI_TAG_NAME,
                            SRC_PI_SERVER_CD = tag.SRC_PI_SERVER_CD,
                            SVC_MSG = ex.Message,
                            SVC_MSG_TYP = svcType.Tag,
                            SVC_MSG_SEVIRITY = Severity.Exception,
                            AREA_POINT_ID = tag.AREA_POINT_ID
                        });
                        throw;
                    }
                }

                // Execute the search query
                var findExistingTags = await PIPoint.FindPIPointsAsync(piServer, queries: _queries, attributeNames: new List<string>() { PICommonPointAttributes.InstrumentTag });

                // Itterate the result
                foreach (var foundTag in findExistingTags)
                {
                    // Find the tag with matching instrumenttag name
                    var originalTag = tags.Where(t => t.AREA_PI_TAG_NAME == foundTag.GetAttribute(PICommonPointAttributes.InstrumentTag).ToString()).FirstOrDefault();
                    try
                    {

                        // The result is valid and the exsiting tag name different than the found one
                        if (originalTag != null && originalTag.ECCPI_EXST_TAG_NAME != foundTag.Name)
                        {
                            if (!string.IsNullOrEmpty(originalTag.ECCPI_TAG_REN_RQST_FLG))
                            {
                                switch (originalTag.ECCPI_TAG_REN_RQST_FLG)
                                {
                                    // Tag isn't requested to be renamed => update the local databse with the existing tag name
                                    case "N":
                                    default:
                                        originalTag.ECCPI_EXST_TAG_NAME = foundTag.Name;
                                        // Update the record in oracle db
                                        var updateExistingTag = await _tagCreatorStore.UpdateExistingTag(originalTag.EAWFT_NUM, foundTag.Name);
                                        originalTag.ECCPI_TAG_CRE_FLG = "Y";
                                        break;
                                    // Tag is requested to be renamed => Update the tag name in the central PI Server
                                    case "Y":
                                        // Rename the tag in PI and update existing tag name accordinaly in oracle 
                                        foundTag.SetAttribute(PICommonPointAttributes.Tag, originalTag.ECCPI_TAG_NAME);
                                        foundTag.SaveAttributes(new string[] { PICommonPointAttributes.Tag });

                                        // Flag the tag as renamed in oracle database
                                        var updateRenamedExistingTag = await _tagCreatorStore.UpdateExistingTag(originalTag.EAWFT_NUM, originalTag.ECCPI_TAG_NAME, renamedInPIServerFlag: true);
                                        originalTag.ECCPI_TAG_CRE_FLG = "Y";
                                        break;
                                }
                                _dbLoggerDetails.Log(new DbLoggerDetailsDataModel
                                {
                                    EAWFT_NUM = originalTag.EAWFT_NUM,
                                    ECCPI_TAG_NAME = originalTag.ECCPI_TAG_NAME,
                                    AREA_PI_TAG_NAME = originalTag.AREA_PI_TAG_NAME,
                                    SRC_PI_SERVER_CD = originalTag.SRC_PI_SERVER_CD,
                                    SVC_MSG = "Updated Tag name successfully",
                                    SVC_MSG_TYP = svcType.Tag,
                                    SVC_MSG_SEVIRITY = Severity.Information,
                                    AREA_POINT_ID = originalTag.AREA_POINT_ID
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _dbLoggerDetails.Log(new DbLoggerDetailsDataModel
                        {
                            EAWFT_NUM = originalTag.EAWFT_NUM,
                            ECCPI_TAG_NAME = originalTag.ECCPI_TAG_NAME,
                            AREA_PI_TAG_NAME = originalTag.AREA_PI_TAG_NAME,
                            SRC_PI_SERVER_CD = originalTag.SRC_PI_SERVER_CD,
                            SVC_MSG = ex.Message,
                            SVC_MSG_TYP = svcType.Tag,
                            SVC_MSG_SEVIRITY = Severity.Exception,
                            AREA_POINT_ID = originalTag.AREA_POINT_ID,
                        });
                        //throw;
                    }
                }
                _foundTagsNum += findExistingTags.Count();
                _start += _limit;
            }
        }



        /// <summary>
        /// Remap the tags into points definition
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public async Task<IDictionary<string, IDictionary<string, object>>> GetPointsDefinitions(IEnumerable<PITagDataModel> tags)
        {

            // Group the tags list by source server name
            IEnumerable<string> tagsSourceServersNames = tags.GroupBy(t => t.SRC_PI_SERVER_CD).Select(t => t.Key);
            IDictionary<string, PointSourceDataModel> pointSources = new Dictionary<string, PointSourceDataModel>();
            foreach (var serverKey in tagsSourceServersNames)
            {
                // Get the point source information of the current group(area)
                PointSourceDataModel availablePointSource = await _tagCreatorStore.GetServerPointSource(serverKey.ToString());
                if (availablePointSource != null)
                {
                    pointSources.Add(serverKey, availablePointSource);
                    _areasPointSources.Add(availablePointSource);
                }
            }

            // Map and return the tags record to map definition format
            return tags.MapToPIPointDefinition(withDescriptor: true, pointSourceDefinitions: pointSources);
        }
    }
}
