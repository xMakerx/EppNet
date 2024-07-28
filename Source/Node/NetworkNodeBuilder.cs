///////////////////////////////////////////////////////
/// Filename: NetworkNodeBuilder.cs
/// Date: July 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Core;
using EppNet.Exceptions;
using EppNet.Services;
using EppNet.Sockets;

using System;
using System.Diagnostics.CodeAnalysis;

namespace EppNet.Node
{

    /// <summary>
    /// Handy builder pattern for constructing a new <see cref="NetworkNode"/>!<br/>
    /// <see cref="Build"/> will automatically register the generated <see cref="NetworkNode"/>,<br/>
    /// if a <see cref="NetworkNode"/> has already been generated, it will unregister the old one.
    /// </summary>
    public sealed class NetworkNodeBuilder
    {

        private string _name;
        private Distribution _distro;
        private ExceptionStrategy _exceptStrat;

        private BaseSocket _socket;
        private ServiceManager _svcMgr;
        public NetworkNodeBuilder() => Reset();

        public NetworkNodeBuilder(string name, Distribution distro) : this()
        {
            this._name = name;
            this._distro = distro;
        }

        public NetworkNodeBuilder(Distribution distro) : this(string.Empty, distro) { }

        public void Reset()
        {
            this._name = string.Empty;
            this._distro = Distribution.Shared;
            this._exceptStrat = ExceptionStrategy.ThrowAll;
            this._socket = null;
            this._svcMgr = null;
        }

        public NetworkNodeBuilder SetName(string name)
        {
            _name = name;   
            return this;
        }

        public NetworkNodeBuilder SetDistribution(Distribution distro)
        {
            _distro = distro;
            return this;
        }

        public NetworkNodeBuilder SetExceptionStrategy(ExceptionStrategy exceptStrat)
        {
            _exceptStrat = exceptStrat;
            return this;
        }

        public NetworkNodeBuilder SetSocket(BaseSocket socket)
        {
            _socket = socket;

            if (_socket.Node != null)
                throw new InvalidOperationException("Socket is already associated with a different NetworkNode!");

            return this;
        }

        public NetworkNodeBuilder WithService<T>(out T service) where T : Service
        {
            if (_svcMgr == null)
                _svcMgr = new ServiceManager();

            _svcMgr.TryAddService(out service);
            return this;
        }

        public NetworkNodeBuilder WithService([NotNull] Service service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            if (service.Node != null)
                throw new InvalidOperationException("Service is already associated with a different NetworkNode!");

            // Ensure we have the service manager set. Add the service
            _svcMgr = (_svcMgr == null) ? service._serviceMgr : _svcMgr;
            _svcMgr.TryAddService(service);

            return this;
        }

        /// <summary>
        /// Constructs a new <see cref="NetworkNode"/>!
        /// </summary>
        /// <returns></returns>

        public NetworkNode Build()
        {
            // Create the new NetworkNode
            NetworkNode node = new NetworkNode(_name, _distro, _exceptStrat, _socket, _svcMgr);

            if (_svcMgr != null)
                _svcMgr._node = node;

            if (_socket != null)
                _socket._Internal_SetupFor(node);

            // We cannot create new nodes with the same socket or service manager
            _socket = null;
            _svcMgr = null;

            return node;
        }
        
    }

}
