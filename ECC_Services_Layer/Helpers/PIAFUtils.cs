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
        public static PISystem piSystem { get; set; }

        //public static void TestConnection()
        //{
        //    PISystems piSystems = new PISystems(true);
        //    piSystem = piSystems.DefaultPISystem;
        //    piSystem.Connect();
        //    try
        //    {
        //        EventLog.WriteEntry("Area Searcher Module", "Connection Successful", EventLogEntryType.Information);
        //    }
        //    catch (Exception e)
        //    {
        //        EventLog.WriteEntry("Area Searcher Module", e.Message, EventLogEntryType.Error);
        //    }
        //}

        public static PIServer GetPIServer(string name)
        {
            PIServers piServers = new PIServers();
            PIServer piServer = piServers[name];
            return piServer;
        }


        //TODO: Implement abstract services that communicates with PI and AF.
    }
}
