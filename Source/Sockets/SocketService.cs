///////////////////////////////////////////////////////
/// Filename: SocketService.cs
/// Date: July 16, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Logging;
using EppNet.Services;

using System;

namespace EppNet.Sockets
{

    public class SocketService : Service
    {

        /// <summary>
        /// The <see cref="ISocket"/> associated with this service
        /// </summary>
        public ISocket Socket { protected set; get; }

        public SocketService(ServiceManager svcMgr) : base(svcMgr)
        {
            this.Socket = null;
        }

        /// <summary>
        /// Sets the <see cref="ISocket"/> associated with this service
        /// </summary>

        public void SetSocket(ISocket socket)
        {

            if (socket == null)
            {
                ArgumentNullException exp = new(nameof(socket), "Cannot set the Socket to null!");
                Notify.Error(exp.Message, exp);
                return;
            }

            // Don't do anything if we pass the same socket
            if (socket == Socket)
                return;

            if (Status != ServiceState.Offline)
            {
                Notify.Warning("Service must be stopped to change associated Socket!");
                return;
            }

            this.Socket = socket;
            Notify.Debug("Updated the associated Socket!");
        }

        /// <summary>
        /// Requests the service to start.<br/>
        /// Ensure a Socket is associated with <see cref="SetSocket(ISocket)"/> or the request will fail.
        /// </summary>
        /// <returns>Whether or not the request went through.</returns>

        public override bool Start()
        {
            if (Socket == null)
            {
                Notify.Error("Cannot start service if no Socket is associated with this service!");
                return false;
            }

            // For the most part, we don't have to do anything special. Use the base class's Start
            return base.Start();
        }

    }

}