///////////////////////////////////////////////////////
/// Filename: Socket.cs
/// Date: September 5, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Core;
using EppNet.Exceptions;

using System;
using System.Diagnostics;

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
        Unregistered    = 1 << 0,
        Registered      = 1 << 1,
        Online          = 1 << 2,
        Disconnected    = 1 << 3
    }

    public abstract class Socket : IDisposable
    {

        public SocketType Type { get => _type; }
        protected SocketType _type;
        public SocketStatus Status
        {
            internal set
            {
                _status = value;
            }

            get => _status;
        }

        protected SocketStatus _status;

        public Timestamp CreateTimeMs => _createTimeMs;
        protected Timestamp _createTimeMs;

        public Timestamp LastPollTimeMs => _lastPollTimeMs;
        protected Timestamp _lastPollTimeMs;

        public Host ENetHost { get => _enet_host; }
        protected Host _enet_host;

        protected Address _enet_addr;
        protected Event _enet_event;

        public Socket()
        {
            if (Network.CanRegisterSockets())
                throw new NetworkException("You must initialize the Network first!");

            _createTimeMs = Timestamp.ZeroMonotonicMs();
            _lastPollTimeMs = Timestamp.ZeroMonotonicMs();

            this._type = SocketType.Unknown;
            this._status = SocketStatus.Unregistered;
            this._enet_host = null;
            this._enet_addr = default(Address);
            this._enet_event = default(Event);
        }

        public bool Register()
        {
            Trace.Assert(_type != SocketType.Unknown, "Socket#Register() is trying to register a Socket of Unknown type.");
            Trace.Assert(Network.CanRegisterSockets(), "Socket#Register() requires that the Network singleton is setup and initialized.");

            return Network.Instance.RegisterSocket(this);
        }

        /// <summary>
        /// Internal method to create the internal ENet <see cref="Host"/> object
        /// </summary>
        /// <returns></returns>

        protected abstract bool Create();

        /// <summary>
        /// Polls for new network events. By default this is non-blocking and will not wait to receive a new event.
        /// Specify a timeout value to make this poll blocking until either an event is received or the specified
        /// time has expired.
        /// </summary>
        /// <param name="timeoutMs"></param>
        /// <exception cref="NetworkException"></exception>

        public virtual void Poll(int timeoutMs = 0)
        {
            if (_enet_host == null || !Network.IsOnline())
                throw new NetworkException("Socket is not valid!");

            bool polled = false;

            while (!polled)
            {
                if (_enet_host.CheckEvents(out _enet_event) <= 0)
                {
                    if (_enet_host.Service(timeoutMs, out _enet_event) <= 0)
                        break;

                    polled = true;
                    _lastPollTimeMs.Set(Network.MonoTime);
                }

                switch (_enet_event.Type)
                {

                    case EventType.None:
                        break;

                    case EventType.Connect:
                        // A new peer has connected!
                        break;

                    case EventType.Disconnect:
                        // A peer has disconnected.
                        break;

                    case EventType.Timeout:
                        // A peer has timed out.
                        break;

                    case EventType.Receive:
                        // Received a packet
                        break;

                }

            }
        }

        public void Flush()
        {
            _enet_host?.Flush();
        }

        public void Dispose()
        {
            this.Close();
        }

        public void Close()
        {
            if (IsOpen())
            {
                _enet_host.Dispose();
                Network.Instance.UnregisterSocket(this);
            }
        }

        /// <summary>
        /// Checks if this socket represents a server
        /// </summary>
        /// <returns></returns>

        public bool IsServer() => _type == SocketType.Server;

        public bool IsOpen() => _enet_host != null;
        

    }

}