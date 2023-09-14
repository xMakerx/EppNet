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
    /// <summary>
    /// Yes, UDP doesn't have the concept of "connections" but
    /// for simplicity's sake we use the term to describe "known
    /// computers that have sent us datagrams" and keep who's
    /// who organized.
    /// </summary>

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

        /// <summary>
        /// Packages a <see cref="IDatagram"/> (Calls <see cref="IDatagram.Pack"/>) and
        /// sends it with the specified <see cref="PacketFlags"/>.
        /// </summary>
        /// <param name="datagram"></param>
        /// <param name="flags"></param>

        public void Send(IDatagram datagram, PacketFlags flags)
        {
            Packet enet_packet = new Packet();
            enet_packet.Create(datagram.Pack(), flags);

            byte channelID = datagram.GetChannelID();
            Channel channel = Channel.GetById(channelID);
            channel.DatagramsSent++;

            _enet_peer.Send(datagram.GetChannelID(), ref enet_packet);
        }

        /// <summary>
        /// Sends a <see cref="IDatagram"/> with <see cref="PacketFlags.Instant"/>.
        /// </summary>
        /// <param name="datagram"></param>

        public void SendInstant(IDatagram datagram) => Send(datagram, PacketFlags.Instant);

        /// <summary>
        /// The server always receives ID 0.
        /// </summary>
        /// <returns></returns>

        public bool IsServer() => ID == 0;

    }
}
