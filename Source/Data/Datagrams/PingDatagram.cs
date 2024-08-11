///////////////////////////////////////////////////////
/// Filename: PingDatagram.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Messaging;
using EppNet.Sim;

using System;

namespace EppNet.Data.Datagrams
{

    public class PingDatagram : Datagram
    {
        public ulong SentTime { set; get; }
        public ulong ReceivedTime { set; get; }

        public PingDatagram()
        {
            this.Header = 0x1;
            this.ChannelID = (byte)Channels.Connectivity;
            this._collectible = false;
        }

        public override void Write()
        {
            base.Write();
            WriteULong(SentTime);
            WriteULong(ReceivedTime);
        }

        public override void Read()
        {
            base.Read();

            this.SentTime = ReadULong();

            if (Sender.IsServer())
            {
                this.ReceivedTime = ReadULong();

                TimeSpan remoteTimeSpan = TimeSpan.FromMilliseconds(ReceivedTime);
                Sender.Node.Socket.Clock.Synchronize(remoteTimeSpan);
                return;
            }

            // This happens on the server
            PingDatagram pong = new PingDatagram();
            pong.Write();

            // Send an acknowledgement!
            Sender.SendInstant(pong);
        }

    }

}
