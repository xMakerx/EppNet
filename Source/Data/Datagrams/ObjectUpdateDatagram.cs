///////////////////////////////////////////////////////
/// Filename: ObjectUpdateDatagram.cs
/// Date: September 25, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Data.Datagrams
{

    public class ObjectUpdateDatagram : Datagram
    {

        public ObjectUpdateDatagram()
        {
            this.Header = 0x3;
            this.ChannelID = 0x1;
        }

    }

}
