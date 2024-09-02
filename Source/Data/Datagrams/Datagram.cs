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

    public abstract class Datagram : BytePayload, IDatagram, ILoggable
    {

        public static byte GetDatagramIndexAndData(BytePayload payload, out byte data)
        {
            byte b = payload.ReadByte();
            data = 0;

            if (AvailableHeaderBits > 0)
            {
                // First, Let's extract the bits related to the Datagram's key (index)
                // This starts at bit 0 and goes to (8 - AvailableHeaderBits) - 1.
                byte indexByte = (byte)(b & ((1 << 8 - AvailableHeaderBits) - 1));

                // Now, let's fetch the data
                int mask = (1 << AvailableHeaderBits) - 1;
                int shifted = b >> (8 - AvailableHeaderBits);
                int extractedBits = shifted & mask;
                data = (byte)extractedBits;

                b = indexByte;
            }

            return b;
        }

        /// <summary>
        /// The length of every header in bytes
        /// </summary>
        public static int HeaderByteLength { internal set; get; }

        public static int AvailableHeaderBits { internal set; get; }
        public static int MaxHeaderDataDecimalValue { internal set; get; }

        public ILoggable Notify { get => this; }

        public byte Index { internal set; get; }

        /// <summary>
        /// Denotes special data that should be at most 3 bits
        /// and is sent within the header.
        /// </summary>
        public byte HeaderData { set; get; }

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

        public virtual void WriteHeader()
        {
            this.WriteUInt8(GetHeader());
        }

        public byte GetHeader()
        {
            byte header = Index;

            // Let's see if we have header data to write.
            if (HeaderData > 0 && HeaderFitsExtraData())
            {
                byte edit = header;
                int startBit = 8 - AvailableHeaderBits;
                int mask = ((1 << AvailableHeaderBits) - 1) << startBit;
                edit &= (byte)~mask;

                int alignedSourceBits = (HeaderData << startBit) & mask;
                edit |= (byte)alignedSourceBits;
                header = edit;
            }

            return header;
        }

        public byte GetChannelID() => ChannelID;

        public Connection GetSender() => Sender;

        public bool HeaderFitsExtraData()
            => HeaderData <= MaxHeaderDataDecimalValue;
        public bool HasHeaderData()
            => HeaderData > 0;
    }

}
