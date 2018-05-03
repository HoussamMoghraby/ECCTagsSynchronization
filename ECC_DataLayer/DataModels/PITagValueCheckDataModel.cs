using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECC_DataLayer.DataModels
{
    public class PITagValueCheckDataModel : PITagDataModel
    {
        public dynamic AreaServerValue { get; set; }
        public dynamic ECCServerValue { get; set; }
        public bool IsValuesMatching { get; set; }
        public string MatchingValuesRemark { get; set; }
    }
}
