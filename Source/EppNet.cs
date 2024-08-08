///////////////////////////////////////////////////////
/// Filename: EppNet.cs
/// Date: July 29, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Exceptions;
using EppNet.Logging;
using EppNet.Node;
using EppNet.Sockets;

using System;

namespace EppNet
{

    public static class EppNet
    {

        /// <summary>
        /// Whether or not the C++ ENet library has been initialized.<br/>
        /// Don't worry about manually initializing ENet unless you're an advanced user.<br/><br/>
        /// See <see cref="NetworkNodeManager._Internal_TryRegisterNode(NetworkNode)"/> and <br/><see cref="NetworkNodeManager._Internal_TryUnregisterNode(NetworkNode)"/>
        /// </summary>
        public static bool ENet_Initialized { private set; get; }

        /// <summary>
        /// Tries to initialize the ENet library with no special callbacks<br/>
        /// For more information on <see cref="Callbacks"/>, please visit:<br/>
        /// https://github.com/nxrighthere/ENet-CSharp/tree/master?tab=readme-ov-file#integrate-with-a-custom-memory-allocator
        /// </summary>
        /// <exception cref="InvalidOperationException">C++ library failed to initialize</exception>
        /// <returns>Whether or not ENet was initialized</returns>
        public static bool InitializeENet() => _Internal_TryInitializeENet(null);

        /// <summary>
        /// Tries to initialize the ENet library with the specified callbacks<br/>
        /// For more information on <see cref="Callbacks"/>, please visit:<br/>
        /// https://github.com/nxrighthere/ENet-CSharp/tree/master?tab=readme-ov-file#integrate-with-a-custom-memory-allocator
        /// </summary>
        /// <exception cref="ArgumentNullException">Callbacks is null!</exception>
        /// <exception cref="InvalidOperationException">C++ library failed to initialize</exception>
        /// <returns>Whether or not ENet was initialized</returns>
        public static bool InitializeENet(Callbacks callbacks) => _Internal_TryInitializeENet(callbacks);

        /// <summary>
        /// Tries to deinitialize ENet if no nodes are registered.
        /// </summary>
        /// <returns>Whether or not ENet was deinitialized</returns>
        /// <exception cref="InvalidOperationException">A NetworkNode is registered</exception>
        public static bool DeinitializeENet()
        {
            if (!ENet_Initialized)
                return false;

            if (!NetworkNodeManager.IsEmpty())
                throw new InvalidOperationException("Cannot deinitialize the C++ ENet library if a NetworkNode is registered!");

            Library.Deinitialize();
            ENet_Initialized = false;
            return true;
        }

        /// <summary>
        /// Creates a new <see cref="NetworkNode"/> with a default <see cref="ServerSocket"/> associated with it.<br/>
        /// The following characteristics are set by default if not changed explicitly:<br/>
        /// - <see cref="NetworkNode.Name"/> is set to "DefaultServer"<br/>
        /// - <see cref="Exceptions.ExceptionStrategy"/> is set to <see cref="Exceptions.ExceptionStrategy.ThrowAll"/>, and<br/>
        /// - <see cref="LogLevelFlags"/> are set to <see cref="LogLevelFlags.InfoWarnErrorFatal"/>.<br/>
        /// <b>NOTE: </b>You are responsible for disposing of the node when you're done using it!
        /// </summary>
        /// <param name="listenPort">The port to listen for new clients on</param>
        /// <param name="name">The </param>
        /// <returns>A newly minted NetworkNode!</returns>

        public static NetworkNode CreateServer(int listenPort, string name = "DefaultServer", 
            ExceptionStrategy exceptStrat = ExceptionStrategy.ThrowAll,
            LogLevelFlags logLevelFlags = LogLevelFlags.InfoWarnErrorFatal)
        {
            NetworkNodeBuilder builder = new NetworkNodeBuilder(name, Distribution.Server)
                .SetExceptionStrategy(exceptStrat);

            NetworkNode node = builder.Build();
            ServerSocket socket = node.Socket as ServerSocket;
            socket.Port = (ushort) listenPort;
            socket.Notify.SetLogLevel(logLevelFlags);
            return node;
        }

        /// <summary>
        /// Creates a new <see cref="NetworkNode"/> with a default <see cref="ClientSocket"/> associated with it.<br/>
        /// The following characteristics are set by default if not changed explicitly:<br/>
        /// - <see cref="NetworkNode.Name"/> is set to "DefaultServer"<br/>
        /// - <see cref="Exceptions.ExceptionStrategy"/> is set to <see cref="Exceptions.ExceptionStrategy.ThrowAll"/>, and<br/>
        /// - <see cref="LogLevelFlags"/> are set to <see cref="LogLevelFlags.InfoWarnErrorFatal"/>.<br/>
        /// <b>NOTE: </b>You are responsible for disposing of the node when you're done using it!
        /// </summary>
        /// <param name="listenPort">The port to listen for new clients on</param>
        /// <param name="name">The </param>
        /// <returns>A newly minted NetworkNode!</returns>

        public static NetworkNode CreateClient(string ipAddress, int port, string name = "DefaultClient",
            ExceptionStrategy exceptStrat = ExceptionStrategy.ThrowAll,
            LogLevelFlags logLevelFlags = LogLevelFlags.InfoWarnErrorFatal)
        {
            NetworkNodeBuilder builder = new NetworkNodeBuilder(name, Distribution.Client)
                .SetExceptionStrategy(exceptStrat);

            NetworkNode node = builder.Build();
            ClientSocket socket = node.Socket as ClientSocket;
            socket.HostName = ipAddress;
            socket.Port = (ushort) port;
            socket.Notify.SetLogLevel(logLevelFlags);
            return node;
        }

        /// <summary>
        /// Tries to initialize the ENet library if it hasn't been initialized already
        /// </summary>
        /// <param name="callbacks">ENet callbacks to use</param>
        /// <returns>Whether it was initialized</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static bool _Internal_TryInitializeENet(Callbacks callbacks)
        {
            // Did we already initialize?
            if (ENet_Initialized)
                return false;

            ENet_Initialized = callbacks == null ? Library.Initialize() : Library.Initialize(callbacks);

            if (!ENet_Initialized)
                throw new InvalidOperationException("Something went wrong while trying to initialize ENet!");

            return ENet_Initialized;
        }

    }

}
