///////////////////////////////////////////////////////
/// Filename: NetworkNode.cs
/// Date: July 9, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
using System;
using System.Runtime.CompilerServices;

using EppNet.Services;
using EppNet.Sockets;
using EppNet.Logging;
using EppNet.Core;
using EppNet.Exceptions;
using Serilog;

namespace EppNet.Node
{

    /// <summary>
    /// NetworkNodes contain everything necessary for an instance of E++Net to function.
    /// 
    /// </summary>

    public class NetworkNode : ILoggable, IEquatable<NetworkNode>
    {
        public ILoggable Notify { get => this; }

        public string Name;
        public readonly Guid UUID;
        public readonly Distribution Distro;

        public ExceptionStrategy ExceptionStrategy;

        /// <summary>
        /// Fetches the <see cref="ServiceManager"/> associated with this node
        /// </summary>
        public ServiceManager Services { get => _serviceMgr; }

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

        internal ServiceManager _serviceMgr;
        internal BaseSocket _socket;

        internal readonly int _index;

        private RuntimeFileMetadata _logMetadata;

        public NetworkNode(Distribution distro)
        {
            this.Name = string.Empty;
            this.UUID = Guid.NewGuid();
            this.Distro = distro;
            this.ExceptionStrategy = ExceptionStrategy.ThrowAll;

            this._index = NetworkNodeManager._nodes.Count;
            this._serviceMgr = new(this);
            this._socket = null;
            this._logMetadata = null;

            // Let's try to register this
            NetworkNodeManager.TryRegisterNode(this);

            Serilog.Debugging.SelfLog.Enable(Console.Error);
            Log.Logger = new LoggerConfiguration().WriteTo.Console().MinimumLevel.Debug().CreateLogger();
            Notify.SetLogLevel(LogLevelFlags.Debug);
        }

        public NetworkNode(Distribution distro, string name) : this(distro)
        {
            this.Name = name;
        }

        ~NetworkNode()
        {
            NetworkNodeManager.TryUnregisterNode(this);
        }

        /// <summary>
        /// Tries to start this NetworkNode:
        /// <br/>- Opens the socket (creates if necessary)
        /// <br/>- Starts the ServiceManager
        /// </summary>
        /// <returns></returns>

        public bool TryStart()
        {
            // Try to start our NetworkNode 
            if (Socket.Create())
            {
                // Start our services
                Services.Start();
                return true;
            }

            return false;
        }

        internal void _Internal_SetSocket(BaseSocket socket)
        {
            this._socket = socket;
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
    }

}