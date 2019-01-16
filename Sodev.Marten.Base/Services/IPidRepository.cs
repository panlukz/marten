using System.Collections.Generic;
using Sodev.Marten.Base.Model;

namespace Sodev.Marten.Base.Services
{
    public interface IPidRepository
    {
        IEnumerable<PidParameters> GetAllPidsParameters();
    }
}