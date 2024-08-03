///////////////////////////////////////////////////////
/// Filename: NetworkNode.cs
/// Date: July 9, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
using EppNet.Attributes;
using EppNet.Core;
using EppNet.Data;
using EppNet.Exceptions;
using EppNet.Logging;
using EppNet.Registers;
using EppNet.Services;
using EppNet.Sim;
using EppNet.Sockets;
using EppNet.Time;
using EppNet.Utilities;

using Serilog;
using System;
using System.Runtime.CompilerServices;

namespace EppNet.Node
{

    /// <summary>
    /// NetworkNodes contain everything necessary for an instance of E++Net to function.
    /// 
    /// </summary>

    public class NetworkNode : ILoggable, IEquatable<NetworkNode>, IDataHolder, INameable
    {

        public ILoggable Notify { get => this; }

        public string Name { set; get; }
        public readonly Guid UUID;
        public readonly Distribution Distro;

        public ExceptionStrategy ExceptionStrategy;

        /// <summary>
        /// Fetches the <see cref="ServiceManager"/> associated with this node
        /// </summary>
        public ServiceManager Services { get => _serviceMgr; }

        /// <summary>
        /// The <see cref="BaseSocket"/> associated with this Node.<br/>
        /// If you try to get the Socket without creating one first, it will<br/>
        /// generate a default node based on the <see cref="Distribution"/> type.<br/>
        /// - <see cref="Distribution.Client"/> => <see cref="ClientSocket"/><br/>
        /// - <see cref="Distribution.Server"/> => <see cref="ServerSocket"/>
        /// </summary>
        public BaseSocket Socket
        {
            get
            {
                if (_socket == null)
                {
                    switch (Distro)
                    {
                        case Distribution.Client:
                            new ClientSocket(this);
                            break;

                        case Distribution.Server:
                            new ServerSocket(this);
                            break;
                    }

                }

                return _socket;
            }

        }

        public Timestamp Time
        {

            get
            {
                if (Socket == null)
                    return Timestamp.FromMonoNow();

                return new Timestamp(TimestampType.Milliseconds, false, (long) Socket.Clock.Time);
            }

        }

        internal ServiceManager _serviceMgr;
        internal BaseSocket _socket;

        internal readonly int _index;

        private RuntimeFileMetadata _logMetadata;
        private bool _started;

        internal NetworkNode(string name, Distribution distro, ExceptionStrategy exceptStrat, BaseSocket socket, ServiceManager serviceManager)
        {

            if (distro == Distribution.Shared)
                throw new InvalidOperationException("Invalid Distribution type! Must be either Server or Client!");

            this.Name = name;
            this.UUID = Guid.NewGuid();
            this.Distro = distro;
            this.ExceptionStrategy = exceptStrat;

            this._serviceMgr = serviceManager ?? new(this);
            this._socket = socket;
            this._logMetadata = null;
            this._started = false;

            Serilog.Debugging.SelfLog.Enable(Console.Error);
            Log.Logger = new LoggerConfiguration().WriteTo.Console().MinimumLevel.Verbose().CreateLogger();
            Notify.SetLogLevel(LogLevelFlags.InfoWarnErrorFatal);

            // Let's try to register this
            if (!NetworkNodeManager._Internal_TryRegisterNode(this, out _index))
                throw new InvalidOperationException("Node has already been added!");
        }

        public NetworkNode(Distribution distro) : this(string.Empty, distro, ExceptionStrategy.ThrowAll, null, null) { }

        public NetworkNode(string name, Distribution distro) : this(name, distro, ExceptionStrategy.ThrowAll, null, null) { }

        ~NetworkNode() => Dispose(false);

        public void Dispose(bool disposing)
        {
            NetworkNodeManager._Internal_TryUnregisterNode(this);
            TryStop(!disposing);

            // Ensure our custom data is cleaned up
            IDataHolder.DeleteAllData(this);
        }

        public void Dispose() => Dispose(true);

        /// <summary>
        /// Tries to start this NetworkNode:
        /// <br/>- Opens the socket (creates if necessary; implied by <see cref="Socket"/>#get)
        /// <br/>- Starts the ServiceManager
        /// </summary>
        /// <returns>Whether or not we started</returns>

