///////////////////////////////////////////////////////
/// Filename: Simulation.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;
using EppNet.Sockets;

namespace EppNet.Sim
{

    public class Simulation : ISimBase
    {

        protected static Simulation _instance;
        public static Simulation Get() => _instance;

        public static ulong Time => (_instance != null ? _instance.Clock.Time : 0);

        public readonly Socket Socket;
        public readonly MessageDirector MessageDirector;
        public readonly SimClock Clock;

        public Simulation(Socket socket)
        {
            if (Simulation._instance != null)
                return;

            Simulation._instance = this;
            this.Socket = socket;
            this.MessageDirector = new MessageDirector();
            this.Clock = new SimClock(this);
        }

        public SimClock GetClock() => Clock;
        public Socket GetSocket() => Socket;
    }

}
