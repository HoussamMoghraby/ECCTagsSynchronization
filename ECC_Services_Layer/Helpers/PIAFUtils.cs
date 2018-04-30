﻿using OSIsoft.AF;
using OSIsoft.AF.PI;

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

        public static PISystem GetPISystem(string name)
        {
            PISystems piSystems = new PISystems();
            PISystem piSystem = piSystems[name];
            return piSystem;
        }

    }
}
