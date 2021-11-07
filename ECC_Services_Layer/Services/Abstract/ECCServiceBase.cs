using ECC_DataLayer.Helpers;

namespace ECC_AFServices_Layer.Services.Abstract
{
    public abstract class ECCServiceBase
    {
        internal DbLogger _dbLogger;

        public ECCServiceBase()
        {
            _dbLogger = new DbLogger(ServiceName);
        }

        public string ServiceName
        {
            get { return this.GetType().Name; }
            set { }
        }

        public void LogServiceStart()
        {
            var id = _dbLogger.LogStart();
            DbLogger._dbLoggerDataModel.EASR_NUM = id;
            Logger.Info(ServiceName, "Job Started");
        }

        public void LogServiceEnd()
        {
            Logger.Info(ServiceName, "Job End");
            _dbLogger.LogEnd();
        }
    }
}