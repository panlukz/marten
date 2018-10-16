using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Base.Services
{
    public class LiveDataService : ILiveDataService
    {
        private readonly IConnectionService connectionService;

        public LiveDataService(IConnectionService connectionService)
        {
            this.connectionService = connectionService;
        }
    }

    public interface ILiveDataService
    {

    }
}
