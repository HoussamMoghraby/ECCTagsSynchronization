using ECC_AFServices_Layer.Services;
using ECC_IFields_Services.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ECC_IFields_WindowsServices
{
    partial class ECCPITagCreator : ServiceBase, IWService
    {
        private TagCreatorService _service = new TagCreatorService();
        public ECCPITagCreator()
        {
            log4net.Config.XmlConfigurator.Configure(); // Added to point log4net for log4net.config
            InitializeComponent();
        }

        public void InitializeSchedule()
        {
            QJobs _job = new QJobs(_service);
            _job.ScheduleJob(/*_service*/);
        }

        protected override void OnStart(string[] args)
        {
            Debugger.Launch();
            InitializeSchedule();
            // TODO: Add code here to start your service.           
            _service.Start();
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }
    }
}
