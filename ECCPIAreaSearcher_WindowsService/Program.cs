﻿using ECCPIAreaSearcher_Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ECCPIAreaSearcher_WindowsService
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
                new ECCPIAreaSearcherService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
