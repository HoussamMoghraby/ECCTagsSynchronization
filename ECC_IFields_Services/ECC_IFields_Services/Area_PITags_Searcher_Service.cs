using ECC_IFields_Services.Helpers;
using ECC_PIAFServices_Layer;
using ECC_PIAFServices_Layer.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ECC_IFields_Services
{
    partial class Area_PITags_Searcher_Service : ServiceBase, IWService
    {
        public Area_PITags_Searcher_Service()
        {
            InitializeComponent();
            Debugger.Launch();
            InitializeSchedule();
        }

        public void InitializeSchedule()
        {
            QJobs.ScheduleJobs(ServiceName);
        }

        /// <summary>
        /// Get the tags from areas servers and store them in Oracle table in database updating the flags required.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            // TODO: Add code here to start your service.
            Debugger.Launch();
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }
    }
}
