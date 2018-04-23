using ECC_AFServices_Layer.Helpers;
using ECC_AFServices_Layer.Services;
using ECC_AFServices_Layer.Services.Abstract;
using ECC_DataLayer.Helpers;
using System.Diagnostics;
using System.ServiceProcess;

namespace ECC_IFields_WindowsServices
{
    partial class ECCPITagCreator : ServiceBase, IWService
    {
        private TagCreatorService _service = new TagCreatorService();
        public ECCPITagCreator()
        {
            //log4net.Config.XmlConfigurator.Configure(); // Added to point log4net for log4net.config
            Debugger.Launch();
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
            _service.Start();
            InitializeSchedule();
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }
    }
}
