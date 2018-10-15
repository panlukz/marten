using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sodev.Marten.Base.Connection;

namespace Sodev.Marten.Base.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly Connection.Connection connection = new Connection.Connection();

        public ConnectionService()
        {
        }

        public void Connect()
        {
            throw new NotImplementedException();
        }
    }

    public interface IConnectionService
    {
        void Connect();
    }
}
