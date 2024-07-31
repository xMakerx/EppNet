﻿///////////////////////////////////////////////////////
/// Filename: BaseSocket.cs
/// Date: July 27, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Connections;
using EppNet.Logging;
using EppNet.Node;
using EppNet.Time;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Diagnostics.CodeAnalysis;

namespace EppNet.Sockets
{

    public enum SocketType
    {
        Unknown     = 0,
        Client      = 1,
        Server      = 2
    }

    public abstract class BaseSocket : ILoggable, IDisposable
    {

        public ILoggable Notify => this;

        public NetworkNode Node { get => _node; }
        public Clock Clock { get => _clock; }
        public ConnectionManager ConnectionManager { get => _connMgr; }

        public SocketType Type { get => _type; }

        /// <summary>
        /// The IP address remote clients connect to
        /// </summary>
        public string IP
        {

            set
            {

                if (string.IsNullOrEmpty(value) || value.Equals(_ip))
                    return;

                if (_enet_addr.SetIP(value))
                    _ip = value;

            }

            get => _enet_addr.GetIP();
        }

        /// <summary>
        /// The port for communications
        /// </summary>

        public ushort Port
        {
            set
            {
                if (_port != value)
                {
                    _port = value;
                    _enet_addr.Port = value;
                }
            }

            get => _enet_addr.Port;
        }

        /// <summary>
        /// The remote host a client is connecting to
        /// </summary>

        public string HostName
        {

            set
            {
                if (string.IsNullOrEmpty(value) || _hostName.Equals(value))
                    return;

                if (_enet_addr.SetHost(value))
                    _hostName = value;
            }

            get => _enet_addr.GetHost();

        }

        public int MaxClients
        {
            set
            {

                if (value < 0 || value > 4095)
                {
                    Node.HandleException(new ArgumentOutOfRangeException("MaxClients must be between 0 and 4095!"));
                    return;
                }

                _maxClients = value;
            }

            get => _maxClients;
        }

        public Timestamp CreateTimeMono;
        public Timestamp LastPollMono;

        internal NetworkNode _node;
        protected ConnectionManager _connMgr;

        protected Host _enet_host;
        protected Peer? _enet_peer;
        protected Address _enet_addr;
        protected Event _enet_event;

        protected string _ip;
        protected ushort _port;
        protected string _hostName;
        private int _maxClients;

        protected readonly SocketType _type;
        internal Clock _clock;

        protected BaseSocket(SocketType type)
        {
            this.LastPollMono = new Timestamp(TimestampType.Milliseconds, true, 0L);
            this.CreateTimeMono = new Timestamp(TimestampType.Milliseconds, true, 0L);

            // ENet library inits
            this._enet_host = null;
            this._enet_peer = null;
            this._enet_addr = new();

            // Backing fields for props
            this._node = null;
            this._connMgr = null;
            this._type = type;
            this._clock = new();
            this._ip = _hostName = string.Empty;
            this._port = 0;
        }

        protected BaseSocket(NetworkNode node, SocketType type) : this(type) => _Internal_SetupFor(node);

        ~BaseSocket() => Dispose(false);

        

        public virtual void OnPeerConnected(Peer peer)
        {
            ConnectionManager.HandleConnectionEstablished(peer);
        }

        public virtual void OnPeerDisconnected(Peer peer, uint disconnectReasonIdx)
        {
            ConnectionManager.HandleConnectionLost(peer, DisconnectReason.GetFromID(disconnectReasonIdx));
        }

        public abstract void OnPacketReceived(Peer peer, Packet packet);

