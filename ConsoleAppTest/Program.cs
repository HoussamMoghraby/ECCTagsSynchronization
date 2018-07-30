using ECC_AFServices_Layer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TagAssetMapperService _service = new TagAssetMapperService();
            var result = _service.StartAsync().Result;
            Console.WriteLine("Execution Result = " + result.ToString());
            Console.ReadLine();
        }
    }
}
