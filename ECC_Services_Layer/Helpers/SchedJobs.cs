using ECC_AFServices_Layer.Services.Abstract;
using ECC_DataLayer.Helpers;
using Quartz;
using Quartz.Impl;
using System.Configuration;
using System.Threading.Tasks;

namespace ECC_AFServices_Layer.Helpers
{
    public class BackgroundJob
    {
        public static IECCService _serviceInstance;
        

        public BackgroundJob(IECCService serviceInstance)
        {
            _serviceInstance = serviceInstance;
        }

        public class JobWrapper : IJob
        {
            async Task IJob.Execute(IJobExecutionContext context)
            {
                // run the service
                await _serviceInstance.StartAsync();
            }
        }

        public async void ScheduleJob()
        {
            Logger.Info("","Next execution is scheduled");
            Quartz.Logging.LogProvider.IsDisabled = true;
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();
            IJobDetail job = JobBuilder.Create<JobWrapper>().Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("ServiceSJob", "ServiceNextSSchedule")
                .WithCronSchedule(GetConfigurationSchedule()) // @1:00AM schedule
                                                              //.StartAt(DateTime.UtcNow)
                .WithPriority(1)
                .Build();
            await scheduler.ScheduleJob(job, trigger);
        }

        private static string GetConfigurationSchedule()
        {
            //return ConfigurationSettings.AppSettings.Get("SERVICE_RUN_SCHEDULE");
            string _runFrequency = "daily";
            string _cronSchedule = "0 0 0 1/1 * ? *";
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings.Get("RUN_FREQUENCY")))
            {
                _runFrequency = ConfigurationSettings.AppSettings.Get("RUN_FREQUENCY");

                string _hour = ConfigurationSettings.AppSettings.Get("RUN_HOUR");
                string _minute = ConfigurationSettings.AppSettings.Get("RUN_MINUTE");
                switch (_runFrequency.ToLower())
                {
                    case "daily":
                        _cronSchedule = string.Format("0 {0} {1} 1/1 * ? *", _minute, _hour);
                        break;
                    case "weekly":
                        _cronSchedule = string.Format("0 {0} {1} ? * SUN *", _minute, _hour);
                        break;
                    case "monthly":
                        _cronSchedule = string.Format("0 {0} {1} 1 1/1 ? *", _minute, _hour);
                        break;
                    case "yearly":
                        _cronSchedule = string.Format("0 {0} {1} 1 1 ? *", _minute, _hour);
                        break;
                    case "minute":
                        _cronSchedule = "0 0/1 * 1/1 * ? *";
                        break;
                    default:
                        break;
                }
            }
            return _cronSchedule;

        }       

    }
}
