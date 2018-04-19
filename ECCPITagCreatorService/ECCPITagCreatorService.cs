﻿using ECC_AFServices_Layer.Helpers;
using ECC_AFServices_Layer.Services;
using ECC_AFServices_Layer.Services.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ECCPITagCreatorService
{
    public partial class ECCPITagCreatorService : ServiceBase, IWService
    {
        private TagCreatorService _service = new TagCreatorService();
        public ECCPITagCreatorService()
        {
            log4net.Config.XmlConfigurator.Configure(); // Added to point log4net for log4net.config
            InitializeComponent();
        }

        public void InitializeSchedule()
        {
            BackgroundJob _job = new BackgroundJob(_service);
            _job.ScheduleJob();
        }

        protected override void OnStart(string[] args)
        {
            Debugger.Launch();
            // TODO: Add code here to start your service.           
            _service.Start();
            InitializeSchedule();
        }

        protected override void OnStop()
        {
        }
    }
}