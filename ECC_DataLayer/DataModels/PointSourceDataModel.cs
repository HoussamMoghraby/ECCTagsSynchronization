using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECC_DataLayer.DataModels
{
    public class PointSourceDataModel
    {
        public int EPS_NUM { get; set; }
        public string PI_SERVER_CD { get; set; }
        public string POINT_SRC_NAME { get; set; }
        public string LOCATION1 { get; set; }
        public string LOCATION4 { get; set; }
        public long NUM_OF_TAGS { get; set; }
    }
}
