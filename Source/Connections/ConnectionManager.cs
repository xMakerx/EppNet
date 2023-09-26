///////////////////////////////////////////////////////
/// Filename: ConnectionManager.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Sockets;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

            // Prevents an add'l hash lookup
            ref Connection valOrNew = ref CollectionsMarshal.GetValueRefOrAddDefault(_connections,
                enetPeer.ID, out bool isNew);

            if (isNew)
            {
                valOrNew = new Connection(this, enetPeer);
                OnConnectionEstablished?.Invoke(valOrNew);
            }

            return isNew;
        }

        public bool HandleConnectionLost(Peer enetPeer, DisconnectReason reason)
        {
            // Prevents an add'l hash lookup
            ref Connection valOrNull = ref CollectionsMarshal.GetValueRefOrNullRef(_connections, enetPeer.ID);

            if (!Unsafe.IsNullRef(ref valOrNull))
            {
                _connections.Remove(enetPeer.ID);
                OnConnectionLost?.Invoke(new DisconnectEvent(valOrNull, reason));
                return true;
            }

            return false;
        }

        public Connection Get(uint id)
        {
            _connections.TryGetValue(id, out Connection conn);
            return conn;
        }

    }

}
