using ECC_PIAFServices_Layer.Abstract;
using OSIsoft.AF;
using OSIsoft.AF.PI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ECC_PIAFServices_Layer
{
    public class PIAFService : IPIAFService
    {
        public PISystem piSystem { get; set; }

        public void Connect()
        {
            PISystems piSystems = new PISystems(true);
            piSystem = piSystems["ECC-PISRV1"];
            piSystem.Connect(new NetworkCredential("administrator", "P@ssw0rd"));

            var piSrv = (new PIServers())["ECC-PISRV1"];

            var tags = PIPoint.FindPIPoints(piSrv, new List<string>() { "BRG*" });
            EventLog.WriteEntry("Area Searcher Module", string.Join(",", tags.Select(t => t.Name).ToArray()),EventLogEntryType.Information);
        }


        //TODO: Implement abstract services that communicates with PI and AF.

    }
}