        public bool TryStart()
        {

            // Let's ensure our expression trees are ready.
            DatagramRegister dgRegister = DatagramRegister.Get();
            ObjectRegister objRegister = ObjectRegister.Get();
            Timestamp start = Timestamp.FromMonoNow();

            if (!dgRegister.IsCompiled())
            {
                Notify.Verbose("Compiling Datagram expression trees... Please wait.");

                CompilationResult result = dgRegister.Compile();

                if (!result.Successful)
                {
                    Notify.Fatal("Compilation of Datagram expression trees failed!");
                    throw result.Error;
                }

                Notify.Info(new TemplatedMessage("Successfully compiled Datagram expression trees in {time} ms", (Timestamp.FromMonoNow() - start).Value));
            }

            start = Timestamp.FromMonoNow();
            if (!objRegister.IsCompiled())
            {

                Notify.Verbose("Looking up objects w/NetworkObjectAttribute...");
                AttributeFetcher.AddType<NetworkObjectAttribute>(type =>
                {
                    bool isValid = type.IsClass && typeof(ISimUnit).IsAssignableFrom(type);

                    if (!isValid)
                        Notify.Error($"[{type.Name}] Invalid use of NetworkObjectAttribute. Provided type does not extend ISimUnit!!");

                    return isValid;
                });

                Notify.Verbose("Compiling Object expression trees... Please wait.");

                CompilationResult result = objRegister.Compile();

                if (!result.Successful)
                {
                    Notify.Fatal("Compilation of Object expression trees failed!");
                    throw result.Error;
                }

                Notify.Info(new TemplatedMessage("Successfully compiled Object expression trees in {time} ms", (Timestamp.FromMonoNow() - start).Value));
            }

            // Try to create our socket
            if (Socket.Create())
            {

                try
                {
                    // Start our services
                    Services.Start();
                    _started = true;

                    Console.Beep();
                    Notify.Debug("Started!");
                    return true;
                }
                catch (Exception ex)
                {
                    Notify.Error("Failed to start!");
                    HandleException(ex);
                }
            }

            return false;
        }

        public bool TryStop() => TryStop(false);

        /// <summary>
        /// Tries to stop this NetworkNode
        /// </summary>
        /// <param name="finalizer">Whether or not a finalizer ran this</param>
        /// <returns></returns>

        public bool TryStop(bool finalizer)
        {
            if (!_started)
                return false;

            if (finalizer)
            {
                // Let's discourage this behavior! Bad, bad!
                const string warning = "It's bad practice to not manually stop a NetworkNode. Ensure you call NetworkNode#TryStop() when closing your application.";
                Notify.Warn(warning);
            }

            Console.Beep();
            Socket.Dispose(!finalizer);
            Services.Stop();
            _started = false;

            Notify.Debug("Stopped!");
            
            return true;
        }

        public void HandleException(Exception exception,
            [CallerFilePath] string callerFilepath = null, 
            [CallerMemberName] string callerMemberName = null)
        {

            // No exception occurred. What is this rubbish!?
            if (exception == null)
                return;

            // TODO: NetworkNode: Perhaps behave differently depending on the severity of the exception?

            switch (ExceptionStrategy)
            {

                case ExceptionStrategy.ThrowAll:
                    // Stop all our services
                    // This will politely disconnect everyone as well.
                    _serviceMgr.Stop();

                    // Scream!
                    throw exception;

                case ExceptionStrategy.LogOnly:
                    RuntimeFileMetadata.GetMetadataFromPath(callerFilepath, out RuntimeFileMetadata metadata, cacheIfNecessary: false);
                    TemplatedMessage msgData = new("[{filename}#{memberName}()] A {exception} occurred with message {message}", metadata.Filename, 
                        ILoggableExtensions.ResolveMemberName(callerMemberName), exception.GetType().Name, exception.Message);
                    Notify.Error(msgData, exception);
                    break;

            }
        }

        public bool Equals(NetworkNode other)
        {
            if (other == null)
                return false;

            return other.UUID.Equals(this.UUID);
        }

        public override bool Equals(object obj)
            => obj is NetworkNode other && Equals(other);

        public override int GetHashCode()
        {
            int hashCode = _index + UUID.GetHashCode();
            hashCode ^= (_index + 1);
            return hashCode;
        }

        void ILoggable._Internal_SetMetadata(RuntimeFileMetadata metadata)
        {
            _logMetadata = metadata;
        }

        RuntimeFileMetadata ILoggable._Internal_GetMetadata() => _logMetadata;

        internal void _Internal_SetSocket(BaseSocket socket)
        {
            this._socket = socket;
        }
    }

}