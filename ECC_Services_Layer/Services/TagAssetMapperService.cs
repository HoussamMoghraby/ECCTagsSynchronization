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
    public class TagAssetMapperService : ECCServiceBase, IECCService
    {
        private TagAssetMapperStore _tagMapperStore = new TagAssetMapperStore();
        private string _eccAFServerName = ConfigurationSettings.AppSettings.Get("ECC_AF_ServerName");
        private string _eccPIServerName = ConfigurationSettings.AppSettings.Get("ECC_PI_ServerName");


        /// <summary>
        /// Gets the tags recenly created and not mapped yet from the oracle database
        /// 1- Checks if the attributes exists in AF => If it does not exist it will keep the flags untouched and add a remark in the table
        /// 2- Assign the tag to the target attribute and flag the oracle database record
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartAsync()
        {
            PISystem piSystem = PIAFUtils.GetPISystem(_eccAFServerName);
            try
            {
                //Get the potential data references that could be used in when updating AF attributes
                Dictionary<string, AFPlugIn> _dataReferences = new Dictionary<string, AFPlugIn>()
                    {
                        { PIAFUtils.DataReference.PIPoint, PIAFUtils.GetDataReferencePlugin(piSystem) },
                        { PIAFUtils.DataReference.PIPointArray, PIAFUtils.GetDataReferencePlugin(piSystem,PIAFUtils.DataReference.PIPointArray) }
                    };

                // Get the created and unmapped tags from oracle
                var tags = await _tagMapperStore.GetUnmappedTags();
                if (tags != null && tags.Count() > 0)
                {
                    Logger.Info(ServiceName, "Finding Attributes");

                    // Validate the attributes in AF
                    var queryAttributes = AFAttribute.FindAttributesByPath(tags.DistinctBy(t => t.W_AF_ATTRB_FULL_PATH).MapToListOfAttributePath(), null);
                    Logger.Info(ServiceName, string.Format("Found {0} Attributes", queryAttributes.Count()));

                    IList<AFAttribute> attributes = new List<AFAttribute>();
                    IList<string> configStrings = new List<string>();
                    foreach (var res in queryAttributes.Results)
                    {
                        // Find the tags for a found attribute
                        IEnumerable<PITagDataModel> _attributeTags = tags.Where(t => t.W_AF_ATTRB_FULL_PATH == res.Value.GetPath());
                        // Define the data reference type of the target attribute based on tags count
                        bool _isDataReferenceArray = (_attributeTags.Count() > 1);
                        string _configString = string.Empty;

                        // Construct the configuration string based on existing tags name
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
                            string _attrConfigString = attr.ConfigString;

                            // Fetch the configuration string and decide
                            // 1- If the attribute isn't assigned to a PI Tag then ignore it
                            // 2- If the tag is flagged to be overriden then ignore the case #1 and override the existing configuration in AF and remark the previously existing tag name
                            string[] _splittedConfigString = _attrConfigString.Split(new string[] { @"\" }, StringSplitOptions.None);
                            bool _overrideConfigString = (!string.IsNullOrEmpty(_attributeTags.FirstOrDefault().ECCPI_AF_MAP_OVR_FLG) && _attributeTags.FirstOrDefault().ECCPI_AF_MAP_OVR_FLG == "Y");
                            bool _currentConfigStringIsdefault = (
                                _attrConfigString.Contains((attr.Element.Name + "." + attr.Name)) ||
                                _attrConfigString == @"\\%Server%\%Element%.%Attribute%" ||
                                _splittedConfigString[_splittedConfigString.GetLength(0) - 1].Contains("%Element%.%Attribute%")
                                );

                            // Attribute is valid to be mapped
                            if (_currentConfigStringIsdefault || _overrideConfigString == true)
                            {
                                attr.DataReferencePlugIn = (_isDataReferenceArray) ? _dataReferences[PIAFUtils.DataReference.PIPointArray] : _dataReferences[PIAFUtils.DataReference.PIPoint];
                                attributes.Add(attr);
                                _attributeTags.ForEach(at => at.IsValidForAssetMapping = true);
                                configStrings.Add(string.Format(@"\\{0}\{1}", _eccPIServerName, _configString));
                            }
                            // Attribute is not valid to be mapped
                            else
                                _attributeTags.ForEach(at =>
                                {
                                    at.ECCPI_AF_MAP_REM = attr.ConfigString;
                                    at.IsValidForAssetMapping = false;
                                });
                        }
                    }
                    try
                    {
                        //Update the found attributes each with its PITag ConfigString
                        Logger.Info(ServiceName, "Updating AF");
                        AFAttribute.SetConfigStrings(attributes, configStrings);
                        piSystem.CheckIn(AFCheckedOutMode.ObjectsCheckedOutToMe);
                        Logger.Info(ServiceName, "AF Checked in");

                        //Update the Tag flags and status in Oracle database
                        var successTags = tags.Where(t => t.IsValidForAssetMapping.HasValue && t.IsValidForAssetMapping.Value == true);
                        foreach (var successTag in successTags)
                        {
                            var updateStatus = await _tagMapperStore.UpdateMappedTag(successTag.EAWFT_NUM, string.Format("Tag Mapped Successfully in {0}{1}", _eccAFServerName, (successTag.IsValidForAssetMapping == true) ? string.Format(" & Replaced {0}", successTag.ECCPI_AF_MAP_REM) : null), 'Y');
                        }
                        Logger.Info(ServiceName, string.Format("{0} Mapped Tags", (successTags != null) ? successTags.Count() : 0));

                        //Update skipped tags
                        var skippedTags = tags.Where(t => t.IsValidForAssetMapping.HasValue && t.IsValidForAssetMapping.Value == false);
                        foreach (var skippedTag in skippedTags)
                        {
                            var updateStatus = await _tagMapperStore.UpdateMappedTag(skippedTag.EAWFT_NUM, skippedTag.ECCPI_AF_MAP_REM, 'N');
                        }
                        Logger.Info(ServiceName, string.Format("{0} Skipped Tags", (skippedTags != null) ? skippedTags.Count() : 0));


                        await _tagMapperStore.Commit();
                    }
                    catch (Exception e)
                    {
                        Logger.Error(ServiceName, e);
                    }

                    //Update the status of the ERROR results of Attributes/Elements mapping
                    if (queryAttributes.HasErrors)
                    {
                        var errorTags = tags.Where(t => queryAttributes.Errors.ContainsKey(t.W_AF_ATTRB_FULL_PATH));
                        foreach (var errorTag in errorTags)
                        {
                            var updateStatus = await _tagMapperStore.UpdateMappedTag(errorTag.EAWFT_NUM, string.Format(@"{0} not found", errorTag.W_AF_ATTRB_FULL_PATH), 'N');
                        }
                        Logger.Info(ServiceName, string.Format("{0} Error Tags", (errorTags != null) ? errorTags.Count() : 0));
                        await _tagMapperStore.Commit();
                    }

                }

            }
            catch (Exception e)
            {
                Logger.Error(ServiceName, e);
                return false;
            }
            piSystem.CheckIn(AFCheckedOutMode.ObjectsCheckedOutToMe);
            piSystem.Disconnect();
            return true;
        }
    }
}
