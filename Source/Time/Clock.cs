///////////////////////////////////////////////////////
/// Filename: Clock.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data.Datagrams;

namespace EppNet.Time
{

    public class Clock
    {

        public readonly ClockStrategy Strategy;

        public float Latency { internal set; get; }

        public float LatencyDelta { internal set; get; }

        public ulong Time { internal set; get; }

        /// <summary>
        /// ENet also provides this, but this is used internally and is calculated
        /// based on ping datagram request and response times.
        /// </summary>
        public ulong RoundTripTime { internal set; get; }

        protected internal bool _initialized;

        public Clock()
        {
            this.Strategy = new CommonClockStrategy(this, 5000L);
        }

        public void Initialize()
        {
            // Ensure first initialization.
            if (_initialized)
                return;

            Strategy.Initialize();
            _initialized = true;
        }

        public void Tick(float delta)
        {
            Strategy.Tick(delta);
        }

        /// <summary>
        /// ET Phone Home!!<br/>
        /// Pings the remote host.
        /// </summary>

        public void Ping()
        {
            Strategy.Ping();
        }

        public void ProcessPong(PingDatagram ping_dg)
        {
            Strategy.ProcessPong(ping_dg);
        }


    }

}

