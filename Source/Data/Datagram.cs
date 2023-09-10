///////////////////////////////////////////////////////
/// Filename: Datagram.cs
/// Date: September 10, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using System.IO;

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
                _stream = RecyclableStreamMgr.GetStream();

            if (_writer == null)
            {
                _writer = new BinaryWriter(_stream, Encoder);

                if (Header != 0)
                    _writer.Write(Header);
            }
        }

    }

}
