///////////////////////////////////////////////////////
/// Filename: Simulation.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Attributes;
using EppNet.Logging;
using EppNet.Registers;
using EppNet.Sockets;
using EppNet.Time;
using EppNet.Utilities;

using Serilog;

using System;
using System.Threading;

using ENetLib = ENet.Library;

namespace EppNet.Sim
{

    public class Simulation : ISimBase, ILoggable
    {

        protected static Simulation _instance = null;
        public static Simulation Get() => _instance;

        public static bool StopRequested
        {
            set
            {
                // Must have an instance
                if (_instance == null)
                    return;

                if (value != _instance._stopRequested)
                    _instance._stopRequested = value;
            }

            get => (_instance != null) ? _instance._stopRequested : false;
        }

        public static bool Running
        {
            get => (_instance != null) ? _instance._running : false;
        }

        public readonly BaseSocket Socket;
        public readonly Clock Clock;

        public ILoggable Notify { get => this; }

        public Distribution DistroType { internal set; get; }

        protected bool _initialized;

        protected Timer _tickerTimer;
        protected bool _stopRequested;
        protected bool _running;

        public Simulation(BaseSocket socket)
        {
            if (Simulation._instance != null)
                return;

            Serilog.Debugging.SelfLog.Enable(Console.Error);
            Log.Logger = new LoggerConfiguration().WriteTo.Console().MinimumLevel.Debug().CreateLogger();

            Simulation._instance = this;
            this._initialized = false;
            this._tickerTimer = null;
            this._stopRequested = false;
            this._running = false;

            this.Socket = socket;
            //this.Clock = new();
        }

        public void Initialize(Callbacks enet_callbacks = null)
        {
            if (Notify.AssertTrueOrLog(Serilog.Events.LogEventLevel.Warning, "Called initialize twice?", !_initialized))
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
                Notify.Info("Initializing with ENet callbacks...");
                _initialized = ENetLib.Initialize(enet_callbacks);
            }
            else
            {
                Notify.Info("Initializing without ENet callbacks...");
                _initialized = ENetLib.Initialize();
            }

            if (Notify.AssertTrueOrFatal("Failed to initialize the ENet Library!", _initialized))
            {
                Notify.Info($"Initialized ENet ver-{ENetLib.version}!");

                // We were able to initialize ENet successfully!
                Notify.Info("Compiling Datagram expression trees...");
                DatagramRegister.Get().Compile();

                Notify.Info("Compiling Object expression trees...");
                ObjectRegister.Get().Compile();
            }
        }

        public void Start()
        {
            if (!_initialized)
            {
                Notify.Warn("Tried to start without first calling Simulation#Initialize()!");
                return;
            }

            if (_running)
            {
                Notify.Warn("The simulation is already running!");
                return;
            }

            _stopRequested = false;
            
            // FIXME: Setup processing order and start disruptor


            int period = SimSettings.CalculateMsBetweenUpdates();
            _tickerTimer = new Timer((state) => DoTick(state), this, period, period);
            SimSettings.UpdateRateChanged += _UpdateTickTimer;

            _running = true;
            Notify.Info("Started simulating...");

            int i = 0;
            while (_running)
            {
                i++;
            }
        }

        /// <summary>
        /// Requests the Simulation to stop processing updates
        /// </summary>

        public void Stop()
        {
            StopRequested = true;
            SimSettings.UpdateRateChanged -= _UpdateTickTimer;
        }

        private void _UpdateTickTimer()
        {
            int period = (int) SimSettings.CalculateMsBetweenUpdates();
            _tickerTimer.Change(period, period);
        }

        public void DoTick(object state)
        {
            //Simulation sim = state as Simulation;

            if (StopRequested)
            {
                // Let's stop running
                _tickerTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _running = false;

                // FIXME: Clear queues here?
                return;
            }

        }

        public static void Main(string[] args)
        {
            var sim = new Simulation(null);
            sim.Initialize();
            sim.Start();
        }

        public Clock GetClock() => Clock;
        public BaseSocket GetSocket() => Socket;
    }

}
