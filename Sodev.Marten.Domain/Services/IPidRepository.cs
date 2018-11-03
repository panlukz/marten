using System.Collections;
using System.Collections.Generic;
using Sodev.Marten.Base.Model;

namespace Sodev.Marten.Domain.Services
{
    public interface IPidRepository
    {
        IEnumerable<PidParameters> GetAllPidsParameters();
    }
}