        public virtual void Poll(int timeoutMs = 0)
        {
            if (!IsOpen())
            {
                Node.HandleException(new InvalidOperationException("Socket has not been opened!"));
                return;
            }

            bool polled = false;

            while (!polled)
            {
                if (_enet_host.CheckEvents(out _enet_event) <= 0)
                {
                    if (_enet_host.Service(timeoutMs, out _enet_event) <= 0)
                        break;

                    polled = true;
                    LastPollMono.Value = Library.Time;
                }

                switch (_enet_event.Type)
                {

                    case EventType.None:
                        break;

                    case EventType.Connect:
                        // A new peer has connected!
                        OnPeerConnected(_enet_event.Peer);
                        break;

                    case EventType.Disconnect:
                        // A peer has disconnected!
                        OnPeerDisconnected(_enet_event.Peer, _enet_event.Data);
                        break;

                    case EventType.Receive:
                        using (_enet_event.Packet)
                            OnPacketReceived(_enet_event.Peer, _enet_event.Packet);
                        break;

                }
            }
        }

        public virtual bool Create()
        {
            if (_node == null || _connMgr == null)
            {
                Notify.Warn("Cannot create the socket without a valid NetworkNode and ConnectionManager service");
                return false;
            }

            if (IsOpen())
            {
                Notify.Warn("Tried to call start when the socket is already open?!");
                return false;
            }

            if (_port == 0)
            {
                Node.HandleException(new InvalidOperationException("Tried to create socket with a port of 0!"));
                return false;
            }

            _enet_host = new();

            try
            {

                string displayIP = string.IsNullOrEmpty(_ip) ? "127.0.0.1" : _ip;

                switch (Type)
                {

                    case SocketType.Server:
                        _enet_host.Create(_enet_addr, MaxClients);
                        Notify.Info($"Starting listening on {displayIP}:{Port}... Peer limit: {MaxClients}");
                        break;

                    case SocketType.Client:
                        _enet_host.Create();
                        _enet_peer = _enet_host.Connect(_enet_addr);

                        Notify.Info($"Trying to connect to {displayIP}:{Port}...");
                        break;
                }

                // Let's begin our clock!
                Clock.Start();

                CreateTimeMono.Value = Library.Time;
                Notify.Debug("Successfully created the Socket!");
            }
            catch (Exception ex)
            {
                Node.HandleException(ex);
                return false;
            }

            return true;
        }

        public void Dispose(bool disposing)
        {
            Clock?.Stop();

            if (!IsServer())
                _enet_peer?.DisconnectNow(0);

            _enet_host?.Flush();

            if (disposing)
                _enet_host.Dispose();

            _enet_host = null;
            _enet_peer = null;
        }

        public void Dispose() => Dispose(true);

        /// <summary>
        /// If this socket represents a server
        /// </summary>
        /// <returns></returns>
        public bool IsServer() => _type == SocketType.Server;
        public bool IsOpen() => _enet_host != null;

        /// <summary>
        /// Tries to update the <see cref="ClockStrategy"/> of the internal <see cref="Clock"/> if <see cref="IsOpen"/> == false
        /// </summary>
        /// <param name="strategy"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>

        public bool SetClockStrategy([NotNull] ClockStrategy strategy)
        {
            if (strategy == null)
                throw new ArgumentNullException("Must have a valid ClockStrategy instance!");

            if (IsOpen())
            {
                InvalidOperationException exception = new InvalidOperationException("Cannot manage the clock with the socket open!");

                if (Node != null)
                {
                    Node.HandleException(exception);
                    return false;
                }

                // Don't have a node attached. Let's scream
                throw exception;
            }

            _clock._strat = strategy;
            return true;
        }

        internal void _Internal_SetupFor([NotNull] NetworkNode node)
        {
            if (node == null)
                throw new ArgumentNullException("Must have a valid NetworkNode instance!");

            // Set NetworkNode, ServiceManager, ConnectionManager, and Clock
            this._node = node;

            if (Node._socket != null)
            {
                Node.HandleException(new InvalidOperationException("NetworkNode cannot have more than one Socket associated with it!"));
                return;
            }

            // Let's set Node to use this
            Node._Internal_SetSocket(this);

            // Fetches or creates a connection manager
            _connMgr = Node.Services.GetService<ConnectionManager>();

            if (_connMgr == null)
                _connMgr = new ConnectionManager(Node.Services);
            
        }

    }
}