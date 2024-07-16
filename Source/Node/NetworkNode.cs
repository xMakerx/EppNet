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

        /// <summary>
        /// Fetches the <see cref="ServiceManager"/> associated with this node
        /// </summary>
        public ServiceManager Services { get => _serviceMgr; }

        public Socket Socket { get => _socket; }

        internal ServiceManager _serviceMgr;
        internal Socket _socket;

        internal readonly int _index;

        private RuntimeFileMetadata _logMetadata;

        public NetworkNode(Distribution distro)
        {
            this.Name = string.Empty;
            this.UUID = Guid.NewGuid();
            this.Distro = distro;

            this._index = NetworkNodeManager._nodes.Count;
            this._serviceMgr = new(this);
            this._socket = null;

            // Let's try to register this
            NetworkNodeManager.TryRegisterNode(this);
        }

        public NetworkNode(Distribution distro, string name) : this(distro)
        {
            this.Name = name;
        }

        ~NetworkNode()
        {
            NetworkNodeManager.TryUnregisterNode(this);
        }

        public void HandleException(Exception exception,
            [CallerFilePath] string callerFilepath = null, 
            [CallerMemberName] string callerMemberName = null)
        {

            // No exception occurred. What is this rubbish!?
            if (exception == null)
                return;


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