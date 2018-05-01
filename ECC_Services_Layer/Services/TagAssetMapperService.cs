using ECC_AFServices_Layer.Helpers;
using ECC_AFServices_Layer.Services.Abstract;
using ECC_DataLayer.DataModels;
using ECC_DataLayer.Helpers;
using ECC_DataLayer.Stores;
using ECC_PIAFServices_Layer.Helpers;
using MoreLinq;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace ECC_AFServices_Layer.Services
{
    public class TagAssetMapperService : IECCService
    {
        private TagAssetMapperStore _tagMapperStore = new TagAssetMapperStore();
        private string _eccAFServerName = ConfigurationSettings.AppSettings.Get("ECC_AF_ServerName");
        private string _eccPIServerName = ConfigurationSettings.AppSettings.Get("ECC_PI_ServerName");


        public bool Test()
        {
            PISystem piSystem = PIAFUtils.GetPISystem(_eccAFServerName);
            var _dataReference_PointArray = PIAFUtils.GetDataReferencePlugin(piSystem, PIAFUtils.DataReference.PIPointArray);
            IList<AFAttribute> attributes = new List<AFAttribute>();
            IList<string> configStrings = new List<string>();
            AFPlugIn _dataReferencePlugin = AFDataReference.GetPIPointDataReference(piSystem);
            AFAttribute attr = AFAttribute.FindAttribute(@"\\ECCPIAFTEST\ECC PI AF\Saudi Aramco Fields\Southern Area\KHRS\KHRS-740\WH|Upper_Annuli_Pressure", null);
            attr.DataReferencePlugIn = _dataReference_PointArray;
            attributes.Add(attr);
            configStrings.Add(@"\\fa-ep15w6\KHRS-0740-PI-0001.PV|KHRS-0740-PI-0002.PV");
            AFAttribute.SetConfigStrings(attributes, configStrings);
            piSystem.CheckIn(AFCheckedOutMode.ObjectsCheckedOutToMe);
            return true;
        }
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
                
                Dictionary<string, AFPlugIn> _dataReferences = new Dictionary<string, AFPlugIn>()
                    {
                        { PIAFUtils.DataReference.PIPoint, PIAFUtils.GetDataReferencePlugin(piSystem) },
                        { PIAFUtils.DataReference.PIPointArray, PIAFUtils.GetDataReferencePlugin(piSystem,PIAFUtils.DataReference.PIPointArray) }
                    };
                var tags = (await _tagMapperStore.GetUnmappedTags());
                if (tags != null && tags.Count() > 0)
                {
                    //tags = tags.DistinctBy(t => t.ECCPI_TAG_NAME).DistinctBy(t => t.W_AF_ATTRB_FULL_PATH);
                    Logger.Info("ECCPITagAssetMapper", "Finding Attributes");
                    var queryAttributes = AFAttribute.FindAttributesByPath(tags.DistinctBy(t => t.W_AF_ATTRB_FULL_PATH).MapToListOfAttributePath(), null);
                    Logger.Info("ECCPITagAssetMapper", string.Format("Found {0} Attributes", queryAttributes.Count()));
                    //queryAttributes.Results.Add("aa", AFAttribute.FindAttribute(tags.DistinctBy(t => t.W_AF_ATTRB_FULL_PATH).MapToListOfAttributePath().FirstOrDefault(), null));
                    IList<AFAttribute> attributes = new List<AFAttribute>();
                    IList<string> configStrings = new List<string>();
                    //AFPlugIn _dataReferencePlugin = AFDataReference.GetPIPointDataReference(piSystem);
                    foreach (var res in queryAttributes.Results)
                    {
                        IEnumerable<PITagDataModel> _attributeTags = tags.Where(t => t.W_AF_ATTRB_FULL_PATH == res.Value.GetPath());
                        bool _isDataReferenceArray = (_attributeTags.Count() > 1);
                        string _configString = string.Empty;
                        _attributeTags.ForEach(at =>
                        {
                            if (string.IsNullOrEmpty(_configString))
                                _configString = at.ECCPI_EXST_TAG_NAME;
                            else
                                _configString += string.Format("|{0}", at.ECCPI_EXST_TAG_NAME);
                        });

                        if (!string.IsNullOrEmpty(_configString))
                        {
                            AFAttribute attr = res.Value;
                            attr.DataReferencePlugIn = (_isDataReferenceArray) ? _dataReferences[PIAFUtils.DataReference.PIPointArray] : _dataReferences[PIAFUtils.DataReference.PIPoint];
                            attributes.Add(attr);
                            configStrings.Add(string.Format(@"\\{0}\{1}", _eccPIServerName, _configString));
                        }
                    }
                    try
                    {
                        //TODO: uncomment the below
                        //Update the found attributes each with its PITag ConfigString
                        Logger.Info("ECCPITagAssetMapper", "Updating AF");
                        AFAttribute.SetConfigStrings(attributes, configStrings);
                        piSystem.CheckIn(AFCheckedOutMode.ObjectsCheckedOutToMe);
                        Logger.Info("ECCPITagAssetMapper", "AF Checked in");
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
