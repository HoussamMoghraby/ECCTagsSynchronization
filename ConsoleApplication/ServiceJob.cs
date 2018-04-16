using ECC_PIAFServices_Layer.Services;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    public class ServiceJob
    {

        public class JobWrapper : IJob
        {
            async Task IJob.Execute(IJobExecutionContext context)
            {
                //var _service = new AreaSearcherService();
                //await _service.Start();
                Program.StartAreaSearcherService();
            }
        }

        public static async void NextSchedule()
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();
            IJobDetail job = JobBuilder.Create<JobWrapper>().Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("ServiceJob", "ServiceNextSchedule")
                .WithCronSchedule(ConfigurationSettings.AppSettings.Get("SERVICE_RUN_SCHEDULE")) // @1:00AM schedule
                //.StartAt(DateTime.UtcNow)
                .WithPriority(1)
                .Build();
            await scheduler.ScheduleJob(job, trigger);
        }
    }
}
