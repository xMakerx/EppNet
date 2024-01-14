///////////////////////////////////////////////////////
/// <summary>
/// Filename: PacketReceivedEvent.cs
/// Date: January 13, 2024
/// Author: Maverick Liberty
/// </summary>
///////////////////////////////////////////////////////

using ENet;

using EppNet.Core;

namespace EppNet.Processes.Events
{

    public struct PacketReceivedEvent
    {

        public static PacketReceivedEvent From(Event enetEvent) => new(enetEvent.Packet, enetEvent.Peer, enetEvent.ChannelID);

        public readonly Packet Packet;
        public readonly Peer Sender;
        public readonly byte ChannelID;

        /// <summary>
        /// Monotonic time of reception
        /// </summary>
        public readonly Timestamp MonoTimestamp;

        public PacketReceivedEvent(Packet packet, Peer sender, byte channelID)
        {
            this.Packet = packet;
            this.Sender = sender;
            this.ChannelID = channelID;
            this.MonoTimestamp = Timestamp.FromMonoNow();
        }

        public void Dispose()
        {
            Packet.Dispose();
        }

    }

}
