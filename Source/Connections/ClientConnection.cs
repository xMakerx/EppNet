///////////////////////////////////////////////////////
/// Filename: ClientConnection.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
using ENet;

using EppNet.Data.Datagrams;
using EppNet.Sockets;

namespace EppNet.Connections
{
    public class ClientConnection : Connection
    {

        internal ClientConnection() { }

        internal ClientConnection(BaseSocket socket, Peer peer) : base(socket, peer) { }

        /// <summary>
        /// Forcibly closes the connection.
        /// </summary>

        public void Eject()
            => Eject(DisconnectReasons.Ejected);

        /// <summary>
        /// Forcibly closes the connection with the
        /// specified reason.
        /// </summary>
        /// <param name="reason"></param>

        public void Eject(DisconnectReason reason)
        {
            using DisconnectDatagram datagram = new(reason);
            SendInstant(datagram);
        }

    }
}
