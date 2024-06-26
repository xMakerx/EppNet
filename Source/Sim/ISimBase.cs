﻿///////////////////////////////////////////////////////
/// Filename: ISimBase.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Sockets;

namespace EppNet.Sim
{

    public interface ISimBase
    {
        public EppNet.Time.Clock GetClock();
        public Socket GetSocket();

    }

}
