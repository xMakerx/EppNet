﻿///////////////////////////////////////////////////////
/// Filename: Server.cs
/// Date: September 5, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
using EppNet.core;
using EppNet.exception;

namespace EppNet.endpoint
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
            Network.Instance.Status = NetworkStatus.Online;
            return true;
        }

        public bool Start(int port, int maxClients)
        {
            if (Network.IsOnline())
                throw new NetworkException("You cannot have more than one endpoint!");

            _enet_addr.Port = (ushort)port;
            _enet_host.Create(_enet_addr, maxClients);
            return Create();
        }

    }

}