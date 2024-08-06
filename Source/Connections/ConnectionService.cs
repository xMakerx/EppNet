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

                _connections = new PageList<Connection>(itemsPerPage);
            }
            else if (_socket.MaxClients > 1)
            {
                _connections = new OrderedDictionary<ulong, Connection>();
            }
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
            if (_connections is PageList<Connection> pageList)
                pageList.Dispose();
        }

        public void EjectAll() => EjectAll(DisconnectReason.Unknown);

        public void EjectAll(DisconnectReason reason)
        {

            Action<Connection> action = (Connection c) =>
            {
                Notify.Debug($"Forcibly ejected {c}...");
                c.Eject(reason);
                OnConnectionLost?.Invoke(new(c, reason));
            };

            if (_socket.MaxClients > 64)
            {
                PageList<Connection> pList = _connections as PageList<Connection>;
                pList.DoOnActive(action);
                pList.Clear();
                pList.PurgeEmptyPages();
            }
            else
            {
                if (_socket.MaxClients > 1)
                {
                    OrderedDictionary<ulong, Connection> dict = _connections as OrderedDictionary<ulong, Connection>;
                    foreach (Connection c in dict.Values)
                        action.Invoke(c);

                    dict.Clear();
                }
                else
                {
                    action.Invoke(Peer);
                    Peer.Dispose();
                    Peer = null;
                }

            }
        }

        public bool HandleConnectionEstablished(Peer enetPeer)
        {

            Connection conn;
            bool added;

            if (_socket.MaxClients > 64)
            {
                PageList<Connection> pList = _connections as PageList<Connection>;
                added = pList.TryAllocate(enetPeer.ID, out conn);
            }
            else
            {
                conn = new Connection();

                if (_socket.MaxClients > 1)
                {
                    OrderedDictionary<ulong, Connection> dict = _connections as OrderedDictionary<ulong, Connection>;
                    dict.Add(enetPeer.ID, conn);
                }
                else
                {
                    Peer = conn;
                }

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
            Action<Connection> action = (Connection c) =>
            {
                Notify.Debug($"Connection to {c} lost");
                OnConnectionLost?.Invoke(new DisconnectEvent(c, reason));
            };

            Connection conn;
            bool removed;

            if (_socket.MaxClients > 64)
            {
                PageList<Connection> pList = _connections as PageList<Connection>;
                pList.TryGetById(enetPeer.ID, out conn);
                action.Invoke(conn);
                removed = pList.TryFree(conn);
            }
            else
            {

                if (_socket.MaxClients > 1)
                {
                    OrderedDictionary<ulong, Connection> dict = _connections as OrderedDictionary<ulong, Connection>;
                    dict.TryGetValue(enetPeer.ID, out conn);
                    action.Invoke(conn);
                    removed = dict.Remove(enetPeer.ID);
                }
                else
                {
                    action.Invoke(Peer);
                    Peer.Dispose();
                    Peer = null;
                    removed = true;
                }

            }


            return removed;
        }

        public Connection Get(ulong id)
        {
            Connection conn;

            if (_socket.MaxClients > 64)
            {
                PageList<Connection> pList = _connections as PageList<Connection>;
                pList.TryGetById(id, out conn);
            }
            else
            {
                if (_socket.MaxClients > 1)
                {
                    OrderedDictionary<ulong, Connection> dict = _connections as OrderedDictionary<ulong, Connection>;
                    dict.TryGetValue(id, out conn);
                }
                else
                    conn = Peer;
            }

            if (conn == null)
                Notify.Error(new TemplatedMessage("Peer ID {id} does not represent a valid Connection!", id));

            return conn;
        }

        public virtual bool CanConnect(Peer peer) => true;

    }

}
