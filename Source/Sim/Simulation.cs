///////////////////////////////////////////////////////
/// Filename: Simulation.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Attributes;
using EppNet.Core;
using EppNet.Data;
using EppNet.Registers;
using EppNet.Sockets;
using EppNet.Utilities;

using Serilog;

using System;

namespace EppNet.Sim
{

    public class Simulation : ISimBase
    {

        protected static Simulation _instance = null;
        public static Simulation Get() => _instance;

        /// <summary>
        /// Fetches the monotonic time as a <see cref="Timestamp"/> or returns 0 if ENet hasn't been initialized.
        /// Monotonic time is the time since this device was powered on and is maintained on the kernel
        /// </summary>

        public static Timestamp MonotonicTimestamp
        {
            get
            {
                if (_instance != null && _instance._initialized)
                    _monotonicTimestamp.Set(MonotonicTime);

                return _monotonicTimestamp;
            }
        }

        private static Timestamp _monotonicTimestamp = new Timestamp(TimestampType.Milliseconds, true, 0L);

        /// <summary>
        /// Shorthand for <see cref="MonotonicTimestamp"/>
        /// </summary>
        public static Timestamp MonotoTs => MonotonicTimestamp;

        /// <summary>
        /// Shorthand for <see cref="MonotonicTime"/>
        /// </summary>
        public static uint MonoTime => MonotonicTime;

        /// <summary>
        /// Fetches the monotonic time or returns 0 if ENet hasn't been initialized.
        /// Monotonic time is the time since this device was powered on and is maintained on the kernel
        /// </summary>

        public static uint MonotonicTime
        {
            get
            {
                uint t = 0;

                if (_instance != null && _instance._initialized)
                    t = ENet.Library.Time;

                return t;
            }
        }

        public static ulong Time => (_instance != null ? _instance.Clock.Time : 0);

        public readonly Socket Socket;
        public readonly MessageDirector MessageDirector;
        public readonly SimClock Clock;

        protected bool _initialized;

        public Simulation(Socket socket)
        {
            if (Simulation._instance != null)
                return;

            Serilog.Debugging.SelfLog.Enable(Console.Error);
            Log.Logger = new LoggerConfiguration().WriteTo.Console().MinimumLevel.Debug().CreateLogger();

            Simulation._instance = this;
            this.Socket = socket;
            this.MessageDirector = new MessageDirector();
            this.Clock = new SimClock(this);
            this._initialized = false;
        }

        public void Initialize(Callbacks enet_callbacks = null)
        {
            if (LoggingExtensions.AssertFalseOrLog(_initialized, 
                Serilog.Events.LogEventLevel.Warning, 
                "[Simulation#Initialize()] Called initialize twice?"))
                return;

            AttributeFetcher.AddType<NetworkObjectAttribute>(type =>
            {
                bool isValid = type.IsClass && typeof(ISimUnit).IsAssignableFrom(type);

                if (!isValid)
                    Log.Error($"[{type.Name}] Invalid use of NetworkObjectAttribute. Provided type does not extend ISimUnit!!");

                return isValid;
            });

            if (enet_callbacks != null)
            {
                Log.Information("[Simulation#Initialize()] Initializing with ENet callbacks");
                _initialized = ENet.Library.Initialize(enet_callbacks);
            }
            else
            {
                Log.Information("[Simulation#Initialize()] Initializing without ENet callbacks");
                _initialized = ENet.Library.Initialize();
            }

            if (LoggingExtensions.AssertTrueOrFatal(_initialized, "[Simulation#Initialize()] Failed to initialize the ENet Library!"))
            {
                // We were able to initialize ENet successfully!
                Log.Information("[Simulation#Initialize()] Compiling Datagram expression trees...");
                DatagramRegister.Get().Compile();

                Log.Information("[Simulation#Initialize()] Compiling custom object expression trees...");
                ObjectRegister.Get().Compile();
            }
        }

        public static void Main(string[] args)
        {
            var sim = new Simulation(null);
            sim.Initialize();
        }

        public SimClock GetClock() => Clock;
        public Socket GetSocket() => Socket;
    }

}
