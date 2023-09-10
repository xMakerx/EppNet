///////////////////////////////////////////////////////
/// Filename: Session.cs
/// Date: September 6, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Core;
using EppNet.Sockets;

namespace EppNet.Sessions
{

    public class Session
    {

        /// <summary>
        /// The socket this session is related to.
        /// </summary>
        public readonly Socket Socket;

        public Timestamp BeginTimeMs { get => _begin_time_ms; }
        protected readonly Timestamp _begin_time_ms;

        protected readonly Peer _enet_peer;

        /// <summary>
        /// The ID associated with this session
        /// </summary>
        public int ID { get => _peer_id; }
        protected int _peer_id;

        public Session(Socket socketIn, Peer peerIn)
        {
            this.Socket = socketIn;
            this._enet_peer = peerIn;

            // On the client side, my peer ID is always 0. We will have to wait for the server
            // to supply our remote ID.
            this._peer_id = (Socket.Type == SocketType.Server) ? (int)peerIn.ID : -1;
            this._begin_time_ms = Network.MonotonicTimestamp;

        }


    }

}
