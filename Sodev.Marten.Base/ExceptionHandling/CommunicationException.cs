using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Base.ExceptionHandling
{
    public class CommunicationException : Exception
    {
        public CommunicationException() {}
        public CommunicationException(string message) : base(message) {}
    }
}
