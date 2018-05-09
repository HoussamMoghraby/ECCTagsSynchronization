using ECC_DataLayer.DataModels;
using ECC_DataLayer.Stores;
using OSIsoft.AF.PI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ECC_AFServices_Layer.Helpers
{
    public static class ModelsMapper 
    {

        public static PITagDataModel MapToPITagDataModel(this PIPoint piPoint, string sourcePIServerCode)
        {
            PITagDataModel _piTag = new PITagDataModel(areaTagName: piPoint.Name, tagDescriptor: (piPoint.GetAttribute(PICommonPointAttributes.Descriptor) != null ? piPoint.GetAttribute(PICommonPointAttributes.Descriptor).ToString() : null), sourcePIServerCode: sourcePIServerCode, areaPIPointId: piPoint.ID);

            //Advanced Attributes
            IDictionary<string, object> adAttrs = piPoint.GetAttributes(new string[]
            {
                PICommonPointAttributes.EngineeringUnits,
                PICommonPointAttributes.DigitalSetName,
                PICommonPointAttributes.PointType,
                PICommonPointAttributes.Location2,
                PICommonPointAttributes.Location3,
                PICommonPointAttributes.Location5,
                PICommonPointAttributes.UserInt1,
                PICommonPointAttributes.UserInt2,
                PICommonPointAttributes.UserReal1,
                PICommonPointAttributes.UserReal2,
                PICommonPointAttributes.Compressing,
                PICommonPointAttributes.CompressionDeviation,
                PICommonPointAttributes.CompressionMaximum,
                PICommonPointAttributes.CompressionMinimum,
                PICommonPointAttributes.CompressionPercentage,
                PICommonPointAttributes.ExceptionDeviation,
                PICommonPointAttributes.ExceptionMaximum,
                PICommonPointAttributes.ExceptionMinimum,
                PICommonPointAttributes.ExceptionPercentage,
                PICommonPointAttributes.Span,
                PICommonPointAttributes.Step,
                PICommonPointAttributes.TypicalValue,
                PICommonPointAttributes.Zero
            }
            );

            _piTag.ENGUNITS = adAttrs.AssignAttributeValue(PICommonPointAttributes.EngineeringUnits);
            _piTag.AREA_DIGITAL_SET = adAttrs.AssignAttributeValue(PICommonPointAttributes.DigitalSetName);
            _piTag.POINTTYPE = adAttrs.AssignAttributeValue(PICommonPointAttributes.PointType);
            _piTag.LOCATION2 = adAttrs.AssignAttributeValue(PICommonPointAttributes.Location2, isNumeric: true);
            _piTag.LOCATION3 = adAttrs.AssignAttributeValue(PICommonPointAttributes.Location3, isNumeric: true);
            _piTag.LOCATION5 = adAttrs.AssignAttributeValue(PICommonPointAttributes.Location5, isNumeric: true);
            _piTag.USERINT1 = adAttrs.AssignAttributeValue(PICommonPointAttributes.UserInt1, isNumeric: true);
            _piTag.USERINT2 = adAttrs.AssignAttributeValue(PICommonPointAttributes.UserInt2, isNumeric: true);
            _piTag.USERREAL1 = adAttrs.AssignAttributeValue(PICommonPointAttributes.UserReal1, isNumeric: true);
            _piTag.USERREAL2 = adAttrs.AssignAttributeValue(PICommonPointAttributes.UserReal2, isNumeric: true);
            _piTag.COMPRESSING = adAttrs.AssignAttributeValue(PICommonPointAttributes.Compressing);
            _piTag.COMPDEV = adAttrs.AssignAttributeValue(PICommonPointAttributes.CompressionDeviation, isNumeric: true);
            _piTag.COMPMAX = adAttrs.AssignAttributeValue(PICommonPointAttributes.CompressionMaximum, isNumeric: true);
            _piTag.COMPMIN = adAttrs.AssignAttributeValue(PICommonPointAttributes.CompressionMinimum, isNumeric: true);
            _piTag.COMPDEVPERCENT = adAttrs.AssignAttributeValue(PICommonPointAttributes.CompressionPercentage, isNumeric: true);
            _piTag.EXCDEV = adAttrs.AssignAttributeValue(PICommonPointAttributes.ExceptionDeviation, isNumeric: true);
            _piTag.EXCMAX = adAttrs.AssignAttributeValue(PICommonPointAttributes.ExceptionMaximum, isNumeric: true);
            _piTag.EXCMIN = adAttrs.AssignAttributeValue(PICommonPointAttributes.ExceptionMinimum, isNumeric: true);
            _piTag.EXCDEVPERCENT = adAttrs.AssignAttributeValue(PICommonPointAttributes.ExceptionPercentage, isNumeric: true);
            _piTag.SPAN = adAttrs.AssignAttributeValue(PICommonPointAttributes.Span, isNumeric: true);
            _piTag.STEP = adAttrs.AssignAttributeValue(PICommonPointAttributes.Step, isNumeric: true);
            _piTag.TYPICALVALUE = adAttrs.AssignAttributeValue(PICommonPointAttributes.TypicalValue, isNumeric: true);
            _piTag.ZERO = adAttrs.AssignAttributeValue(PICommonPointAttributes.Zero, isNumeric: true);

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


            if (tag.AREA_PI_TAG_NAME != null)
                attrs.Add(PICommonPointAttributes.InstrumentTag, tag.AREA_PI_TAG_NAME);
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

            if (withDescriptor)
                attrs.Add(PICommonPointAttributes.Descriptor, tag.PI_TAG_DESCRIPTOR);

            if (pointSourceDefinition != null)
            {
                attrs.Add(PICommonPointAttributes.PointSource, pointSourceDefinition.POINT_SRC_NAME);
                attrs.Add(PICommonPointAttributes.Location1, pointSourceDefinition.LOCATION1);
                attrs.Add(PICommonPointAttributes.Location4, pointSourceDefinition.LOCATION4);
            }

            //Set Defaults
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
