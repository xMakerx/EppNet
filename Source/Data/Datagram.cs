///////////////////////////////////////////////////////
/// Filename: Datagram.cs
/// Date: September 10, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

namespace EppNet.Data
{

    public class Datagram : BytePayload
    {
        public byte Header { internal set; get; }

        protected Packet? _enet_packet;

        public Datagram() : base()
        {
            this.Header = 0;
            this._enet_packet = null;
        }

        protected override void _EnsureReadyToWrite()
        {
            if (_stream == null)
            {
                _stream = ObtainStream();
                WriteUInt8(Header);
            }

        }

    }

}
