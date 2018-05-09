using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECC_AFServices_Layer.Services.Abstract
{
    public abstract class ECCServiceBase
    {
        public string ServiceName
        {
            get { return this.GetType().Name; }
            set { }
        }
    }
}
