using ECC_IFields_WindowsServices;
using System.ServiceProcess;

namespace ECC_IFields_Services
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ECCPIAreaSearcher(),
                new ECCPITagCreator()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
