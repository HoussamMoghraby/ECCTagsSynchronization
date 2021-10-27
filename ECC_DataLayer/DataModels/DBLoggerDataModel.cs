using System;

namespace ECC_DataLayer.DataModels
{
    public class DbLoggerDetailsDataModel
    {
        public int EASL_NUM { get; set; }
        public long EAWFT_NUM { get; set; }
        public string ECCPI_TAG_NAME { get; set; }
        public string AREA_PI_TAG_NAME { get; set; }
        public string SRC_PI_SERVER_CD { get; set; }
        public int EASR_NUM { get; set; }
        public string SVC_NAME { get; set; }
        public string SVC_MSG { get; set; }
        public string SVC_MSG_TYP { get; set; }
        public string SVC_MSG_SEVIRITY { get; set; }
        public int AREA_POINT_ID { get; set; }
        public string SVC_PROPOSED_REMEDY { get; set; }
    }

    public class DbLoggerDataModel
    {
        public DbLoggerDataModel()
        {
            REMARKS = string.Empty;
        }
        public int EASR_NUM { get; set; }
        public string SVC_NAME { get; set; }
        public DateTime SVC_START_DT { get; set; }
        public DateTime SVC_END_DT { get; set; }
        public string REMARKS { get; set; }
        public string SVC_STATUS { get; set; }
    }

    public class Severity
    {
        public static string Critical = "Critical";
        public static string Exception = "Exception";
        public static string Error = "Error";
        public static string Warning = "Warning";
        public static string Information = "Information";
        public static string Debug = "Debug";
    }

    public class svcType
    {
        public static string Tag = "Tag";
        public static string Template = "Template";
        public static string Attribute = "Attribute";
    }
    public class Status
    {
        public static string Running = "Running";
        public static string Succeed = "Succeed";
        public static string Fail = "Fail";

    }
}