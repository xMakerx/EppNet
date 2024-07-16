///////////////////////////////////////////////////////
/// Filename: ISocket.cs
/// Date: July 16, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Sockets
{

    public enum SocketType
    {
        Unknown = 0,
        Server = 1,
        Client = 2
    }

    public interface ISocket
    {

        public SocketType Type { get; }

        /// <summary>
        /// If <see cref="SocketType.Server"/>, returns if the Socket is listening for new peers<br/>
        /// If a <see cref="SocketType.Client"/>, returns if we're connected to a server Socket.
        /// </summary>
        /// <returns></returns>
        public bool IsConnected();

        /// <summary>
        /// Whether or not this <see cref="ISocket"/> is a <see cref="SocketType.Server"/>
        /// </summary>
        /// <returns></returns>
        public bool IsServer() => Type == SocketType.Server;

    }

}