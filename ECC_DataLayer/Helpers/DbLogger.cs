using ECC_DataLayer.DataModels;
using ECC_DataLayer.Stores;

namespace ECC_DataLayer.Helpers
{
    public class DbLogger
    {
        private readonly string _serviceName;
        private readonly DbLoggerStore _dbLoggerStore;
        public static DbLoggerDataModel _dbLoggerDataModel;

        public DbLogger(string serviceName)
        {
            _serviceName = serviceName;
            _dbLoggerStore=new DbLoggerStore();
            _dbLoggerDataModel = new DbLoggerDataModel();
        }

        public int LogStart()
        {
            _dbLoggerDataModel = new DbLoggerDataModel();
            return _dbLoggerStore.ExecuteDbLogStartServiceQuery(_serviceName,Status.Running, "Service is currently running");
        }
        public async void LogEnd()
        {
            await _dbLoggerStore.ExecuteDbLogEndServiceQuery(_dbLoggerDataModel.EASR_NUM, _dbLoggerDataModel.SVC_STATUS,
                _dbLoggerDataModel.REMARKS);
        }
    }

    public class DbLoggerDetails
    {
        private readonly string _serviceName;
        private readonly DbLoggerStore _dbLoggerStore;

        public DbLoggerDetails(string serviceName)
        {
            _serviceName = serviceName;
            _dbLoggerStore = new DbLoggerStore();
        }

        public async void Log(DbLoggerDetailsDataModel dbLoggerDetailsDataModel)
        {
            dbLoggerDetailsDataModel.SVC_NAME = _serviceName;
            dbLoggerDetailsDataModel.EASR_NUM = DbLogger._dbLoggerDataModel.EASR_NUM;
            await _dbLoggerStore.ExecuteAddDbLogDetails(dbLoggerDetailsDataModel);
        }
    }
}
