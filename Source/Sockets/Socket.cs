///////////////////////////////////////////////////////
/// Filename: Socket.cs
/// Date: September 5, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Connections;
using EppNet.Core;
using EppNet.Data;
using EppNet.Exceptions;
using EppNet.Processes.Events;
using EppNet.Sim;

using System;

namespace EppNet.Sockets
{
    public enum SocketType : byte
    {
        Unknown         = 0,
        Server          = 1,
        Client          = 2
    }

    public enum SocketStatus : byte
    {
        Unknown         = 0,
        Uninitialized   = 1 << 0,
        Initialized     = 1 << 1,
        Online          = 1 << 2,
        Disconnected    = 1 << 3
    }

    public abstract class Socket : IDisposable
    {

        public SocketType Type { get; }
        public SocketStatus Status { protected set; get; }
        public ConnectionManager ConnectionManager { protected set; get; }

        public MessageDirector MessageDirector { protected set; get; }

        public Timestamp CreateTimeMs { protected set; get; }
        public Timestamp LastPollTimeMs { protected set; get; }

        public Host ENetHost { protected set; get; }

        public Action OnStart;

        /// <summary>
        /// Called prior to cleaning up and closing the native ENetSocket
        /// </summary>
        
        public Action OnStopRequested;

        /// <summary>
        /// Called when the native ENet socket has been closed.
        /// </summary>
        public Action OnStopped;

        protected Address _enet_addr;
        protected Event _enet_event;

        public Socket(SocketType type)
        {
            this.Type = type;
            this.Status = SocketStatus.Uninitialized;
            this.ENetHost = null;

            this.MessageDirector = null;
            this.ConnectionManager = null;

            this.CreateTimeMs = Timestamp.ZeroMonotonicMs();
            this.LastPollTimeMs = Timestamp.ZeroMonotonicMs();

            // Actions
            this.OnStart = null;
            this.OnStopRequested = null;
            this.OnStopped = null;

            this._enet_addr = default;
            this._enet_event = default;
        }

        /// <summary>
        /// Internal method to create the internal ENet <see cref="Host"/> object
        /// </summary>
        /// <returns></returns>

        protected virtual bool Create()
        {
            CreateTimeMs.SetToMonoNow();
            ConnectionManager = new(this);
            MessageDirector = new();
            OnStart?.Invoke();

            this.Status = SocketStatus.Online;
            return true;
        }

        /// <summary>
        /// If you override this, make sure you call this or
        /// call <see cref="ConnectionManager.HandleNewConnection(Peer)"/>
        /// manually.
        /// </summary>
        /// <param name="peer"></param>

        protected virtual void OnPeerConnected(Peer peer)
        {
            ConnectionManager.HandleNewConnection(peer);
        }

        protected virtual void OnPeerDisconnected(Peer peer, DisconnectReason reason)
        {
            ConnectionManager.HandleConnectionLost(peer, reason);
        }

        /// <summary>
        /// Polls for new network events. By default this is non-blocking and will not wait to receive a new event.
        /// Specify a timeout value to make this poll blocking until either an event is received or the specified
        /// time has expired.
        /// </summary>
        /// <param name="timeoutMs"></param>
        /// <exception cref="NetworkException"></exception>

        public virtual void Poll(int timeoutMs = 0)
        {
            if (!IsOpen())
                throw new NetworkException("Socket has not been initialized!");

            bool polled = false;

            while (!polled)
            {
                if (ENetHost.CheckEvents(out _enet_event) <= 0)
                {
                    if (ENetHost.Service(timeoutMs, out _enet_event) <= 0)
                        break;

                    polled = true;
                    LastPollTimeMs.Set(Simulation.MonoTime);
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
                        // A peer has disconnected.
                        OnPeerDisconnected(_enet_event.Peer, DisconnectReason.Quit);
                        break;

                    case EventType.Timeout:
                        // A peer has timed out.
                        OnPeerDisconnected(_enet_event.Peer, DisconnectReason.TimedOut);
                        break;

                    case EventType.Receive:
                        // Received a packet
                        PacketReceivedEvent evt = PacketReceivedEvent.From(this, _enet_event);
                        Connection conn = ConnectionManager.Get(_enet_event.Peer.ID);

                        if (conn == null)
                        {
                            // Received a packet from an unknown connection??
                            MessageDirector.OnPacketReceived(conn, evt);
                        }

                        OnPacketReceived(evt);
                        break;

                }

            }
        }

        public void Flush() => ENetHost?.Flush();

        public void Dispose()
        {
            this.Close();
        }

        public void Close()
        {
            if (IsOpen())
            {
                OnStopRequested?.Invoke();
                ENetHost.Flush();
                ENetHost.Dispose();

                // Let's call our shutdown handlers.
                OnStopped?.Invoke();
            }
        }

        /// <summary>
        /// Checks if this socket represents a server
        /// </summary>
        /// <returns></returns>

        public bool IsServer() => Type == SocketType.Server;

        public bool IsOpen() => ENetHost != null;
        

    }

}