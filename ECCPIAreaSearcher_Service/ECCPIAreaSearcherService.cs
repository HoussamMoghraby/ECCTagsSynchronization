using ECC_AFServices_Layer.Helpers;
using ECC_AFServices_Layer.Services.Abstract;
using ECC_DataLayer.Helpers;
using ECC_PIAFServices_Layer.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ECCPIAreaSearcher_Service
{
    partial class ECCPIAreaSearcherService : ServiceBase, IWService
    {
        private AreaSearcherService _service = new AreaSearcherService();
        public ECCPIAreaSearcherService()
        {
            Logger.Initialize();
            InitializeComponent();
        }

        public void InitializeSchedule()
        {
            BackgroundJob _job = new BackgroundJob(_service);
            _job.ScheduleJob();
        }

        protected override void OnStart(string[] args)
        {
            //Debugger.Launch();
            // TODO: Add code here to start your service.       
            Logger.Info(_service.ServiceName, "Job Started");
            var execute = _service.StartAsync().Result;
            Logger.Info(_service.ServiceName, "Job Ended");
            InitializeSchedule();
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }
    }
}
