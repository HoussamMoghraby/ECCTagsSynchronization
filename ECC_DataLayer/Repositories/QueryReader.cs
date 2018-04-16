using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ECC_DataLayer.Repositories
{
    public class QueryReader
    {
        public static string ReadQuery(string name)
        {

            string _query = string.Empty;
            //read the xml file
            var appDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            XElement rootElement = XElement.Load(appDirectory + @"SQLQueries.xml");
            var queryNodeSearch = rootElement.Elements().Where(e => e.Name == "query" && (e.Attribute("name") != null && e.Attribute("name").Value.ToString().ToLower() == name.ToLower()));
            if (queryNodeSearch != null && queryNodeSearch.Count() > 0)
                _query = queryNodeSearch.FirstOrDefault().Value;
            else
                throw new ApplicationException("Query not found in xml file!");

            return _query;
        }
    }
}
