using OSIsoft.AF;
using OSIsoft.AF.PI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECC_PIAFServices_Layer.Helpers
{
    public static class PIAFUtils
    {
        //TODO: Implement abstract services that communicates with PI and AF.

        public static PISystem piSystem { get; set; }

        public static PIServer GetPIServer(string name)
        {
            PIServers piServers = new PIServers();
            PIServer piServer = piServers[name];
            return piServer;
        }
        
    }
}
