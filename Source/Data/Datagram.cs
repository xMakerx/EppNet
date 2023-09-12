///////////////////////////////////////////////////////
/// Filename: Datagram.cs
/// Date: September 10, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Connections;

namespace EppNet.Data
{

    public class Datagram : BytePayload, IDatagram
    {
        public byte Header { internal set; get; }

        /// <summary>
        /// This is populated remotely.
        /// </summary>
        public Connection Sender { internal set; get; }
        public byte ChannelID { internal set; get; }

        public Datagram() : base()
        {
            this.Header = 0x0;
            this.ChannelID = 0x0;
            this.Sender = null;
        }

        protected override void _EnsureReadyToWrite()
        {
            if (_stream == null)
            {
                _stream = ObtainStream();
                WriteHeader();
            }

        }

        public virtual void Read() { }

        public virtual void WriteHeader() => WriteUInt8(Header);

        public byte GetHeader() => Header;
        public byte GetChannelID() => ChannelID;

        public Connection GetSender() => Sender;
    }

}
