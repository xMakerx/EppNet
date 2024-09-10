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
using EppNet.Snapshots;
using EppNet.Sockets;
using EppNet.Utilities;

using System;
using System.Timers;

namespace EppNet.Connections
{

    /// <summary>
    /// Yes, UDP doesn't have the concept of "connections" but
    /// for simplicity's sake we use the term to describe<br/> "known
    /// computers that have sent us datagrams" and keep who's
    /// who organized.
    /// </summary>

    public abstract class Connection : Pageable, INodeDescendant, ILoggable
    {

        public ILoggable Notify { get => this; }
        public NetworkNode Node { internal set; get; }

        /// <summary>
        /// The socket this connection originates from.
        /// </summary>
        public BaseSocket Socket { internal set; get; }
        public ConnectionService Service { internal set; get; }
        public ChannelService ChannelService { internal set; get; }

        public SnapshotServiceBase SnapshotService { internal set; get; }

        public Peer ENet_Peer { internal set; get; }

        /// <summary>
        /// The ENet-provided peer ID.
        /// </summary>
        public uint ENet_ID { internal set; get; }

        /// <summary>
        /// When this connection was established.
        /// </summary>
        public TimeSpan Established { internal set; get; }

        public bool IsAuthenticated { internal set; get; }

        /// <summary>
        /// Is this connection up-to-date and receiving a consistent flow of
        /// snapshots that
        /// </summary>
        public bool IsSynchronized { internal set; get; }

        public DesyncEvent LastDesyncEvent { internal set; get; }

        /// <summary>
        /// The last time we received a snapshot from this connection
        /// </summary>
        public TimeSpan LastReceivedSnapshot { internal set; get; }

        public Timer SnapshotCheckTimer { protected set; get; }

        protected Connection()
        {
            this.Service = null;
            this.Socket = null;
            this.Node = null;
            this.Service = null;
            this.ChannelService = null;

            this.ENet_Peer = default;
            this.ENet_ID = default;
            this.Established = LastReceivedSnapshot = default;
            this.IsAuthenticated = false;
            this.SnapshotCheckTimer = null;
            this.Page = null;
            this.ID = -1;
        }

        protected Connection(BaseSocket socket, Peer peer) : this()
            => _Internal_Setup(socket, peer);

        protected internal virtual void _Internal_Setup(BaseSocket socket, Peer peer)
        {
            Guard.AgainstNull(socket);

            if (Socket == null)
            {
                // We only need to set this once.
                this.Socket = socket;
                this.Node = socket.Node;
                this.Service = Node.Services.GetService<ConnectionService>();
                this.ChannelService = Node.Services.GetService<ChannelService>();
                this.SnapshotService = Node.Services.GetService<SnapshotServiceBase>();
            }

            this.ENet_Peer = peer;
            this.ENet_ID = peer.ID;
            this.Established = socket.Node.Time;
            this.IsAuthenticated = IsSynchronized = false;

            if (SnapshotService != null)
            {
                // We're interested in the snapshot timer
                this.SnapshotCheckTimer = new(SnapshotService.SnapshotInterval * 1000d);
                this.SnapshotCheckTimer.AutoReset = true;
                //this.SnapshotCheckTimer.Elapsed += 
            }
        }

        public override void Dispose()
        {
            this.ENet_Peer = default;
            this.ENet_ID = default;
            this.Established = LastReceivedSnapshot = default;
            this.IsAuthenticated = IsSynchronized = false;
            this.LastDesyncEvent = null;
            base.Dispose();
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

        public bool SendInstant(IDatagram datagram)
            => Send(datagram, PacketFlags.Instant);

        /// <summary>
        /// The server always receives ID 0.
        /// </summary>
        /// <returns></returns>

        public virtual bool IsServer()
            => !IsFree() && ENet_ID == 0;

        public override string ToString()
            => $"Connection ID {ENet_ID} {ENet_Peer.IP}:{ENet_Peer.Port}";
    }
}
