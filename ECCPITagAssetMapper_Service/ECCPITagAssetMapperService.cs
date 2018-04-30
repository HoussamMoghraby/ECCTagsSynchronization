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

namespace ECCPITagAssetMapper_Service
{
    public partial class ECCPITagAssetMapperService : ServiceBase, IWService
    {
        private TagAssetMapperService _service = new TagAssetMapperService();
        public ECCPITagAssetMapperService()
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
            Logger.Info("ECCPITagAssetMapper", "Job Started");
            var execute = _service.StartAsync().Result;
            Logger.Info("ECCPITagAssetMapper", "Job Ended");
            InitializeSchedule();
        }

        protected override void OnStop()
        {
        }
    }
}
