using ECC_AFServices_Layer.Helpers;
using ECC_AFServices_Layer.Services.Abstract;
using ECC_DataLayer.Helpers;
using ECC_DataLayer.Stores;
using ECC_PIAFServices_Layer.Helpers;
using MoreLinq;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.PI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECC_AFServices_Layer.Services
{
    public class TagAssetMapperService : IECCService
    {
        private TagAssetMapperStore _tagMapperStore = new TagAssetMapperStore();
        private string _eccAFServerName = ConfigurationSettings.AppSettings.Get("ECC_AF_ServerName");

        /// <summary>
        /// Get the Tags created in ECCPITagCreatorModule joined with the associated AF element  
        ///  if the attribute exists => update the data reference with new tag created
        /// else => create the new attribute with the right data reference
        /// if the element does not exist => flag the database that the element is missing
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartAsync()
        {
            PISystem piSystem = PIAFUtils.GetPISystem(_eccAFServerName);
            try
            {
                var tags = await _tagMapperStore.GetUnmappedTags();
                if (tags != null && tags.Count() > 0)
                {
                    //tags = tags.DistinctBy(t => t.ECCPI_TAG_NAME).DistinctBy(t => t.W_AF_ATTRB_FULL_PATH);
                    var queryAttributes = AFAttribute.FindAttributesByPath(tags.MapToListOfAttributePath(), null);
                    IList<AFAttribute> attributes = new List<AFAttribute>();
                    IList<string> configStrings = new List<string>();
                    AFPlugIn _dataReferencePlugin = AFDataReference.GetPIPointDataReference(piSystem);
                    foreach (var res in queryAttributes.Results)
                    {
                        string _configString = tags.Where(t => t.W_AF_ATTRB_FULL_PATH == res.Value.GetPath()).FirstOrDefault().ECCPI_TAG_NAME;
                        if (!string.IsNullOrEmpty(_configString))
                        {
                            AFAttribute attr = res.Value;
                            attr.DataReferencePlugIn = _dataReferencePlugin;
                            attributes.Add(attr);
                            configStrings.Add(string.Format(@"\\{0}\{1}", _eccAFServerName, _configString));
                        }
                    }
                    try
                    {
                        //Update the found attributes each with its PITag ConfigString
                        AFAttribute.SetConfigStrings(attributes, configStrings);
                        piSystem.CheckIn(AFCheckedOutMode.ObjectsCheckedOutToMe);
                        //Update the Tag flags and status in Oracle database
                        var successTags = tags.Where(t => attributes.Select(attr => attr.GetPath()).Contains(t.W_AF_ATTRB_FULL_PATH));
                        foreach (var successTag in successTags)
                        {
                            var updateStatus = await _tagMapperStore.UpdateMappedTag(successTag.EAWFT_NUM, string.Format("Tag Mapped Successfully in {0}", _eccAFServerName), 'Y');
                        }
                        Logger.Info("ECCPITagAssetMapper", string.Format("{0} Mapped Tags", (successTags != null) ? successTags.Count() : 0));
                        await _tagMapperStore.Commit();
                    }
                    catch (Exception e)
                    {
                        Logger.Error("ECCPITagAssetMapper", e);
                    }

                    //Update the status of the ERROR results of Attributes/Elements mapping
                    if (queryAttributes.HasErrors)
                    {
                        var errorTags = tags.Where(t => queryAttributes.Errors.ContainsKey(t.W_AF_ATTRB_FULL_PATH));
                        foreach (var errorTag in errorTags)
                        {
                            var updateStatus = await _tagMapperStore.UpdateMappedTag(errorTag.EAWFT_NUM, string.Format(@"{0} not found", errorTag.W_AF_ATTRB_FULL_PATH), 'N');
                        }
                        Logger.Info("ECCPITagAssetMapper", string.Format("{0} Error Tags", (errorTags != null) ? errorTags.Count() : 0));
                        await _tagMapperStore.Commit();
                    }

                }

            }
            catch (Exception e)
            {
                Logger.Error("ECCPITagAssetMapper", e);
                piSystem.CheckIn(AFCheckedOutMode.ObjectsCheckedOutToMe);
                piSystem.Disconnect();
                return false;
            }
            piSystem.CheckIn(AFCheckedOutMode.ObjectsCheckedOutToMe);
            piSystem.Disconnect();
            return true;
        }
    }
}
