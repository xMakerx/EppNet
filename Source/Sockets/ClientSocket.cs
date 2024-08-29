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

        public ClientSocket() : base(SocketType.Client) { }

        public ClientSocket(NetworkNode node) : base(node, SocketType.Client) { }

        public bool ConnectTo(string host, ushort port)
        {
            this.HostName = host;
            this.Port = port;
            return Create();
        }

        public void ConnectTo(string host, int port) => ConnectTo(host, Convert.ToUInt16(port));

        public override void OnPeerConnected(Peer peer)
        {
            this.Companion = new ServerConnection(this, peer);
        }

        public override void OnPeerDisconnected(Peer peer, uint disconnectReasonIdx)
        {
            this.Companion.Dispose();
        }
    }

}
