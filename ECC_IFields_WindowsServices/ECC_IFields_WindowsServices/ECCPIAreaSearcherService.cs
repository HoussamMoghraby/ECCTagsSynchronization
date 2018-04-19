using ECC_AFServices_Layer.Helpers;
using ECC_AFServices_Layer.Services.Abstract;
using ECC_PIAFServices_Layer.Services;
using System.Diagnostics;
using System.ServiceProcess;

namespace ECC_IFields_Services
{
    partial class ECCPIAreaSearcher : ServiceBase, IWService
    {
        private AreaSearcherService _service = new AreaSearcherService();
        public ECCPIAreaSearcher()
        {
            log4net.Config.XmlConfigurator.Configure(); // Added to point log4net for log4net.config
            InitializeComponent();
        }

        public void InitializeSchedule()
        {
            BackgroundJob _job = new BackgroundJob(_service);
            _job.ScheduleJob();
        }

        /// <summary>
        /// Loop the tag masks listed in the table ECCPI_AF_WELL_EQP_TAG_MASKS that were not processed. for each tag mask, the service will scan a specific area PI server for this tag and find the tag that satisfies the tag mask.
        /// Some fields the service searches well number in the tag description
        /// the found tags, sometime nultiple and their description will be copied from the source PI server and stored in table ECCPI_AF_WELL_FOUND_TAGS
        /// </summary>
        /// <param name="args"></param>
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
