///////////////////////////////////////////////////////
/// Filename: Datagram.cs
/// Date: September 10, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Connections;
using EppNet.Logging;
using EppNet.Messaging;

namespace EppNet.Data.Datagrams
{

    public class Datagram : BytePayload, IDatagram, ILoggable
    {
        public ILoggable Notify { get => this; }

        public byte Header { internal set; get; }
        public bool Written { internal set; get; }
        public long Size { get => Length; }

        /// <summary>
        /// This is populated remotely.
        /// </summary>
        public Connection Sender { internal set; get; }
        public byte ChannelID { internal set; get; }
        public bool Collectible { get => _collectible; }

        protected bool _collectible;

        public Datagram()
        {
            this.Header = 0x0;
            this.ChannelID = 0x0;
            this.Sender = null;
            this.Written = false;
            this._collectible = true;
        }

        public virtual void Read() { }
        public virtual void Write()
        {
            WriteHeader();
            this.Written = true;

            if (!_collectible && ChannelID != (byte)Channels.Connectivity)
                Notify.Warning("Datagram isn't collectible and isn't communicating on the connectivity channel. Consider making it collectible!");
        }

        public virtual void WriteHeader() => this.WriteUInt8(Header);

        public byte GetHeader() => Header;
        public byte GetChannelID() => ChannelID;

        public Connection GetSender() => Sender;
    }

}
