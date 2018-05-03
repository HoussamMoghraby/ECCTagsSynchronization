using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECC_DataLayer.DataModels
{
    public class PITagDataModel
    {

        public long EAWFT_NUM { get; set; }
        public string ECCPI_TAG_NAME { get; set; }
        public string ECCPI_TAG_CRE_FLG { get; set; }
        //Basic Attributes
        public string AREA_PI_TAG_NAME { get; set; }
        public string PI_TAG_DESCRIPTOR { get; set; }
        public string SRC_PI_SERVER_CD { get; set; }
        public int AREA_POINT_ID { get; set; }

        public int? ECCPI_POINT_ID { get; set; }

        //Advanded Attributes
        public string AREA_DIGITAL_SET { get; set; }
        public string ECCPI_DIGITAL_SET { get; set; }
        public string ENGUNITS { get; set; }
        public string POINTTYPE { get; set; }
        public decimal? LOCATION2 { get; set; }
        public decimal? LOCATION3 { get; set; }
        public decimal? LOCATION5 { get; set; }
        public decimal? USERINT1 { get; set; }
        public decimal? USERINT2 { get; set; }
        public decimal? USERREAL1 { get; set; }
        public decimal? USERREAL2 { get; set; }
        public string COMPRESSING { get; set; }
        public decimal? COMPDEV { get; set; }
        public decimal? COMPMAX { get; set; }
        public decimal? COMPMIN { get; set; }
        public decimal? COMPDEVPERCENT { get; set; }
        public decimal? EXCDEV { get; set; }
        public decimal? EXCMAX { get; set; }
        public decimal? EXCMIN { get; set; }
        public decimal? EXCDEVPERCENT { get; set; }
        public decimal? SPAN { get; set; }
        public decimal? STEP { get; set; }
        public decimal? TYPICALVALUE { get; set; }
        public decimal? ZERO { get; set; }

        public string W_AF_ATTRB_SCRTY { get; set; }

        public string W_AF_ATTRB_FULL_PATH { get; set; }
        public string W_GNR_NAME { get; set; }
        public string W_EQP_ATTRIB_NAME { get; set; }


        public string PI_SERVER_NAME { get; set; }
        public string PI_TAG_FLOW_FLG { get; set; }

        public string ECCPI_AF_MAPPED_FLG { get; set; }

        public string ECCPI_EXST_TAG_NAME { get; set; }

        public string ECCPI_TAG_REN_RQST_FLG { get; set; }

        public string ECCPI_TAG_HAS_REN_FLG { get; set; }

        public string ECCPI_AF_MAP_OVR_FLG { get; set; } = "N";

        public string ECCPI_AF_MAP_REM { get; set; }

        public bool? IsValidForAssetMapping { get; set; } = null;

        public PITagDataModel()
        {
            //Parameterless contructor
        }

        public PITagDataModel(string areaTagName, string tagDescriptor, string sourcePIServerCode, int areaPIPointId)
        {
            this.AREA_PI_TAG_NAME = areaTagName.Replace("'", "");
            this.PI_TAG_DESCRIPTOR = tagDescriptor.Replace("'", "");
            this.SRC_PI_SERVER_CD = sourcePIServerCode.Replace("'", "");
            this.AREA_POINT_ID = areaPIPointId;
        }


        public IDictionary<string, object> GetAttributes()
        {
            var tag = this;
            IDictionary<string, object> attrs = new Dictionary<string, object>();
            attrs.Add("descriptor", "CME-TEST");
            attrs.Add("engunits", 999);
            return attrs;
        }
    }
}
