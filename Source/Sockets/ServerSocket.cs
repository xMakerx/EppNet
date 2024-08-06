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

        public ServerSocket(NetworkNode node) : this(node, ConnectionService.ENet_MaxClients) { }

        public ServerSocket(NetworkNode node, int maxClients) : base(node, SocketType.Server)
        {
            this.MaxClients = maxClients;
        }


    }

}