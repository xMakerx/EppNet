///////////////////////////////////////////////////////
/// Filename: NetworkNodeBuilder.cs
/// Date: July 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Exceptions;
using EppNet.Services;
using EppNet.Sockets;

using System;
using System.Diagnostics.CodeAnalysis;

namespace EppNet.Node
{

    /// <summary>
    /// Handy builder pattern for constructing new <see cref="NetworkNode"/>s!<br/>
    /// You can create as many <see cref="NetworkNode"/>s as you would like, as long as you
    /// <br/>use unique <see cref="Service"/> and <see cref="BaseSocket"/> instances.
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

        /// <summary>
        /// Resets internal members associating with constructing a new <see cref="NetworkNode"/><br/>
        /// <paramref name="uniqueOnly"/> == true resets only members that cannot be the same on different nodes
        /// </summary>
        public NetworkNodeBuilder Reset(bool uniqueOnly = false)
        {
            // The following members must be unique for every network node
            this._name = string.Empty; // Names shouldn't be the same, but I don't have any guards in place for this
            this._socket = null;
            this._svcMgr = null;

            if (!uniqueOnly)
            {
                this._distro = Distribution.Shared;
                this._exceptStrat = ExceptionStrategy.ThrowAll;
            }

            return this;
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


        /// <summary>
        /// Tries to set the <see cref="BaseSocket"/> that will be associated with the newly constructed <see cref="NetworkNode"/>.<br/>
        /// Provided socket mustn't be associated with another NetworkNode.<br/>
        /// Setting this to null will have the newly constructed node pick a default socket. See: <see cref="NetworkNode.Socket"/>
        /// </summary>
        /// <param name="socket"></param>
        /// <exception cref="InvalidOperationException">When you try to associate a socket instance with multiple network nodes</exception>
        public NetworkNodeBuilder SetSocket(BaseSocket socket)
        {

            if (socket != null && socket.Node != null)
                throw new InvalidOperationException("Socket is already associated with a different NetworkNode!");

            _socket = socket;
            return this;
        }

        /// <summary>
        /// Creates a new <see cref="Service"/> of the specified type.<br/>
        /// <b>NOTE:</b> If a <see cref="ServiceManager"/> isn't associated with this builder, it will create one.
        /// </summary>
        /// <typeparam name="T">Must be a derived type of Service></typeparam>
        /// <param name="service">The created service</param>

        public NetworkNodeBuilder WithService<T>(out T service) where T : Service
        {
            if (_svcMgr == null)
                _svcMgr = new ServiceManager();

            _svcMgr.TryAddService(out service);
            return this;
        }

        /// <summary>
        /// Tries to add the specified <see cref="Service"/> to the internal <see cref="ServiceManager"/>.<br/>
        /// Provided service mustn't be associated with another node!
        /// </summary>
        /// <param name="service">The service to add</param>
        /// <exception cref="ArgumentNullException">When you try to add a null Service!</exception>
        /// <exception cref="InvalidOperationException">When you try to associate a service instance with multiple network nodes</exception>

        public NetworkNodeBuilder WithService([NotNull] Service service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            // Ensure we have the service manager set. Add the service
            _svcMgr = (_svcMgr == null) ? service._serviceMgr : _svcMgr;
            _svcMgr.TryAddService(service);

            return this;
        }

        /// <summary>
        /// Constructs a new <see cref="NetworkNode"/>!<br/>
        /// The newly constructed node isn't tracked in this instance. After creation, internally resets members that<br/>
        /// must be unique for each <see cref="NetworkNode"/> after creation
        /// </summary>

        public NetworkNode Build()
        {
            // Create the new NetworkNode
            NetworkNode node = new NetworkNode(_name, _distro, _exceptStrat, _socket, _svcMgr);

            if (_svcMgr != null)
                _svcMgr._node = node;

            if (_socket != null)
                _socket._Internal_SetupFor(node);

            // Reset members that should be unique for every node
            Reset(uniqueOnly: true);

            return node;
        }
        
    }

}
