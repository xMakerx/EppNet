///////////////////////////////////////////////////////
/// Filename: Connection.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Data.Datagrams;
using EppNet.Logging;
using EppNet.Messaging;
using EppNet.Sockets;
using EppNet.Time;

namespace EppNet.Connections
{
    /// <summary>
    /// Yes, UDP doesn't have the concept of "connections" but
    /// for simplicity's sake we use the term to describe<br/> "known
    /// computers that have sent us datagrams" and keep who's
    /// who organized.
    /// </summary>

    public class Connection : ILoggable
    {

        public ILoggable Notify { get => this; }

        /// <summary>
        /// The socket this connection originates from.
        /// </summary>
        public readonly BaseSocket Origin;

        /// <summary>
        /// The ENet-provided peer ID.
        /// </summary>
        public uint ID { internal set; get; }

        /// <summary>
        /// When this connection was established.
        /// </summary>
        public readonly Timestamp Established;

        public bool IsAuthenticated { internal set; get; }

        internal readonly Peer _enet_peer;
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
        /// Forcibly closes the connection.
        /// </summary>

        public void Eject() => Eject(DisconnectReason.Ejected);

        /// <summary>
        /// Forcibly closes the connection with the
        /// specified reason.
        /// </summary>
        /// <param name="reason"></param>

        public void Eject(DisconnectReason reason)
        {
            using DisconnectDatagram datagram = new(reason);
            SendInstant(datagram);
        }

        public bool Send(byte[] bytes, byte channelId, PacketFlags flags)
        {
            ChannelService channelService = _manager.Node.Services.GetService<ChannelService>();
            return channelService?.TrySendDataTo(_enet_peer, channelId, bytes, flags) == true;
        }

        /// <summary>
        /// Packages a <see cref="IDatagram"/> (Calls <see cref="IDatagram.Pack"/>) and
        /// sends it with the specified <see cref="PacketFlags"/>.
        /// </summary>
        /// <param name="datagram"></param>
        /// <param name="flags"></param>

        public bool Send(IDatagram datagram, PacketFlags flags)
        {
            ChannelService channelService = _manager.Node.Services.GetService<ChannelService>();

            if (channelService == null)
                return false;

            bool sent = channelService.TrySendTo(_enet_peer, datagram, flags);

            if (sent)
                Notify.Debug($"Successfully sent Datagram {datagram.GetType().Name} to Peer {ID}");
            else
                Notify.Debug($"Failed to send Datagram {datagram.GetType().Name} to Peer {ID}");

            return sent;
        }

        /// <summary>
        /// Sends a <see cref="IDatagram"/> with <see cref="PacketFlags.Instant"/>.
        /// </summary>
        /// <param name="datagram"></param>

        public bool SendInstant(IDatagram datagram) => Send(datagram, PacketFlags.Instant);

        /// <summary>
        /// The server always receives ID 0.
        /// </summary>
        /// <returns></returns>

        public bool IsServer() => ID == 0;

        public override string ToString() => $"Connection ID {_enet_peer.ID} {_enet_peer.IP}:{_enet_peer.Port}";

    }
}
