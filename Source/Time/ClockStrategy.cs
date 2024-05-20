///////////////////////////////////////////////////////
/// Filename: ClockStrategy.cs
/// Date: May 19, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
using EppNet.Data.Datagrams;
using EppNet.Sim;

using System;

namespace EppNet.Time
{

    public abstract class ClockStrategy
    {

        /// <summary>
        /// How many latency samples
        /// </summary>
        public const int DEFAULT_LATENCY_SAMPLES = 10;

        protected readonly Clock Clock;
        public Timestamp LastSync;
        public Timestamp LastTick;
        public Timestamp LastPing;

        /// <summary>
        /// How often (in milliseconds) we should attempt to
        /// synchronize with the remote host.
        /// </summary>

        public ulong SynchronizeIntervalMs
        {

            private set
            {
                _sync_ival_ms = value;
            }

            get => _sync_ival_ms;

        }

        /// <summary>
        /// How many latency samples we take at a time
        /// </summary>
        public int LatencySamples
        {
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("Latency Samples must be greater than 0!");

                _num_latency_samples = value;
            }

            get => _num_latency_samples;
        }

        protected ulong _sync_ival_ms;

        protected float[] _latency_samples;
        protected int _latency_samples_idx;
        protected int _num_latency_samples;

        protected float _decimal_collector;

        protected ClockStrategy(Clock clock, ulong sync_interval_ms)
        {
            this.Clock = clock;
            this.SynchronizeIntervalMs = sync_interval_ms;
            this.LatencySamples = DEFAULT_LATENCY_SAMPLES;
        }

        public void Initialize()
        {
            if (Clock._initialized)
                return;

            this.LastSync = new(TimestampType.Milliseconds, true, 0L);
            this.LastTick = new(TimestampType.Milliseconds, true, 0L);
            this.LastPing = new(TimestampType.Milliseconds, true, 0L);

            this._latency_samples = new float[LatencySamples];
            this._latency_samples_idx = 0;

            this._decimal_collector = 0f;
        }

        public abstract void Tick(float delta);


        /// <summary>
        /// Should we try to synchronize with the remote host?<br/><br/>
        /// 
        /// Conditions to synchronize:<br/>
        /// -> <see cref="LastSync"/> == 0; or<br/>
        /// -> <see cref="SynchronizeIntervalMs"/> > 0; and<br/>
        /// -> Elapsed time since last sync is greater than or equal to <see cref="SynchronizeIntervalMs"/>
        /// </summary>
        /// <returns>True or false</returns>
        public virtual bool ShouldSynchronize()
        {
            // Have we synchronized before? If we haven't,
            // the value of last sync will be 0.
            if (LastSync.Get() == 0)
                return true;

            // If the sync interval is 0, we don't automatically
            // resynchronize
            if (SynchronizeIntervalMs == 0)
                return false;

            ulong elapsed_time = Simulation.MonotonicTime - LastSync;

            // Has enough time passed since our last synchronization?
            return elapsed_time >= SynchronizeIntervalMs;
        }

        public virtual void CalculateLatency(out float latency, out float latency_dt)
        {
            // Reset our index to 0
            _latency_samples_idx = 0;

            // Sort the array into ascending order to
            // calculate midpoint
            Array.Sort(_latency_samples);

            float total_latency = 0f;
            float midpoint = _latency_samples[LatencySamples / 2];

            for (int i = LatencySamples - 1; i > -1; i--)
            {
                float lat = _latency_samples[i];

                if (lat > (2 * midpoint) && lat > 10)
                    // Ignore obvious outlier latencies
                    continue;

                // Add to the total
                total_latency += lat;
            }

            float avg_latency = total_latency / LatencySamples;
            latency_dt = avg_latency - Clock.Latency;
            latency = avg_latency;
        }

        public virtual void Ping()
        {
            LastPing.SetToMonoNow();
            //PingDatagram datagram = new();

            // TODO: Send a ping datagram
        }

        public abstract void ProcessPong(PingDatagram ping_dg);

    }

}