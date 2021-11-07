using ECC_DataLayer.Helpers;
using ECC_DataLayer.Repositories;
using ECC_DataLayer.Stores.Abstract;
using System;
using System.Threading.Tasks;
using ECC_DataLayer.DataModels;

namespace ECC_DataLayer.Stores
{
    public class DbLoggerStore : DataStore
    {
        public int ExecuteDbLogStartServiceQuery(string serviceName, string remarks)
        {
            try
            {
                string query = string.Format(QueryReader.ReadQuery("DbLogStartServiceQuery"),
                    ResolveQueryParam(serviceName),
                    ResolveQueryParam(remarks));

                var id = _repo.InsertAndReturnId(query);
                return id;
            }
            catch (Exception e)
            {
                Logger.Error("DbLoggerStore.ExecuteDbLogStartServiceQuery", e);
                return 0;
            }
        }

        public async Task<int> ExecuteDbLogEndServiceQuery(int id, string remarks)
        {
            try
            {
                string query = string.Format(QueryReader.ReadQuery("DbLogEndServiceQuery"),
                    ResolveQueryParam(SafeLogMessage(remarks)),
                    ResolveQueryParam(id));
                await _repo.ExecuteScalarAsync(query, new { });
                await Commit();
                return 1;
            }
            catch (Exception e)
            {
                Logger.Error("DbLoggerStore.ExecuteDbLogEndServiceQuery", e);
                return 0;
            }
        }

        public async Task<int> ExecuteAddDbLogDetails(DbLoggerDetailsDataModel dbLoggerDetailsDataModel)
        {
            try
            {
                string query = string.Format(QueryReader.ReadQuery("AddDbLogDetails"),
                    ResolveQueryParam(dbLoggerDetailsDataModel.EAWFT_NUM),
                    ResolveQueryParam(dbLoggerDetailsDataModel.ECCPI_TAG_NAME),
                    ResolveQueryParam(dbLoggerDetailsDataModel.AREA_PI_TAG_NAME),
                    ResolveQueryParam(dbLoggerDetailsDataModel.SRC_PI_SERVER_CD),
                    ResolveQueryParam(dbLoggerDetailsDataModel.EASR_NUM),
                    ResolveQueryParam(dbLoggerDetailsDataModel.SVC_NAME),
                    ResolveQueryParam(SafeLogMessage(dbLoggerDetailsDataModel.SVC_MSG)),
                    ResolveQueryParam(dbLoggerDetailsDataModel.SVC_MSG_TYP),
                    ResolveQueryParam(dbLoggerDetailsDataModel.SVC_MSG_SEVIRITY),
                    ResolveQueryParam(dbLoggerDetailsDataModel.AREA_POINT_ID));
                await _repo.ExecuteScalarAsync(query, new { });
                await Commit();
                return 1;
            }
            catch (Exception e)
            {
                Logger.Error("DbLoggerStore.ExecuteAddDbLogDetails", e);
                return 0;
            }
        }
        private static string SafeLogMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return string.Empty;

            message = message.Replace('\t', ' ');
            message = message.Replace(Environment.NewLine, " --> ");
            message = message.Replace("\n", " --> ");
            message = message.Replace('\'', ' ');
            message = message.Replace('"', ' ');
            if (message.Length > 1000)
                message = message.Substring(0, 1000);
            return message;
        }
    }
}