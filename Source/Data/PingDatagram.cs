///////////////////////////////////////////////////////
/// Filename: PingDatagram.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Sim;

namespace EppNet.Data
{

    public class PingDatagram : Datagram
    {
        public ulong SentTime { internal set; get; }
        public ulong ReceivedTime { internal set; get; }

        public PingDatagram()
        {
            this.Header = 0x1;
            this.ChannelID = 0x0;
        }

        public override void Read()
        {
            base.Read();

            this.SentTime = ReadULong();

            if (Sender.IsServer())
            {
                this.ReceivedTime = ReadULong();
                Simulation.Get().Clock.ProcessPong(ReceivedTime, SentTime);
            }
            else
            {
                // This happens on the server
                PingDatagram pong = new PingDatagram();
                pong.WriteULong(SentTime);
                pong.WriteULong(ReceivedTime);

                // Send an acknowledgement!
                Sender.SendInstant(pong);
            }

        }

    }

}
