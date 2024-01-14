///////////////////////////////////////////////////////
/// Filename: Connection.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Core;
using EppNet.Data;
using EppNet.Sockets;
using EppNet.Data.Datagrams;

using System;
using Notify = EppNet.Utilities.LoggingExtensions;

namespace EppNet.Connections
{
    /// <summary>
    /// Yes, UDP doesn't have the concept of "connections" but
    /// for simplicity's sake we use the term to describe<br/> "known
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
        /// Forcibly closes the connection.
        /// </summary>

        public void Eject()
        {
            using (DisconnectDatagram datagram = new(DisconnectReason.Ejected))
            {
                datagram.Write();
                SendInstant(datagram);
            }
        }

        /// <summary>
        /// Forcibly closes the connection with the
        /// specified reason.
        /// </summary>
        /// <param name="reason"></param>

        public void Eject(DisconnectReason reason)
        {
            using (DisconnectDatagram datagram = new(reason))
            {
                datagram.Write();
                SendInstant(datagram);
            }
        }

        public bool Send(byte[] bytes, byte channelId, PacketFlags flags)
        {
            // Create the ENet packet
            Packet packet = new();

            try
            {
                packet.Create(bytes, flags);

                // Send the packet to our ENet peer
                if (_enet_peer.Send(channelId, ref packet))
                {
                    Channel channel = Channel.GetById(channelId);
                    channel.DatagramsSent++;
                    return true;
                }
            }
            catch (Exception e)
            {
                Notify.Error($"Failed to send Datagram to Peer {ID}. Error: {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                // No matter what, dispose of this packet.
                packet.Dispose();
            }

            return false;
        }

        /// <summary>
        /// Packages a <see cref="IDatagram"/> (Calls <see cref="IDatagram.Pack"/>) and
        /// sends it with the specified <see cref="PacketFlags"/>.
        /// </summary>
        /// <param name="datagram"></param>
        /// <param name="flags"></param>

        public bool Send(IDatagram datagram, PacketFlags flags)
        {

            if (!datagram.Written)
                datagram.Write();

            bool sent = Send(datagram.Pack(), datagram.GetChannelID(), flags);

            if (sent)
                Notify.Debug($"Successfully sent Datagram {datagram.GetType().Name} to Peer {ID}");

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

    }
}
