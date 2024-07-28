///////////////////////////////////////////////////////
/// Filename: ClientSocket.cs
/// Date: July 27, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Connections;
using EppNet.Node;

using System;

namespace EppNet.Sockets
{

    public class ClientSocket : BaseSocket
    {

        public ClientSocket(NetworkNode node) : base(node, SocketType.Client)
        {

        }

        public bool ConnectTo(string host, ushort port)
        {
            if (IsOpen())
            {
                Node.HandleException(new InvalidOperationException("Already connected to a remote host! Have you tried disconnecting first?"));
                return false;
            }

            this.HostName = host;
            this.Port = port;

            // Create our ENet host and connect
            this._enet_host = new();
            _enet_host.Create();
            _enet_peer = _enet_host.Connect(_enet_addr);
            return true;
        }

        public void ConnectTo(string host, int port) => ConnectTo(host, Convert.ToUInt16(port));

        public override void OnPacketReceived(Peer peer, Packet packet)
        {
            
        }

        public override void OnPeerConnected(Peer peer)
        {
            ConnectionManager.HandleConnectionEstablished(peer);
        }

        public override void OnPeerDisconnected(Peer peer, uint disconnectReasonIdx)
        {
            ConnectionManager.HandleConnectionLost(peer, DisconnectReason.GetFromID(disconnectReasonIdx));
        }

    }

}
