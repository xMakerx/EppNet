using System;

namespace EppNet.Objects
{

    public interface IPoolable : IDisposable
    {

        public bool IsInitialized();

    }

}
