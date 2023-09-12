///////////////////////////////////////////////////////
/// Filename: ISimBase.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Sockets;

namespace EppNet.Sim
{

    public interface ISimBase
    {
        public SimClock GetClock();
        public Socket GetSocket();

    }

}
