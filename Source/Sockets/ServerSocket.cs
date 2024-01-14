///////////////////////////////////////////////////////
/// Filename: Server.cs
/// Date: September 5, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using Notify = EppNet.Utilities.LoggingExtensions;

namespace EppNet.Sockets
{

    public class ServerSocket : Socket
    {

        public ServerSocket() : base(SocketType.Server) { }

        protected override bool Create()
        {
            _createTimeMs.SetToMonoNow();
            //Network.Instance.Status = NetworkStatus.Online;
            return true;
        }

        public bool Start(int port, int maxClients)
        {
            if (IsOpen())
            {
                Notify.Warn("Tried to call start when the server is already open?!");
                return false;
            }

            _enet_addr.Port = (ushort)port;
            ENetHost.Create(_enet_addr, maxClients);
            return Create();
        }

    }

}