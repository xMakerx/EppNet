///////////////////////////////////////////////////////
/// Filename: Connection.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Collections;
using EppNet.Data.Datagrams;
using EppNet.Logging;
using EppNet.Messaging;
using EppNet.Node;
using EppNet.Sockets;
using EppNet.Time;
using EppNet.Utilities;

namespace EppNet.Connections
{
    /// <summary>
    /// Yes, UDP doesn't have the concept of "connections" but
    /// for simplicity's sake we use the term to describe<br/> "known
    /// computers that have sent us datagrams" and keep who's
    /// who organized.
    /// </summary>

    public class Connection : Pageable, INodeDescendant, ILoggable
    {

        public ILoggable Notify { get => this; }
        public NetworkNode Node { get => Service?.Node; }

        /// <summary>
        /// The socket this connection originates from.
        /// </summary>
        public BaseSocket Socket { internal set; get; }
        public ConnectionService Service { internal set; get; }
        public ChannelService ChannelService
        {
            get
            {
                if (_channelService == null)
                {
                    _channelService = Node.Services.GetService<ChannelService>();
                }

                return _channelService;
            }
        }

        public Peer ENet_Peer { internal set; get; }

        /// <summary>
        /// The ENet-provided peer ID.
        /// </summary>
        public uint ENet_ID { internal set; get; }

        /// <summary>
        /// When this connection was established.
        /// </summary>
        public Timestamp EstablishedMs { internal set; get; }

        public bool IsAuthenticated { internal set; get; }

        private ChannelService _channelService;

        public Connection()
        {
            this.Service = null;
            this.Socket = null;
            this.ENet_Peer = default;
            this.ENet_ID = default;
            this.EstablishedMs = default;
            this.IsAuthenticated = false;
            this.Page = null;
            this.ID = -1;
            this._channelService = null;
        }

        public Connection(BaseSocket socket, Peer peer) : this()
            => _Internal_Setup(socket, peer);

        protected internal void _Internal_Setup(BaseSocket socket, Peer peer)
        {
            Guard.AgainstNull(socket);
            this.Service = socket.ConnectionService;
            this.Socket = socket;
            this.ENet_Peer = peer;
            this.ENet_ID = peer.ID;
            this.EstablishedMs = Timestamp.FromMonoNow();
            this.IsAuthenticated = false;
        }

        public override void Dispose()
        {
            this.ENet_Peer = default;
            this.ENet_ID = default;
            this.EstablishedMs = default;
            this.IsAuthenticated = false;
            base.Dispose();
        }

        /// <summary>
        /// Forcibly closes the connection.
        /// </summary>

        public void Eject() => Eject(DisconnectReasons.Ejected);

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
            => ChannelService?.TrySendDataTo(ENet_Peer, channelId, bytes, flags) == true;

        /// <summary>
        /// Packages a <see cref="IDatagram"/> (Calls <see cref="IDatagram.Pack"/>) and
        /// sends it with the specified <see cref="PacketFlags"/>.
        /// </summary>
        /// <param name="datagram"></param>
        /// <param name="flags"></param>

        public bool Send(IDatagram datagram, PacketFlags flags)
        {
            bool sent = ChannelService?.TrySendTo(ENet_Peer, datagram, flags) == true;

            if (sent)
                Notify.Debug($"Successfully sent Datagram {datagram.GetType().Name} to Peer {ENet_ID}");
            else
                Notify.Debug($"Failed to send Datagram {datagram.GetType().Name} to Peer {ENet_ID}");

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

        public bool IsServer() => !IsFree() && ENet_ID == 0;

        public override string ToString() => $"Connection ID {ENet_ID} {ENet_Peer.IP}:{ENet_Peer.Port}";
    }
}
