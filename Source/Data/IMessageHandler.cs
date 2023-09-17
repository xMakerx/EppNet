/////////////////////////////////////////////
/// Filename: IMessageHandler.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
//////////////////////////////////////////////

namespace EppNet.Data
{

    public interface IMessageHandler
    {

        public void OnDatagramReceived(Datagram datagram);

    }

}
