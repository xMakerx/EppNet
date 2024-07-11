﻿///////////////////////////////////////////////////////
/// Filename: NetworkNode.cs
/// Date: July 9, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
using System;
using System.Runtime.CompilerServices;

using EppNet.Services;
using EppNet.Logging;

namespace EppNet.Node
{

    /// <summary>
    /// NetworkNodes contain everything necessary for an instance of E++Net to function.
    /// 
    /// </summary>

    public class NetworkNode : ILoggable, IEquatable<NetworkNode>
    {

        public string Name;
        public readonly Guid UUID;

        /// <summary>
        /// Fetches the <see cref="ServiceManager"/> associated with this node
        /// </summary>
        public ServiceManager Services
        {
            get => _serviceMgr;
        }

        internal ServiceManager _serviceMgr;

        internal readonly int _index;

        private RuntimeFileMetadata _logMetadata;

        public NetworkNode()
        {
            this._Internal_SetupLogging();
            this.Name = string.Empty;
            this.UUID = Guid.NewGuid();

            this._index = NetworkNodeManager._nodes.Count;
            this._serviceMgr = new(this);

            // Let's try to register this
            NetworkNodeManager.TryRegisterNode(this);
        }

        public NetworkNode(string name) : this()
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