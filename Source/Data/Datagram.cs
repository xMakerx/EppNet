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

        protected Packet? _enet_packet;

        public Datagram() : base()
        {
            this._enet_packet = null;
        }

    }

}
