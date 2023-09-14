/////////////////////////////////////////////
/// Filename: IMessageHandler.cs
/// Date: September 13, 2022
/// Author: Maverick Liberty
//////////////////////////////////////////////

namespace EppNet.Data
{

    public interface IMessageHandler
    {

        public void OnDatagramReceived(Datagram datagram);

    }

}
