///////////////////////////////////////////////////////
/// Filename: ConnectionManager.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Sockets;
using EppNet.Utilities;

using System;
using System.Collections.Generic;

namespace EppNet.Connections
{

    public class ConnectionManager
    {

        public event Action<Connection> OnConnectionEstablished;
        public event Action<DisconnectEvent> OnConnectionLost;

        internal readonly Socket _socket;
        protected readonly Dictionary<uint, Connection> _connections;

        public ConnectionManager(Socket socket)
        {
            _socket = socket;
            _connections = new Dictionary<uint, Connection>();
        }

        public bool HandleNewConnection(Peer enetPeer)
        {
            Connection conn = new Connection(this, enetPeer);
            bool added = _connections.TryAddEntry(enetPeer.ID, conn);

            if (added)
                OnConnectionEstablished?.Invoke(conn);

            return added;
        }

        public bool HandleConnectionLost(Peer enetPeer, DisconnectReason reason) =>
            _connections.ExecuteIfExists(enetPeer.ID, (id, conn) =>
            {
                OnConnectionLost?.Invoke(new DisconnectEvent(conn, reason));
                _connections.Remove(id);
            });

        public Connection Get(uint id)
        {
            _connections.TryGetValue(id, out Connection conn);
            return conn;
        }

    }

}
