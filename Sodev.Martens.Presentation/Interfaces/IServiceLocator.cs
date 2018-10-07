using System;

namespace Sodev.Marten.Presentation.Interfaces
{
    public interface IServiceLocator
    {
        object GetInstance(Type type);
    }
}