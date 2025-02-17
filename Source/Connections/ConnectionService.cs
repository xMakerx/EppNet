///////////////////////////////////////////////////////
/// Filename: ConnectionService.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Collections;
using EppNet.Logging;
using EppNet.Services;
using EppNet.Sockets;

using System;

namespace EppNet.Connections
{

    public class ConnectionService : Service
    {

        /// <summary>
        /// The maximum number of clients ENet allows per socket.
        /// </summary>
        public const int ENet_MaxClients = 4095;

        public event Action<ConnectEvent> OnConnectionEstablished;
        public event Action<DisconnectEvent> OnConnectionLost;

        /// <summary>
        /// Used on clients or servers with max clients of 1.
        /// </summary>
        public Connection Peer { internal set; get; }

        internal readonly BaseSocket _socket;

        // This is an object because this changes depending on how many max clients we have
        protected readonly object _connections;

        public ConnectionService(ServiceManager svcMgr) : base(svcMgr)
        {
            this._socket = svcMgr.Node.Socket;
            this.Peer = null;
            int maxClients = (_socket.MaxClients == ENet_MaxClients) ? ENet_MaxClients + 1 : _socket.MaxClients;

            if (_socket.MaxClients > 64)
            {
                if (maxClients % 64 != 0)
                    Node.HandleException(new InvalidOperationException("MaxClients must be a multiple of 64!"));

                int itemsPerPage = 64;
                int result = maxClients / itemsPerPage;

                if (result >= 4)
                    itemsPerPage = 256;

                _connections = new PageList<ConnectionSlot>(itemsPerPage);
            }
            else
                _connections = new OrderedDictionary<ulong, Connection>();
        }

        public override bool Start()
        {
            bool started = base.Start();

            if (started)
                Status = ServiceState.Online;

            return started;
        }

        public override bool Stop()
        {
            bool stopped = base.Stop();

            if (stopped)
                EjectAll();

            return stopped;
        }

        public override void Dispose(bool disposing)
        {
            if (_connections is PageList<ConnectionSlot> pageList)
                pageList.Dispose();
        }

        public void EjectAll() => EjectAll(DisconnectReasons.Unknown);

        public void EjectAll(DisconnectReason reason)
        {

            Action<ConnectionSlot> action = (ConnectionSlot slot) =>
            {
                if (slot.Connection is ClientConnection c)
                {
                    Notify.Debug($"Forcibly ejected {c}...");
                    c.Eject(reason);
                    OnConnectionLost?.Invoke(new(c, reason));
                }

            };

            if (_socket.MaxClients > 64)
            {
                PageList<ConnectionSlot> pList = _connections as PageList<ConnectionSlot>;
                pList.DoOnActive(action);
                pList.Clear();
                pList.PurgeEmptyPages();
            }
            else
            {
                OrderedDictionary<ulong, ClientConnection> dict = _connections as OrderedDictionary<ulong, ClientConnection>;
                
                foreach (ClientConnection c in dict.Values)
                {
                    Notify.Debug($"Forcibly ejected {c}...");
                    c.Eject(reason);
                    OnConnectionLost?.Invoke(new(c, reason));
                }

                dict.Clear();
            }
        }

        public bool HandleConnectionEstablished(Peer enetPeer)
        {

            ClientConnection conn = new(Node.Socket, enetPeer);
            bool added;

            if (_socket.MaxClients > 64)
            {
                PageList<ConnectionSlot> pList = _connections as PageList<ConnectionSlot>;
                added = pList.TryAllocate(enetPeer.ID, out ConnectionSlot slot);
                slot.Connection = conn;
            }
            else
            {
                OrderedDictionary<ulong, ClientConnection> dict = _connections as OrderedDictionary<ulong, ClientConnection>;
                dict.Add(enetPeer.ID, conn);
                added = true;
            }

            conn._Internal_Setup(_socket, enetPeer);

            if (added)
            {
                Notify.Debug($"New Connection Established to {enetPeer.IP}");
                OnConnectionEstablished?.Invoke(new ConnectEvent(conn));
            }

            return added;
        }

        public bool HandleConnectionLost(Peer enetPeer, DisconnectReason reason)
        {
            Action<ClientConnection> action = (ClientConnection c) =>
            {
                Notify.Debug($"Connection to {c} lost");
                OnConnectionLost?.Invoke(new DisconnectEvent(c, reason));
            };

            ClientConnection conn;
            bool removed;

            if (_socket.MaxClients > 64)
            {
                PageList<ConnectionSlot> pList = _connections as PageList<ConnectionSlot>;
                pList.TryGetById(enetPeer.ID, out ConnectionSlot slot);
                conn = (ClientConnection) slot.Connection;
                action.Invoke(conn);
                removed = pList.TryFree(ref slot);
            }
            else
            {
                OrderedDictionary<ulong, ClientConnection> dict = _connections as OrderedDictionary<ulong, ClientConnection>;
                dict.TryGetValue(enetPeer.ID, out conn);
                action.Invoke(conn);
                removed = dict.Remove(enetPeer.ID);
            }


            return removed;
        }

        public ClientConnection Get(ulong id)
        {
            ClientConnection conn;

            if (_socket.MaxClients > 64)
            {
                PageList<ConnectionSlot> pList = (PageList<ConnectionSlot>)_connections;
                pList.TryGetById(id, out ConnectionSlot slot);
                conn = (ClientConnection) slot.Connection;
            }
            else
            {
                OrderedDictionary<ulong, ClientConnection> dict = _connections as OrderedDictionary<ulong, ClientConnection>;
                dict.TryGetValue(id, out conn);
            }

            if (conn == null)
                Notify.Error(new TemplatedMessage("Peer ID {id} does not represent a valid Connection!", id));

            return conn;
        }

        public virtual bool CanConnect(Peer peer) => true;

    }

}
