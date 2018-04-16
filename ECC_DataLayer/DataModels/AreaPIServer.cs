using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECC_DataLayer.DataModels
{
    public class AreaPIServer
    {
        public string PI_SERVER_NAME { get; set; }
        public string PI_SERVER_DESC { get; set; }
        public DateTime EP_CR_DATA_DT { get; set; }
        public string EP_CR_DATA_CD { get; set; }
        public DateTime EP_LT_DATA_MOD_DT { get; set; }
        public string EP_LT_DMO_CD { get; set; }
        public string PI_SERVER_CD { get; set; }
        public DateTime? PI_LAST_TAG_PULL_DT { get; set; }
    }
}
