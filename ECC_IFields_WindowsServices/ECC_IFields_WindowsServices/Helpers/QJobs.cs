﻿using ECC_AFServices_Layer.Services.Abstract;
using ECC_PIAFServices_Layer.Services;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECC_IFields_Services.Helpers
{
    public class QJobs
    {
        public static IECCService _serviceInstance;

        public QJobs(IECCService serviceInstance)
        {
            _serviceInstance = serviceInstance;
        }

        public class JobWrapper : IJob
        {
            async Task IJob.Execute(IJobExecutionContext context)
            {
                //run the service
                //AreaSearcherService _serviceInstance = new AreaSearcherService();
                Debugger.Launch();
                await _serviceInstance.Start();
            }
        }

        public async void ScheduleJob()
        {
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




        //public static void ScheduleJobs(string serviceName)
        //{
        //    //TODO: Schedule job for taget service  
        //}

    }
}
