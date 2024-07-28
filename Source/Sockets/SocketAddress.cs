///////////////////////////////////////////////////////
/// Filename: SocketAddress.cs
/// Date: July 27, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;
using System;

namespace EppNet.Sockets
{

    /// <summary>
    /// Wrapper around ENet-CSharp's <see cref="Address"/>.<br/>
    /// Stores the set values of the hostname, IP, and port locally, and<br/>
    /// adds events that can be hooked into to listen for changes
    /// </summary>

    public struct SocketAddress
    {

        /// <summary>
        /// Called when the IP Address changes
        /// <br/>New address followed by the old address
        /// </summary>
        public Action<string, string> OnIPChanged;

        /// <summary>
        /// Called when the port changes
        /// <br/>New port followed by the old port.
        /// </summary>
        public Action<ushort, ushort> OnPortChanged;

        /// <summary>
        /// Called when the host name changes.
        /// <br/>New host name followed by the old host name.
        /// </summary>
        public Action<string, string> OnHostNameChanged;

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
                {
                    OnIPChanged?.Invoke(value, _ip);
                    _ip = value;
                }

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
                    OnPortChanged?.Invoke(value, _port);
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
                {
                    OnHostNameChanged?.Invoke(value, _hostName);
                    _hostName = value;
                }
            }

            get => _enet_addr.GetHost();

        }

        internal Address _enet_addr;
        internal string _ip;
        internal ushort _port;
        internal string _hostName;

        public SocketAddress()
        {
            this._enet_addr = new();
            this._ip = string.Empty;
            this._port = 0;
            this._hostName = string.Empty;
        }

        public SocketAddress(ushort port) : this()
        {
            this.Port = port;
        }

        public SocketAddress(string ipAddress, ushort port) : this()
        {
            this.IP = ipAddress;
            this.Port = port;
        }

        public SocketAddress(string ipAddress, ushort port, string hostName) : this(ipAddress, port)
        {
            this.HostName = hostName;
        }


    }

}