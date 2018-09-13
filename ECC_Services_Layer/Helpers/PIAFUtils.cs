using OSIsoft.AF;
using OSIsoft.AF.PI;
using System;
using System.Linq;

namespace ECC_PIAFServices_Layer.Helpers
{
    public static class PIAFUtils
    {
        public static PISystem piSystem { get; set; }

        public static PIServer GetPIServer(string name)
        {
            PIServers piServers = new PIServers();
            PIServer piServer = piServers[name];
            if (piServer != null)
            {
                try
                {
                    PICollectiveMember pMember = piServer.Collective.Members[0];
                    pMember.ConnectionTimeOut = new TimeSpan(1, 0, 0);
                }
                catch (Exception)
                {
                }
            }
            return piServer;
        }

        public static PISystem GetPISystem(string name)
        {
            PISystems piSystems = new PISystems();
            PISystem piSystem = piSystems[name];
            try
            {
                piSystem.ConnectionInfo.TimeOut = new TimeSpan(1, 0, 0);
            }
            catch (Exception)
            {

            }
            return piSystem;
        }

        internal static AFPlugIn GetDataReferencePlugin(PISystem piSystem, string dataReferenceName = DataReference.PIPoint)
        {
            var dataReferences = piSystem.DataReferencePlugIns.Where(dr => dr.Name.ToLower() == dataReferenceName.ToLower());
            return dataReferences.FirstOrDefault();
        }

        public static class DataReference
        {
            public const string PIPoint = "PI Point";
            public const string PIPointArray = "PI Point Array";
            public const string Formula = "Formula";
        }
    }
}
