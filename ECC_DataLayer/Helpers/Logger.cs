using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ECC_DataLayer.Helpers
{
    public static class Logger
    {
        private static bool _logActivated = true;
        private static readonly ILog _txtLogger = LogManager.GetLogger
            (MethodBase.GetCurrentMethod().DeclaringType);

        public static void Initialize()
        {
            //log4net.Config.XmlConfigurator.Configure(); // Added to point log4net for log4net.config
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo("log4net.config"));
        }

        //public static void Refresh()
        //{
        //    log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo("log4net.config"));
        //}

        public static void Info(string source, string message)
        {
            _txtLogger.Info(message);
            if (_logActivated)
                try
                {
                    EventLog.WriteEntry(source, message, EventLogEntryType.Information);
                }
                catch (Exception e)
                {
                    _txtLogger.Error(e.Message, e);
                }
        }

        public static void Error(string source, Exception ex)
        {
            _txtLogger.Error(ex.Message, ex);
            if (_logActivated)
                try
                {
                    EventLog.WriteEntry(source, ex.Message, EventLogEntryType.Error);
                }
                catch (Exception e)
                {
                    _txtLogger.Error(e.Message, e);
                }
        }

        public static void Warning(string source, string message)
        {
            _txtLogger.Warn(message);
            if (_logActivated)
                try
                {
                    EventLog.WriteEntry(source, message, EventLogEntryType.Warning);
                }
                catch (Exception e)
                {
                    _txtLogger.Error(e.Message, e);
                }
        }
    }
}
