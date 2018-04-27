﻿using ECC_AFServices_Layer.Helpers;
using ECC_AFServices_Layer.Services;
using ECC_AFServices_Layer.Services.Abstract;
using ECC_DataLayer.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ECCPITagValueChecker_Service
{
    public partial class ECCPITagValueCheckerService : ServiceBase, IWService
    {
        private TagValueCheckerService _service = new TagValueCheckerService();
        public ECCPITagValueCheckerService()
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
            Debugger.Launch();
            // TODO: Add code here to start your service.   
            Logger.Info("ECCPITagValueChecker", "Job Started");
            _service.StartAsync();
            Logger.Info("ECCPITagValueChecker", "Job Ended");
            InitializeSchedule();
        }

        protected override void OnStop()
        {
        }
    }
}
