///////////////////////////////////////////////////////
/// Filename: IRegistration.cs
/// Date: September 13, 2022
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Registers
{

    public interface IRegistration<T> : IRegistrationBase
    {
        public T Instance(params object[] args);
    }

    public interface IRegistrationBase : IDisposable
    {
        public bool Compile();
        public bool IsCompiled();

        public object NewGenericInstance(params object[] args);
    }

}
