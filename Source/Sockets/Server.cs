///////////////////////////////////////////////////////
/// Filename: Server.cs
/// Date: September 5, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Sockets
{

    public class Server : Socket
    {

        public Server() : base()
        {
            _type = SocketType.Server;
        }

        protected override bool Create()
        {
            _createTimeMs.SetToMonoNow();
            //Network.Instance.Status = NetworkStatus.Online;
            return true;
        }

        public bool Start(int port, int maxClients)
        {

            _enet_addr.Port = (ushort)port;
            _enet_host.Create(_enet_addr, maxClients);
            return Create();
        }

    }

}