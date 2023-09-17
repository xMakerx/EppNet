///////////////////////////////////////////////////////
/// Filename: IRegistration.cs
/// Date: September 13, 2022
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Registers
{

    public interface IRegistration : ICompilable, IDisposable
    {
        public object NewInstance(params object[] args);
    }

}
