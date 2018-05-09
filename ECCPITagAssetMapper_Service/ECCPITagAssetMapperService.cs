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
            Logger.Info(_service.ServiceName, "Job Started");
            var execute = _service.StartAsync().Result;
            Logger.Info(_service.ServiceName, "Job Ended");
            InitializeSchedule();
        }

        protected override void OnStop()
        {
        }
    }
}
