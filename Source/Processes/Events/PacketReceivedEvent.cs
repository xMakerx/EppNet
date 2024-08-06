﻿///////////////////////////////////////////////////////
/// <summary>
/// Filename: PacketReceivedEvent.cs
/// Date: January 13, 2024
/// Author: Maverick Liberty
/// </summary>
///////////////////////////////////////////////////////

using ENet;

using EppNet.Connections;
using EppNet.Data.Datagrams;
using EppNet.Sockets;
using EppNet.Time;

namespace EppNet.Processes.Events
{

    public class PacketReceivedEvent : RingBufferEvent
    {


        public BaseSocket Socket { internal set; get; }
        public Connection Sender { internal set; get; }
        public byte[] Data { internal set; get; }
        public byte ChannelID { internal set; get; }

        /// <summary>
        /// Monotonic time of reception
        /// </summary>
        public Timestamp MonoTimestamp { internal set; get; }

        public IDatagram Datagram { internal set; get; }

        public PacketReceivedEvent()
        {
            this.Sender = null;
            this.ChannelID = 0;
            this.MonoTimestamp = default;
        }

        public void Initialize(Connection conn, byte[] data, byte channelID)
        {
            this.Sender = conn;
            this.Data = data;
            this.ChannelID = channelID;
            this.MonoTimestamp = Timestamp.FromMonoNow();
        }

        public override void Dispose()
        {
            this.Datagram = null;
            this.Sender = null;
            this.Data = null;
            this.ChannelID = 0;
            this.MonoTimestamp = default;
        }

    }

}
