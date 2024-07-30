///////////////////////////////////////////////////////
/// Filename: Clock.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data.Datagrams;
using EppNet.Services;

using System.Diagnostics.CodeAnalysis;

namespace EppNet.Time
{

    public class Clock : IRunnable
    {

        public ClockStrategy Strategy { get => _strat; }

        public float Latency { internal set; get; }

        public float LatencyDelta { internal set; get; }

        public ulong Time
        {
            internal set
            {
                _time = value;
            }

            get => _time;
        }

        /// <summary>
        /// ENet also provides this, but this is used internally and is calculated
        /// based on ping datagram request and response times.
        /// </summary>
        public ulong RoundTripTime { internal set; get; }

        protected internal bool _started;
        protected internal ClockStrategy _strat;
        protected internal ulong _time;

        internal Clock()
        {
            this._strat = new CommonClockStrategy(this, 5000L);
        }

        internal Clock([NotNull] ClockStrategy strat)
        {
            this._strat = strat;
        }

        public bool Start()
        {
            if (_started)
                return false;

            _Internal_Reset();
            _started = true;
            return true;
        }

        public bool Stop()
        {
            if (!_started)
                return false;

            _started = false;
            _Internal_Reset();
            return true;
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

        private void _Internal_Reset()
        {
            this.Latency = -1f;
            this.LatencyDelta = 1f;
            this.Time = this.RoundTripTime = 0L;
            _strat.Reset();
        }

        internal bool _Internal_Tick(float delta)
        {
            if (!_started)
            {
                // Cannot tick the clock when it hasn't been started yet!
                return false;
            }

            Strategy.Tick(delta);
            return true;
        }

    }

}

