using ECC_AFServices_Layer.Services;
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
using System.Text.RegularExpressions;
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
                    case "area_searcher":
                        var cc = new AreaSearcherService();
                        var sn = cc.ServiceName;
                        StartAreaSearcherService();
                        break;
                    case "tag_creator":
                        TestTagCreator();
                        break;
                    case "asset_mapper":
                        TestAssetMapper();
                        break;
                    case "value_checker":
                        TestValueChecker();
                        break;
                    case "enc":
                        Console.WriteLine("Enter password to be encypted");
                        var passEntry = "ecc123321A";
                        Console.WriteLine(string.Format("The ecryption text is: {0}", encryptString(passEntry)));
                        break;
                    case "test_numbers":
                        TestNumbersEquality();
                        break;
                    case "testoracle":
                        Console.WriteLine("Testing Orcacle Connection");
                        var conn = ConnectionFactory.GetConnection();
                        var res = (new AreaSearcherStore()).GetAreasPIServers().Result;
                        Console.WriteLine(conn.State);
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

        private static void TestNumbersEquality()
        {
            dynamic ECCValue, AreaValue;
            ECCValue = "No Data";
            AreaValue = "No Data";
            string _matchingRemark = null;
            bool _isValuesMatching = false;
            var isECCNumber = Regex.IsMatch(ECCValue.ToString(), @"\d");
            var isAreaNumber = Regex.IsMatch(AreaValue.ToString(), @"\d");
            if (isECCNumber && isAreaNumber)
                _isValuesMatching = Math.Round(ECCValue, 2) == Math.Round(AreaValue, 2);
            else if (!isECCNumber && !isAreaNumber)
            {
                _isValuesMatching = ECCValue.ToString() == AreaValue.ToString();
                if (_isValuesMatching)
                    _matchingRemark = string.Format("Matching with no value: {0}", ECCValue.ToString());
            }
            char _matchingStatus = _isValuesMatching ? 'Y' : 'N';
            Console.WriteLine("Value Matching? = " + _isValuesMatching);
        }

        private static void TestValueChecker()
        {
            var _service = new TagValueCheckerService();
            var exec = _service.StartAsync().Result;
        }

        private static void TestAssetMapper()
        {
            var _service = new TagAssetMapperService();
            var exec = _service.StartAsync().Result;
        }

        private static void TestTagCreator()
        {
            var service = new TagCreatorService();
            var cc = service.StartAsync().Result;
        }

        public static async
        void StartAreaSearcherService()
        {
            Console.WriteLine(string.Format("Service Start at {0}", DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss")));
            var _service = new AreaSearcherService();
            await _service.StartAsync();
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
            byte[] encrypted = CryptoProvider.Encrypt_Aes("ecc123321A");
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
