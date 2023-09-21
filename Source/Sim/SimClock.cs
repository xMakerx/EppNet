///////////////////////////////////////////////////////
/// Filename: SimClock.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using EppNet.Core;

namespace EppNet.Sim
{
    
    public class SimClock
    {

        /// <summary>
        /// How many samples to collect to determine latency.
        /// <br/>Defaults to 10
        /// </summary>

        public int LatencySamples
        {

            set
            {
                if (_initialized)
                    throw new Exception("Clock already initialized!");

                _latency_samples = value;
            }

            get => _latency_samples;

        }

        public int ResyncIntervalMs;

        public float Latency { private set; get; }

        public ulong Time { protected set; get; }

        /// <summary>
        /// ENet also provides this, but this is used internally and is calculated
        /// based on ping datagram request and response times.
        /// </summary>
        public ulong RoundTripTime { protected set; get; }

        protected readonly ISimBase _sim_base;

        protected int _latency_samples = 10;

        protected Timestamp? _last_sync;
        protected Timestamp? _last_tick;
        protected Timestamp? _last_ping;

        protected float _decimal_collector;

        protected float _latency_delta;
        protected float[] _latency_sample_arr;
        protected int _latency_arr_index;

        protected bool _initialized;

        public SimClock(ISimBase simBase)
        {
            this._sim_base = simBase;

            this._last_sync = new Timestamp()
            {
                Type = TimestampType.Milliseconds,
                Monotonic = true,
                Value = 0
            };

            this._last_tick = new Timestamp()
            {
                Type = TimestampType.Milliseconds,
                Monotonic = true,
                Value = 0
            };

            this._last_ping = new Timestamp()
            {
                Type = TimestampType.Milliseconds,
                Monotonic = true,
                Value = 0
            };
        }

        public void Initialize()
        {
            // Ensure first initialization.
            if (_initialized)
                return;

            // Set timestamps to 0.
            _last_sync.Value.Set(0L);
            _last_tick.Value.Set(0L);
            _last_ping.Value.Set(0L);

            Latency = 0;
            Time = 0;
            RoundTripTime = 0;

            // Setup the latency samples array
            _latency_delta = 0f;
            _latency_sample_arr = new float[LatencySamples];
            _latency_arr_index = 0;

            _decimal_collector = 0f;
            _initialized = true;
        }

        public void Tick(float delta)
        {

            ulong a = (ulong)delta * 1000;
            ulong b = (ulong)Math.Floor(_latency_delta);
            Time += a + b;

            // Let's collect the decimals
            _decimal_collector += (_latency_delta - b);
            _decimal_collector += ((delta * 1000) - a);

            _latency_delta = 0;

            if (_decimal_collector >= 1f)
            {
                Time++;
                _decimal_collector--;
            }

            _last_tick.Value.Set(Simulation.MonotonicTime);
        }

        public bool ShouldSynchronize()
        {
            if (ResyncIntervalMs == 0)
                return false;

            // Client simulations usually want to resync from time-to-time.
            long elapsed_time = Simulation.MonotonicTime - _last_sync.Value.Get();

            return elapsed_time > 0 && elapsed_time > ResyncIntervalMs;
        }

        /// <summary>
        /// ET Phone Home!!<br/>
        /// Pings the remote host.
        /// </summary>

        public void Ping()
        {
            // E.T. phone home
            // TODO: Send ping datagram
            _last_ping.Value.Set(Simulation.MonotonicTime);
        }

        /// <summary>
        /// Processes an acknowledged ping.
        /// </summary>
        /// <param name="remote_time"></param>
        /// <param name="local_time"></param>

        public void ProcessPong(ulong remote_time, ulong local_time)
        {

            if (ShouldSynchronize())
            {
                // The time it took for us to ping and get a response.
                RoundTripTime = (ulong) (Simulation.MonotonicTime - _last_ping.Value.Get());

                // Update our time to be the provided remote time plus our round trip time.
                Time = remote_time + RoundTripTime;

                // Set the last time we synchronized
                _last_sync.Value.Set((ulong)Simulation.MonotonicTime);
            }

            float avg_delta = (float)(remote_time - local_time) / 2;
            _latency_sample_arr[_latency_arr_index] = avg_delta;

            if (++_latency_arr_index == LatencySamples)
            {
                _latency_arr_index = 0;

                // Sorts the array into ascending order for
                // midpoint calculation.
                Array.Sort(_latency_sample_arr);

                float total_latency = 0f;
                float midpoint = _latency_sample_arr[(LatencySamples / 2)];

                for (int i = (LatencySamples - 1); i > -1; i--)
                {
                    float latency = _latency_sample_arr[i];

                    if (latency > (2 * midpoint) && latency > 10)
                        // Ignore obvious outlier latencies.
                        // Maybe someone was streaming PornHub? Who knows.
                        continue;

                    // Add to the total latency
                    total_latency += latency;
                }

                float avg_latency = (total_latency / LatencySamples);

                // The change in latency compared.
                _latency_delta = avg_latency - Latency;
                Latency = avg_latency;
            }
        }


    }

}

