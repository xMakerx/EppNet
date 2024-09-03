///////////////////////////////////////////////////////
/// Filename: ServerConnection.cs
/// Date: September 2, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
using ENet;

using EppNet.Sockets;

using System;

namespace EppNet.Connections
{
    /// <summary>
    /// This represents a connection to the server
    /// </summary>
    public class ServerConnection : Connection
    {

        /// <summary>
        /// This is the time the simulation started on the server-side.<br/>
        /// It is unaffected by clock synchronization events as this time
        /// is set at connection reception.
        /// </summary>
        public TimeSpan ServerTime { internal set; get; }

        /// <summary>
        /// Are we both up-to-date with the latest snapshots and have the necessary
        /// buffer to run the simulation?
        /// </summary>
        public bool IsSynchronized { internal set; get; }

        public ServerConnection(BaseSocket socket, Peer peer) : base(socket, peer)
        {
            this.ServerTime = default;
            this.IsSynchronized = false;
        }
    }
}
