using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
