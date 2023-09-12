///////////////////////////////////////////////////////
/// Filename: Connection.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;
using EppNet.Core;
using EppNet.Sockets;
using EppNet.Data;

namespace EppNet.Connections
{

    public class Connection
    {

        /// <summary>
        /// The socket this connection originates from.
        /// </summary>
        public readonly Socket Origin;

        /// <summary>
        /// The ENet-provided peer ID.
        /// </summary>
        public uint ID { internal set; get; }

        /// <summary>
        /// When this connection was established.
        /// </summary>
        public readonly Timestamp Established;

        public bool IsAuthenticated { internal set; get; }

        protected readonly Peer _enet_peer;
        protected readonly ConnectionManager _manager;

        public Connection(ConnectionManager manager, Peer peer)
        {
            this._manager = manager;
            this._enet_peer = peer;

            this.Origin = manager._socket;
            this.ID = peer.ID;
            this.Established = Timestamp.FromMonoNow();
            this.IsAuthenticated = false;
        }

        public void Send(Datagram datagram, PacketFlags flags)
        {
            Packet enet_packet = new Packet();
            enet_packet.Create(datagram.Pack(), flags);

            _enet_peer.Send(datagram.ChannelID, ref enet_packet);
        }

        public void SendInstant(Datagram datagram) => Send(datagram, PacketFlags.Instant);

        /// <summary>
        /// The server always receives ID 0.
        /// </summary>
        /// <returns></returns>

        public bool IsServer() => ID == 0;

    }
}
