using ECC_DataLayer.DataModels;
using OSIsoft.AF.PI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECC_AFServices_Layer.Helpers
{
    public static class ModelsMapper //TODO: remove this and use automapper instead
    {
        public static PITagDataModel MapToPITagDataModel(this PIPoint piPoint, string sourcePIServerCode)
        {
            PITagDataModel _piTag = new PITagDataModel(areaTagName: piPoint.Name, tagDescriptor: (piPoint.GetAttribute("Descriptor") != null ? piPoint.GetAttribute("Descriptor").ToString() : null), sourcePIServerCode: sourcePIServerCode, areaPIPointId: piPoint.ID);

            //Advanced Attributes
            IDictionary<string, object> adAttrs = piPoint.GetAttributes(new string[] { "engunits", "digitalset", "pointtype", "location2", "location3", "location5", "userint1", "userint2", "userreal1", "userreal2", "compressing", "compdev", "compmax", "compmin", "compdevpercent", "excdev", "excmax", "excmin", "excdevpercent", "span", "step", "typicalvalue", "zero" });
            if (adAttrs["engunits"] != null)
                _piTag.ENGUNITS = adAttrs["engunits"].ToString();
            if (adAttrs["digitalset"] != null)
                _piTag.ECCPI_DIGITAL_SET = adAttrs["digitalset"].ToString();
            if (adAttrs["pointtype"] != null)
                _piTag.POINTTYPE = adAttrs["pointtype"].ToString();
            if (adAttrs["location2"] != null)
                _piTag.LOCATION2 = Convert.ToDecimal(adAttrs["location2"]);
            if (adAttrs["location3"] != null)
                _piTag.LOCATION3 = Convert.ToDecimal(adAttrs["location3"]);
            if (adAttrs["location5"] != null)
                _piTag.LOCATION5 = Convert.ToDecimal(adAttrs["location5"]);
            if (adAttrs["userint1"] != null)
                _piTag.USERINT1 = Convert.ToDecimal(adAttrs["userint1"]);
            if (adAttrs["userint2"] != null)
                _piTag.USERINT2 = Convert.ToDecimal(adAttrs["userint2"]);
            if (adAttrs["userreal1"] != null)
                _piTag.USERREAL1 = Convert.ToDecimal(adAttrs["userreal1"]);
            if (adAttrs["userreal2"] != null)
                _piTag.USERREAL2 = Convert.ToDecimal(adAttrs["userreal2"]);
            if (adAttrs["compressing"] != null)
                _piTag.COMPRESSING = adAttrs["compressing"].ToString();
            if (adAttrs["compdev"] != null)
                _piTag.COMPDEV = Convert.ToDecimal(adAttrs["compdev"]);
            if (adAttrs["compmax"] != null)
                _piTag.COMPMAX = Convert.ToDecimal(adAttrs["compmax"]);
            if (adAttrs["compmin"] != null)
                _piTag.COMPMIN = Convert.ToDecimal(adAttrs["compmin"]);
            if (adAttrs["compdevpercent"] != null)
                _piTag.COMPDEVPERCENT = Convert.ToDecimal(adAttrs["compdevpercent"]);
            if (adAttrs["excdev"] != null)
                _piTag.EXCDEV = Convert.ToDecimal(adAttrs["excdev"]);
            if (adAttrs["excmax"] != null)
                _piTag.EXCMAX = Convert.ToDecimal(adAttrs["excmax"]);
            if (adAttrs["excmin"] != null)
                _piTag.EXCMIN = Convert.ToDecimal(adAttrs["excmin"]);
            if (adAttrs["excdevpercent"] != null)
                _piTag.EXCDEVPERCENT = Convert.ToDecimal(adAttrs["excdevpercent"]);
            if (adAttrs["span"] != null)
                _piTag.SPAN = Convert.ToDecimal(adAttrs["span"]);
            if (adAttrs["step"] != null)
                _piTag.STEP = Convert.ToDecimal(adAttrs["step"]);
            if (adAttrs["typicalvalue"] != null)
                _piTag.TYPICALVALUE = Convert.ToDecimal(adAttrs["typicalvalue"]);
            if (adAttrs["zero"] != null)
                _piTag.ZERO = Convert.ToDecimal(adAttrs["zero"]);
            return _piTag;
        }
    }
}
