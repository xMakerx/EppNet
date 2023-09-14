///////////////////////////////////////////////////////
/// Filename: ConnectionManager.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Sockets;

using System;
using System.Collections.Generic;

namespace EppNet.Connections
{

    public class ConnectionManager
    {

        public event Action<Connection> OnConnectionEstablished;
        public event Action<DisconnectEvent> OnConnectionLost;

        internal readonly Socket _socket;
        protected readonly IDictionary<uint, Connection> _connections;

        public ConnectionManager(Socket socket)
        {
            _socket = socket;
            _connections = new Dictionary<uint, Connection>();
        }

        public bool HandleNewConnection(Peer enetPeer)
        {
            bool isNew = !_connections.ContainsKey(enetPeer.ID);

            if (isNew)
            {
                Connection conn = new Connection(this, enetPeer);
                _connections.Add(enetPeer.ID, conn);

                OnConnectionEstablished?.Invoke(conn);
            }

            return isNew;
        }

        public bool HandleConnectionLost(Peer enetPeer, DisconnectReason reason)
        {
            _connections.TryGetValue(enetPeer.ID, out Connection conn);

            if (conn != null)
            {
                _connections.Remove(enetPeer.ID);
                OnConnectionLost?.Invoke(new DisconnectEvent(conn, reason));
            }

            return (conn != null);
        }

        public Connection Get(uint id)
        {
            _connections.TryGetValue(id, out Connection conn);
            return conn;
        }

    }

}
