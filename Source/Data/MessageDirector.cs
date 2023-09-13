///////////////////////////////////////////////////////
/// Filename: MessageDirector.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Connections;

namespace EppNet.Data
{

    public class MessageDirector
    {

        public virtual void OnPacketReceived(Connection sender, Packet packet, byte channelID)
        {
            byte[] received_data = new byte[packet.Length];
            packet.CopyTo(received_data);
        }

    }

}
