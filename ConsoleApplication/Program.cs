using ECC_DataLayer.DataModels;
using ECC_DataLayer.Helpers;
using ECC_DataLayer.Repositories;
using ECC_DataLayer.Stores;
using ECC_PIAFServices_Layer.Services;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure(); // Added to point log4net for log4net.config

            Console.WriteLine("App Started");
            Console.WriteLine("Enter START to start the application or ENC to encrypt a password");
            var entry = Console.ReadLine();
            if (entry != null)
                switch (entry.ToString().ToLower())
                {
                    case "start":
                        StartAreaSearcherService();
                        break;
                    case "enc":
                        Console.WriteLine("Enter password to be encypted");
                        var passEntry = "Synchr0n1ze18";
                        Console.WriteLine(string.Format("The ecryption text is: {0}", encryptString(passEntry)));
                        break;
                    default:

                        break;
                }

            //RegisterJob();
            Console.WriteLine("Job End");
            Console.ReadLine();

            //read the xml file
            //var cc1 = QueryReader.ReadQuery("GetAreasPIServers");
            //var cc2 = QueryReader.ReadQuery("InsertAreaTags");
            //var cc3 = QueryReader.ReadQuery("UpdatePIServerLastPullDate");
            //Logger.Info("ConsoleECC","Test the logs 1");
            //Console.ReadLine();
        }

        public static async
        void StartAreaSearcherService()
        {
            Console.WriteLine(string.Format("Service Start at {0}", DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss")));
            var _service = new AreaSearcherService();
            await _service.Start();
            //var cc = ConnectionFactory.ConnectionString();
            //CryptoTest();
        }

        private static void RegisterJob()
        {
            ServiceJob.NextSchedule();
        }


        static void CryptoTest()
        {
            // Encrypt the string to an array of bytes.
            byte[] encrypted = CryptoProvider.Encrypt_Aes("pltr123456");
            string encString = Convert.ToBase64String(encrypted);
            byte[] encBytes = Convert.FromBase64String(encString);

            // Decrypt the bytes to a string.
            string roundtrip = CryptoProvider.Decrypt_Aes(encBytes);
        }

        static string encryptString(string text)
        {
            byte[] encrypted = CryptoProvider.Encrypt_Aes(text);
            string encString = Convert.ToBase64String(encrypted);
            return encString;
        }
    }
}
