///////////////////////////////////////////////////////
/// Filename: CommonClockStrategy.cs
/// Date: May 19, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
using EppNet.Core;
using EppNet.Data.Datagrams;
using EppNet.Sim;

using System;

namespace EppNet.Time
{

    public class CommonClockStrategy : ClockStrategy
    {

        public CommonClockStrategy(Clock clock, ulong sync_interval_ms) : base(clock, sync_interval_ms) { }

        public override void ProcessPong(PingDatagram ping_dg)
        {

            ulong sent_time = ping_dg.SentTime;
            ulong recv_time = ping_dg.ReceivedTime;

            if (ShouldSynchronize())
            {
                // The time it took for us to ping and get a response
                Clock.RoundTripTime = (ulong)(Simulation.MonotonicTime - LastPing.Get());

                // Update our time to be the provided remote time plus our round trip time.
                Clock.Time = recv_time + Clock.RoundTripTime;

                // Set the last sync time
                LastSync.SetToMonoNow();
            }

            // Set the trip time
            _latency_samples[_latency_samples_idx++] = (recv_time - sent_time) / 2f;

            if (_latency_samples_idx == LatencySamples)
            {
                CalculateLatency(out float latency, out float latency_dt);
                Clock.Latency = latency;
                Clock.LatencyDelta = latency_dt;
            }

        }

        public override void Tick(float delta)
        {
            ulong a = (ulong)delta * 1000;
            ulong b = (ulong)Math.Floor(Clock.LatencyDelta);
            Clock.Time += a + b;

            // Let's collect the decimals
            _decimal_collector += (Clock.LatencyDelta - b);
            _decimal_collector += ((delta * 1000) - a);

            Clock.LatencyDelta = 0;

            if (_decimal_collector >= 1f)
            {
                Clock.Time++;
                _decimal_collector--;
            }

            LastTick.SetToMonoNow();
        }
    }

}