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
                Logger.Info("ScheduleJob", "Next execution is scheduled");
            }
        }

        public async void ScheduleJob()
        {
            Logger.Info("ScheduleJob", "Next execution is scheduled");
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
            var useCronSetting = ConfigurationSettings.AppSettings.Get("USE_CRON");
            if (!string.IsNullOrEmpty(useCronSetting) &&
                bool.TryParse(useCronSetting, out bool useCron) &&
                useCron)
            {
                Logger.Info("ScheduleJob", "USE_CRON equal true");
                var scheduleCronSetting = ConfigurationSettings.AppSettings.Get("RUN_SCHEDULE_CRON");
                if (!string.IsNullOrEmpty(scheduleCronSetting) &&
                    CronExpression.IsValidExpression(scheduleCronSetting))
                {
                    return scheduleCronSetting;
                }
                Logger.Warning("ScheduleJob", "RUN_SCHEDULE_CRON is invalid");
            }

            string _cronSchedule = "0 0 0 1/1 * ? *";
            string _runFrequency = "daily";
            string _hour = "0";
            string _minute = "0";
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings.Get("RUN_FREQUENCY")))
                _runFrequency = ConfigurationSettings.AppSettings.Get("RUN_FREQUENCY");
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings.Get("RUN_HOUR")))
                _hour = ConfigurationSettings.AppSettings.Get("RUN_HOUR");
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings.Get("RUN_MINUTE")))
                _minute = ConfigurationSettings.AppSettings.Get("RUN_MINUTE");
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
            return _cronSchedule;
        }
    }
}