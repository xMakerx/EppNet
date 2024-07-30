///////////////////////////////////////////////////////
/// Filename: ISimBase.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Time;
using EppNet.Sockets;

namespace EppNet.Sim
{

    public interface ISimBase
    {
        public Clock GetClock();
        public BaseSocket GetSocket();

    }

}
