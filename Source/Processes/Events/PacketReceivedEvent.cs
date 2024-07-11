///////////////////////////////////////////////////////
/// <summary>
/// Filename: PacketReceivedEvent.cs
/// Date: January 13, 2024
/// Author: Maverick Liberty
/// </summary>
///////////////////////////////////////////////////////

using ENet;

using EppNet.Connections;
using EppNet.Sockets;
using EppNet.Time;

namespace EppNet.Processes.Events
{

    public class PacketReceivedEvent : RingBufferEvent
    {

        public static PacketReceivedEvent From(Socket socket, Event enetEvent)
        {
            Connection conn = socket.ConnectionManager.Get(enetEvent.Peer.ID);
            return new(conn, enetEvent.Packet, enetEvent.ChannelID);
        }

        public Socket Socket { internal set; get; }
        public Connection Sender { internal set; get; }
        public Packet Packet { internal set; get; }
        public byte ChannelID { internal set; get; }

        /// <summary>
        /// Monotonic time of reception
        /// </summary>
        public Timestamp MonoTimestamp { internal set; get; }

        public PacketReceivedEvent()
        {
            this.Packet = default;
            this.Sender = null;
            this.ChannelID = 0;
            this.MonoTimestamp = default;
        }

        public PacketReceivedEvent(Connection conn, Packet packet, byte channelID)
        {
            this.Packet = packet;
            this.Sender = conn;
            this.ChannelID = channelID;
            this.MonoTimestamp = Timestamp.FromMonoNow();
        }

        public void Initialize(Socket socket, Event enetEvent)
        {
            this.Packet = enetEvent.Packet;
            this.Sender = socket.ConnectionManager.Get(enetEvent.Peer.ID);
            this.ChannelID = enetEvent.ChannelID;
            this.MonoTimestamp = Timestamp.FromMonoNow();
        }

        public override void Dispose()
        {
            Packet.Dispose();
        }

    }

}
