///////////////////////////////////////////////////////
/// Filename: Network.cs
/// Date: September 5, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Exceptions;
using EppNet.Sockets;
using EppNet.Utilities;

using System;
using System.Collections.Generic;

namespace EppNet.Core
{

    public class Network
    {

        #region Static members
        public static Network Instance { get => _instance; }
        protected static Network _instance;

        /// <summary>
        /// Checks if the network is online (i.e. connected to a host or accepting clients)
        /// </summary>
        /// <returns></returns>

        public static bool IsOnline()
        {
            if (Network.Instance == null)
                return false;

            return Network.Instance.Status.IsFlagSet(NetworkStatus.Online);
        }

        /// <summary>
        /// Checks if the Network is ready to register sockets.
        /// </summary>
        /// <returns></returns>

        public static bool CanRegisterSockets() => Network.Instance != null && Network.Instance.Status >= NetworkStatus.Initialized;

        /// <summary>
        /// Fetches the monotonic time as a <see cref="Timestamp"/> or returns 0 if ENet hasn't been initialized.
        /// Monotonic time is the time since this device was powered on and is maintained on the kernel
        /// </summary>

        public static Timestamp MonotonicTimestamp
        {
            get
            {
                if (Instance != null && Instance._enet_initialized)
                    _monotonicTimestamp.Set(MonotonicTime);

                return _monotonicTimestamp;
            }
        }

        private static Timestamp _monotonicTimestamp = new Timestamp(TimestampType.Milliseconds, true, 0L);

        /// <summary>
        /// Shorthand for <see cref="MonotonicTimestamp"/>
        /// </summary>
        public static Timestamp MonotoTs => MonotonicTimestamp;

        /// <summary>
        /// Shorthand for <see cref="MonotonicTime"/>
        /// </summary>
        public static uint MonoTime => MonotonicTime;

        /// <summary>
        /// Fetches the monotonic time or returns 0 if ENet hasn't been initialized.
        /// Monotonic time is the time since this device was powered on and is maintained on the kernel
        /// </summary>

        public static uint MonotonicTime
        {
            get
            {
                uint t = 0;

                if (Instance != null && Instance._enet_initialized)
                    t = ENet.Library.Time;

                return t;
            }
        }

        #endregion

        public System.Exception Error;

        public event Action<NetworkStatus, NetworkStatus> OnStatusChanged;

        public NetworkStatus Status {
            internal set
            {
                if (value != _status)
                {
                    OnStatusChanged?.Invoke(_status, value);
                    _status = value;
                }

            }
            get => _status;
        }

        protected NetworkStatus _status;

        protected HashSet<Socket> _sockets;

        protected bool _enet_initialized;

        public Network()
        {
            if (_instance != null)
                throw new NetworkException("There can be at most one Network instance.");

            Network._instance = this;
            Error = null;

            _status = NetworkStatus.Ready;
            _sockets = new HashSet<Socket>();

            _enet_initialized = false;
        }

        ~Network()
        {
            Reset();

            if (_enet_initialized)
                ENet.Library.Deinitialize();
        }

        /// <summary>
        /// Resets all sockets but does not deinitialize ENet!
        /// </summary>
        public void Reset()
        {
            // Clears the posted exception if one exists.
            ClearException();

            foreach (Socket socket in _sockets)
                socket.Close();

            _sockets.Clear();
        }

        public void PostException(System.Exception exception)
        {
            Error = exception;
            Status = _status.SetFlag(NetworkStatus.Error, true);
            throw exception;
        }

        public void ClearException()
        {
            Error = null;

            if (_status.IsFlagSet(NetworkStatus.Error))
                Status = _status.ClearFlags(NetworkStatus.Error);
        }

        /// <summary>
        /// Registers the specified <see cref="Socket"/> to be managed by the Network. Socket
        /// cannot be a duplicate or null.
        /// </summary>
        /// <param name="socket"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>

        public bool RegisterSocket(Socket socket)
        {
            if (socket == null)
                throw new ArgumentNullException("#RegisterSocket(Socket) must be passed a valid Socket instance!");

            if (_sockets.Contains(socket))
                throw new ArgumentException("#RegisterSocket(Socket) was passed a Socket that was already registered!");

            return _sockets.Add(socket);
        }

        public bool UnregisterSocket(Socket socket)
        {
            if (!(socket != null && _sockets.Contains(socket)))
                return false;

            return _sockets.Remove(socket);
        }

        /// <summary>
        /// Fetches a registered <see cref="Socket"/> of the specified type.
        /// If you want a subclass, <see cref="GetSocketOfType{T}(Type)"/> to prevent a cast.
        /// </summary>
        /// <param name="typeIn"></param>
        /// <returns></returns>

        public Socket GetSocketOfType(SocketType typeIn)
        {
            foreach (Socket socket in _sockets)
            {
                if (socket.Type == typeIn)
                    return socket;
            }

            return null;
        }

        /// <summary>
        /// Fetches a registered <see cref="Socket"/> that derives from the specified <see cref="Type"/>
        /// NOTE: Returns null if no socket found
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="socketType"></param>
        /// <returns></returns>

        public T GetSocketOfType<T>(Type socketType) where T : Socket
        {

            foreach (Socket socket in _sockets)
            {
                if (socket.GetType().IsAssignableFrom(socketType))
                    return (T)socket;
            }

            return null;

        }

        public bool Initialize() => Initialize(null);

        /// <summary>
        /// Initializes the ENet library with callbacks (if specified).
        /// <see cref="Callbacks"/> for more information.
        /// Passing null is the equivalent of initializing ENet without special callbacks.
        /// </summary>
        /// <param name="callbacks"></param>
        /// <returns></returns>

        public bool Initialize(Callbacks callbacks)
        {
            if (_enet_initialized)
                return _enet_initialized;

            if (callbacks != null)
                _enet_initialized = ENet.Library.Initialize(callbacks);
            else
                _enet_initialized = ENet.Library.Initialize();

            if (!_enet_initialized)
                Status = NetworkStatus.Error;
            else
                Status = _status.SetFlag(NetworkStatus.Initialized, true);

            return _enet_initialized;
        }

        public bool Initialized() => _enet_initialized;

    }

}
