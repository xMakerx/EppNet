/////////////////////////////////////////////
/// Filename: IMessageHandler.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Data.Datagrams;

namespace EppNet.Data
{

    public interface IMessageHandler
    {

        public void OnDatagramReceived(Datagram datagram);

    }

}
