///////////////////////////////////////////////////////
/// Filename: Server.cs
/// Date: September 5, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Connections;
using EppNet.Logging;
using EppNet.Node;

using System;

namespace EppNet.Sockets
{

    public class ServerSocket : BaseSocket
    {

        public ServerSocket(NetworkNode node) : base(node, SocketType.Server)
        {
            this.MaxClients = 4095;
        }

        public override void OnPacketReceived(Peer peer, Packet packet)
        {
            throw new System.NotImplementedException();
        }

    }

}