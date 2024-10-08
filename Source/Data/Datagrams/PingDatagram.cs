﻿///////////////////////////////////////////////////////
/// Filename: PingDatagram.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Messaging;

using System;

namespace EppNet.Data.Datagrams
{

    public class PingDatagram : Datagram
    {
        public float Time { set; get; }
        public float ReceivedTime { set; get; }

        public PingDatagram()
        {
            this.ChannelID = (byte)Channels.Connectivity;
            this._collectible = false;
        }

        public override void Write()
        {
            base.Write();
            this.WriteFloat(Time);
            this.WriteFloat(ReceivedTime);
        }

        public override void Read()
        {
            base.Read();

            this.Time = this.ReadFloat();

            if (Sender.IsServer())
            {
                this.ReceivedTime = this.ReadFloat();

                TimeSpan remoteTimeSpan = TimeSpan.FromMilliseconds(Time);
                Sender.Node.Socket.Clock.Synchronize(remoteTimeSpan);
                return;
            }

            PingDatagram pong = new()
            {
                ReceivedTime = Time,
                Time = (float)Sender.Node.Socket.Node.Time.TotalMilliseconds
            };

            // Send an acknowledgement
            Sender.SendInstant(pong);
            pong.Dispose();
        }

    }

}
