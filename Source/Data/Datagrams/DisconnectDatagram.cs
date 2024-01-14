///////////////////////////////////////////////////////
/// Filename: DisconnectDatagram.cs
/// Date: September 25, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Connections;
using EppNet.Utilities;

namespace EppNet.Data.Datagrams
{

    public class DisconnectDatagram : Datagram
    {
        public DisconnectReason Reason { internal set; get; } 

        public DisconnectDatagram()
        {
            this.Reason = DisconnectReason.Quit;
            this.Header = 0x2;
            this.ChannelID = 0x0;
        }

        public DisconnectDatagram(DisconnectReason reason) : this()
        {
            this.Reason = reason;
        }

        public override void Write()
        {
            base.Write();

            DisconnectReason generic = DisconnectReason.GetFromID(Reason.ID);
            byte reasonId = Reason.ID;

            if (generic.Message != Reason.Message)
            {
                // The reason message is different than the generic. Let's turn on
                // the first bit to denote that.
                reasonId = reasonId.EnableBit(7);
                WriteByte(reasonId);
                WriteString8(Reason.Message);
                return;
            }

            WriteByte(reasonId);
        }

        public override void Read()
        {
            base.Read();

            byte reasonId = ReadByte();
            string reasonMessage = string.Empty;

            if (reasonId.IsBitOn(7))
            {
                // Bit 7 was enabled, so that means we have a custom message
                reasonId = reasonId.ResetBit(7);
                reasonMessage = ReadString8();
            }

            DisconnectReason? generic = DisconnectReason.GetFromID(Reason.ID);

            if (generic != null)
            {
                this.Reason = generic.Value;

                if (reasonMessage != string.Empty)
                    this.Reason.SetMessage(reasonMessage);
            }
            else
                this.Reason = new DisconnectReason(reasonId, reasonMessage);
        }

        public override void Dispose()
        {
            this.Reason = default;
            base.Dispose();
        }

    }

}
