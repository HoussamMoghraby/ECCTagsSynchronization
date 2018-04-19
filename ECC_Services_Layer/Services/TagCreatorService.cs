using ECC_AFServices_Layer.Helpers;
using ECC_AFServices_Layer.Services.Abstract;
using ECC_DataLayer.DataModels;
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
using System.Text;
using System.Threading.Tasks;

namespace ECC_AFServices_Layer.Services
{
    public class TagCreatorService : IECCService
    {
        private TagCreatorStore _tagCreatorStore = new TagCreatorStore();
        private string _eccPIServerName = ConfigurationSettings.AppSettings.Get("ECC_PI_ServerName");

        public async Task<bool> Start()
        {
            try
            {
                var tags = await _tagCreatorStore.GetTagsForCreation();
                if (tags != null && tags.Count() > 0)
                {
                    PIServer piServer = PIAFUtils.GetPIServer(_eccPIServerName);
                    //Collect all the tags attributes required for insertion
                    var _pointsDefinitions = tags.MapToPIPointDefinition(withDescriptor: true);
                    try
                    {
                        //Insert the tags in bulk
                        tags = tags.DistinctBy(t => t.ECCPI_TAG_NAME);
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

                            foreach (var tag in successTags)
                            {
                                var updateStatus = await _tagCreatorStore.UpdateCreatedTag(tag.EAWFT_NUM, string.Format("Tag Created Successfully in {0}", _eccPIServerName), 'Y');
                            }
                            Logger.Info("ECCPITagCreator", string.Format("{0} Inserted Tags", (successTags != null) ? successTags.Count() : 0));
                            await _tagCreatorStore.Commit();
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
                                    var updateStatus = await _tagCreatorStore.UpdateCreatedTag(tag.EAWFT_NUM, string.Format("Tag Already Exists in {0}", _eccPIServerName), 'Y');
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
                                    var updateStatus = await _tagCreatorStore.UpdateCreatedTag(tag.EAWFT_NUM, _insertResult.Errors.Where(e => e.Key == tag.ECCPI_TAG_NAME).FirstOrDefault().Value.Message, 'N');
                                }
                                Logger.Info("ECCPITagCreator", string.Format("{0} Errors Upon Creation", otherErrorTags.Count()));
                                await _tagCreatorStore.Commit();
                            }
                        }
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
    }
}
