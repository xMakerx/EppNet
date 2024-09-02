///////////////////////////////////////////////////////
/// Filename: Server.cs
/// Date: September 5, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Connections;
using EppNet.Node;

namespace EppNet.Sockets
{

    public class ServerSocket : BaseSocket
    {

        public ConnectionService ConnectionService { private set; get; }

        public ServerSocket(NetworkNode node) : this(node, ConnectionService.ENet_MaxClients) { }

        public ServerSocket(NetworkNode node, int maxClients) : base(node, SocketType.Server)
        {
            this.MaxClients = maxClients;
            this.ConnectionService = null;
        }

        public override void OnPeerConnected(Peer peer)
        {
            if (ConnectionService != null)
                ConnectionService.HandleConnectionEstablished(peer);
            else
                Companion = new ClientConnection(this, peer);
        }

        public override void OnPeerDisconnected(Peer peer, uint disconnectReasonIdx)
        {
            ConnectionService.HandleConnectionLost(peer, DisconnectReasons.GetFromID(disconnectReasonIdx));
        }

        protected override void _Internal_ValidateDependencies()
        {
            base._Internal_ValidateDependencies();

            if (MaxClients > 1)
                ConnectionService = Node.Services.GetOrCreate<ConnectionService>();
        }

        public override bool CanConnect(Peer peer)
        {
            bool canConnect = base.CanConnect(peer);

            if (ConnectionService != null)
                canConnect &= ConnectionService.CanConnect(peer);
            else
                canConnect &= Companion == null;

            return canConnect;
        }


    }

}