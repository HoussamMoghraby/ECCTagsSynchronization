using ECC_AFServices_Layer.Helpers;
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

namespace ECCPITagCreatorService
{
    public partial class ECCPITagCreatorService : ServiceBase, IWService
    {
        private TagCreatorService _service = new TagCreatorService();
        public ECCPITagCreatorService()
        {
            //Debugger.Launch();
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
            var execute = _service.StartAsync().Result;
            InitializeSchedule();
        }

        protected override void OnStop()
        {
        }
    }
}
