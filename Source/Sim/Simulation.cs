///////////////////////////////////////////////////////
/// Filename: Simulation.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Sockets;

using System;

namespace EppNet.Sim
{

    public class Simulation : ISimBase
    {

        protected static Simulation _instance;
        public static Simulation Get() => _instance;

        public static ulong Time => (_instance != null ? _instance.Clock.Time : 0);

        public readonly Socket Socket;
        public readonly SimClock Clock;

        public Simulation(Socket socket)
        {
            if (Simulation._instance != null)
                return;

            Simulation._instance = this;
            this.Socket = socket;
            this.Clock = new SimClock(this);
        }

        public SimClock GetClock() => Clock;
        public Socket GetSocket() => Socket;
    }

}
