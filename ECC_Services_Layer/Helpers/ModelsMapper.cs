using ECC_DataLayer.DataModels;
using ECC_DataLayer.Stores;
using OSIsoft.AF.PI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ECC_AFServices_Layer.Helpers
{
    public static class ModelsMapper //TODO: remove this and use automapper instead
    {

        public static PITagDataModel MapToPITagDataModel(this PIPoint piPoint, string sourcePIServerCode)
        {
            PITagDataModel _piTag = new PITagDataModel(areaTagName: piPoint.Name, tagDescriptor: (piPoint.GetAttribute("Descriptor") != null ? piPoint.GetAttribute("Descriptor").ToString() : null), sourcePIServerCode: sourcePIServerCode, areaPIPointId: piPoint.ID);

            //Advanced Attributes
            IDictionary<string, object> adAttrs = piPoint.GetAttributes(new string[] { "engunits", "digitalset", "pointtype", "location2", "location3", "location5", "userint1", "userint2", "userreal1", "userreal2", "compressing", "compdev", "compmax", "compmin", "compdevpercent", "excdev", "excmax", "excmin", "excdevpercent", "span", "step", "typicalvalue", "zero" });
            //if (adAttrs.ContainsKey("engunits") && adAttrs["engunits"] != null)
            //    _piTag.ENGUNITS = adAttrs["engunits"].ToString();
            _piTag.ENGUNITS = adAttrs.AssignAttributeValue("engunits");
            _piTag.ECCPI_DIGITAL_SET = adAttrs.AssignAttributeValue("digitalset");
            _piTag.POINTTYPE = adAttrs.AssignAttributeValue("pointtype");
            _piTag.LOCATION2 = adAttrs.AssignAttributeValue("location2", isNumeric: true);
            _piTag.LOCATION3 = adAttrs.AssignAttributeValue("location3", isNumeric: true);
            _piTag.LOCATION5 = adAttrs.AssignAttributeValue("location5", isNumeric: true);
            _piTag.USERINT1 = adAttrs.AssignAttributeValue("userint1", isNumeric: true);
            _piTag.USERINT2 = adAttrs.AssignAttributeValue("userint2", isNumeric: true);
            _piTag.USERREAL1 = adAttrs.AssignAttributeValue("userreal1", isNumeric: true);
            _piTag.USERREAL2 = adAttrs.AssignAttributeValue("userreal2", isNumeric: true);
            _piTag.COMPRESSING = adAttrs.AssignAttributeValue("compressing");
            _piTag.COMPDEV = adAttrs.AssignAttributeValue("compdev", isNumeric: true);
            _piTag.COMPMAX = adAttrs.AssignAttributeValue("compmax", isNumeric: true);
            _piTag.COMPMIN = adAttrs.AssignAttributeValue("compmin", isNumeric: true);
            _piTag.COMPDEVPERCENT = adAttrs.AssignAttributeValue("compdevpercent", isNumeric: true);
            _piTag.EXCDEV = adAttrs.AssignAttributeValue("excdev", isNumeric: true);
            _piTag.EXCMAX = adAttrs.AssignAttributeValue("excmax", isNumeric: true);
            _piTag.EXCMIN = adAttrs.AssignAttributeValue("excmin", isNumeric: true);
            _piTag.EXCDEVPERCENT = adAttrs.AssignAttributeValue("excdevpercent", isNumeric: true);
            _piTag.SPAN = adAttrs.AssignAttributeValue("span", isNumeric: true);
            _piTag.STEP = adAttrs.AssignAttributeValue("step", isNumeric: true);
            _piTag.TYPICALVALUE = adAttrs.AssignAttributeValue("typicalvalue", isNumeric: true);
            _piTag.ZERO = adAttrs.AssignAttributeValue("zero", isNumeric: true);


            #region Old Way of Getting Attributes Values
            //if (adAttrs.ContainsKey("pointtype") && adAttrs["pointtype"] != null)
            //    _piTag.POINTTYPE = adAttrs["pointtype"].ToString();
            //if (adAttrs.ContainsKey("location2") && adAttrs["location2"] != null)
            //    _piTag.LOCATION2 = Convert.ToDecimal(adAttrs["location2"]);
            //if (adAttrs.ContainsKey("location3") && adAttrs["location3"] != null)
            //    _piTag.LOCATION3 = Convert.ToDecimal(adAttrs["location3"]);
            //if (adAttrs.ContainsKey("location5") && adAttrs["location5"] != null)
            //    _piTag.LOCATION5 = Convert.ToDecimal(adAttrs["location5"]);
            //if (adAttrs.ContainsKey("userint1") && adAttrs["userint1"] != null)
            //    _piTag.USERINT1 = Convert.ToDecimal(adAttrs["userint1"]);
            //if (adAttrs.ContainsKey("userint2") && adAttrs["userint2"] != null)
            //    _piTag.USERINT2 = Convert.ToDecimal(adAttrs["userint2"]);
            //if (adAttrs["userreal1"] != null)
            //    _piTag.USERREAL1 = Convert.ToDecimal(adAttrs["userreal1"]);
            //if (adAttrs["userreal2"] != null)
            //    _piTag.USERREAL2 = Convert.ToDecimal(adAttrs["userreal2"]);
            //if (adAttrs["compressing"] != null)
            //    _piTag.COMPRESSING = adAttrs["compressing"].ToString();
            //if (adAttrs["compdev"] != null)
            //    _piTag.COMPDEV = Convert.ToDecimal(adAttrs["compdev"]);
            //if (adAttrs["compmax"] != null)
            //    _piTag.COMPMAX = Convert.ToDecimal(adAttrs["compmax"]);
            //if (adAttrs["compmin"] != null)
            //    _piTag.COMPMIN = Convert.ToDecimal(adAttrs["compmin"]);
            //if (adAttrs["compdevpercent"] != null)
            //    _piTag.COMPDEVPERCENT = Convert.ToDecimal(adAttrs["compdevpercent"]);
            //if (adAttrs["excdev"] != null)
            //    _piTag.EXCDEV = Convert.ToDecimal(adAttrs["excdev"]);
            //if (adAttrs["excmax"] != null)
            //    _piTag.EXCMAX = Convert.ToDecimal(adAttrs["excmax"]);
            //if (adAttrs["excmin"] != null)
            //    _piTag.EXCMIN = Convert.ToDecimal(adAttrs["excmin"]);
            //if (adAttrs["excdevpercent"] != null)
            //    _piTag.EXCDEVPERCENT = Convert.ToDecimal(adAttrs["excdevpercent"]);
            //if (adAttrs["span"] != null)
            //    _piTag.SPAN = Convert.ToDecimal(adAttrs["span"]);
            //if (adAttrs["step"] != null)
            //    _piTag.STEP = Convert.ToDecimal(adAttrs["step"]);
            //if (adAttrs["typicalvalue"] != null)
            //    _piTag.TYPICALVALUE = Convert.ToDecimal(adAttrs["typicalvalue"]);
            //if (adAttrs["zero"] != null)
            //    _piTag.ZERO = Convert.ToDecimal(adAttrs["zero"]);
            #endregion
            return _piTag;
        }


        private static dynamic AssignAttributeValue(this IDictionary<string, object> adAttrs, string attrName, bool isNumeric = false)
        {
            dynamic attr = null;
            if (adAttrs.ContainsKey(attrName) && adAttrs[attrName] != null)
            {
                if (isNumeric)
                    attr = Convert.ToDecimal(adAttrs[attrName]);
                else
                    attr = adAttrs[attrName].ToString();
            }
            return attr;
        }

        public static IDictionary<string, object> MapToPIPointAttributes(this PITagDataModel tag, bool withDescriptor = false, PointSourceDataModel pointSourceDefinition = null)
        {
            Dictionary<string, object> attrs = new Dictionary<string, object>() { };

            //
            //TODO: to be implemented -- fill area_tag_name in instrumenttag
            if (tag.AREA_PI_TAG_NAME != null)
                attrs.Add(PICommonPointAttributes.InstrumentTag, tag.AREA_PI_TAG_NAME);
            // TODO: Uncomment this
            if (tag.ENGUNITS != null)
                attrs.Add(PICommonPointAttributes.EngineeringUnits, tag.ENGUNITS);
            if (tag.ECCPI_DIGITAL_SET != null)
                attrs.Add(PICommonPointAttributes.DigitalSetName, tag.ECCPI_DIGITAL_SET);
            if (tag.POINTTYPE != null)
                attrs.Add(PICommonPointAttributes.PointType, tag.POINTTYPE);
            if (tag.LOCATION2 != null)
                attrs.Add(PICommonPointAttributes.Location2, tag.LOCATION2);
            if (tag.LOCATION3 != null)
                attrs.Add(PICommonPointAttributes.Location3, tag.LOCATION3);
            if (tag.LOCATION5 != null)
                attrs.Add(PICommonPointAttributes.Location5, tag.LOCATION5);
            if (tag.USERINT1 != null)
                attrs.Add(PICommonPointAttributes.UserInt1, tag.USERINT1);
            if (tag.USERINT2 != null)
                attrs.Add(PICommonPointAttributes.UserInt2, tag.USERINT2);
            if (tag.USERREAL1 != null)
                attrs.Add(PICommonPointAttributes.UserReal1, tag.USERREAL1);
            if (tag.USERREAL2 != null)
                attrs.Add(PICommonPointAttributes.UserReal2, tag.USERREAL2);
            if (tag.COMPRESSING != null)
                attrs.Add(PICommonPointAttributes.Compressing, tag.COMPRESSING);
            if (tag.COMPDEV != null)
                attrs.Add(PICommonPointAttributes.CompressionDeviation, tag.COMPDEV);
            if (tag.COMPMAX != null)
                attrs.Add(PICommonPointAttributes.CompressionMaximum, tag.COMPMAX);
            if (tag.COMPMIN != null)
                attrs.Add(PICommonPointAttributes.CompressionMinimum, tag.COMPMIN);
            if (tag.COMPDEVPERCENT != null)
                attrs.Add(PICommonPointAttributes.CompressionPercentage, tag.COMPDEVPERCENT);
            if (tag.EXCDEV != null)
                attrs.Add(PICommonPointAttributes.ExceptionDeviation, tag.EXCDEV);
            if (tag.EXCMAX != null)
                attrs.Add(PICommonPointAttributes.ExceptionMaximum, tag.EXCMAX);
            if (tag.EXCMIN != null)
                attrs.Add(PICommonPointAttributes.ExceptionMinimum, tag.EXCMIN);
            if (tag.EXCDEVPERCENT != null)
                attrs.Add(PICommonPointAttributes.ExceptionPercentage, tag.EXCDEVPERCENT);
            if (tag.SPAN != null)
                attrs.Add(PICommonPointAttributes.Span, tag.SPAN);
            if (tag.STEP != null)
                attrs.Add(PICommonPointAttributes.Step, tag.STEP);
            if (tag.TYPICALVALUE != null)
                attrs.Add(PICommonPointAttributes.TypicalValue, tag.TYPICALVALUE);
            if (tag.ZERO != null)
                attrs.Add(PICommonPointAttributes.Zero, tag.ZERO);

            if (tag.W_AF_ATTRB_SCRTY != null)
            {
                attrs.Add(PICommonPointAttributes.PointSecurity, tag.W_AF_ATTRB_SCRTY);
                attrs.Add(PICommonPointAttributes.DataSecurity, tag.W_AF_ATTRB_SCRTY);
            }


            // TODO: REMOVE_TEST
            if (withDescriptor)
                attrs.Add(PICommonPointAttributes.Descriptor, tag.PI_TAG_DESCRIPTOR);

            if (pointSourceDefinition != null)
            {
                attrs.Add(PICommonPointAttributes.PointSource, pointSourceDefinition.POINT_SRC_NAME);
                attrs.Add(PICommonPointAttributes.Location1, pointSourceDefinition.LOCATION1);
                attrs.Add(PICommonPointAttributes.Location4, pointSourceDefinition.LOCATION4);
            }

            //Override Defaults
            attrs.Add(PICommonPointAttributes.Shutdown, 0);
            attrs.Add(PICommonPointAttributes.Scan, 1);
            attrs.Add(PICommonPointAttributes.Archiving, 1);
            attrs.Add(PICommonPointAttributes.ConversionFactor, 1);
            return attrs;
        }

        public static IDictionary<string, IDictionary<string, object>> MapToPIPointDefinition(this IEnumerable<PITagDataModel> tags, bool withDescriptor = false, IDictionary<string, PointSourceDataModel> pointSourceDefinitions = null)
        {
            Dictionary<string, IDictionary<string, object>> tagsDefs = new Dictionary<string, IDictionary<string, object>>();
            foreach (var tag in tags)
            {
                if (!tagsDefs.ContainsKey(tag.ECCPI_TAG_NAME))
                    tagsDefs.Add(tag.ECCPI_TAG_NAME, tag.MapToPIPointAttributes(withDescriptor, pointSourceDefinition: pointSourceDefinitions.ContainsKey(tag.SRC_PI_SERVER_CD) ? pointSourceDefinitions[tag.SRC_PI_SERVER_CD] : null));
            }
            return tagsDefs;
        }


        public static IEnumerable<string> MapToListOfAttributePath(this IEnumerable<PITagDataModel> tags)
        {
            List<string> _pathList = new List<string>();
            foreach (var tag in tags)
            {
                _pathList.Add(tag.W_AF_ATTRB_FULL_PATH);
            }
            return _pathList;
        }
    }
}
