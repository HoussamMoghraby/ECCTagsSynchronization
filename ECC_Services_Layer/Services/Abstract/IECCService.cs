using System.Threading.Tasks;

namespace ECC_AFServices_Layer.Services.Abstract
{
    public interface IECCService
    {
        Task<bool> StartAsync();
    }

    public interface IWService
    {
        void InitializeSchedule();
    }
}
