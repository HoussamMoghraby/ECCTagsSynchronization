using ECC_AFServices_Layer.Helpers;
using ECC_AFServices_Layer.Services;
using ECC_AFServices_Layer.Services.Abstract;
using System;

namespace ConsoleAppTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TagCreatorService _service = new TagCreatorService();
            //InitializeSchedule(_service);
            var result = _service.StartAsync().Result;
            Console.WriteLine("Execution Result = " + result.ToString());
            Console.ReadLine();
        }

        public static void InitializeSchedule(IECCService _service)
        {
            BackgroundJob _job = new BackgroundJob(_service);
            _job.ScheduleJob();
        }
    }
}
