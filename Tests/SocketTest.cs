///////////////////////////////////////////////////////
/// Filename: SocketTests.cs
/// Date: SocketTest.cs
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Core;
using EppNet.Logging;
using EppNet.Node;
using EppNet.Sockets;

namespace EppNet.Tests
{

    public class SocketTest
    {

        public SocketTest()
        {
            NetworkNodeBuilder builder = new NetworkNodeBuilder("SocketTest", Distribution.Server)
                .SetExceptionStrategy(Exceptions.ExceptionStrategy.LogOnly);

            NetworkNode node = builder.Build();

            ServerSocket serverSocket = node.Socket as ServerSocket;
            serverSocket.Port = 4296;
            serverSocket.Notify.SetLogLevel(LogLevelFlags.All);

            node.TryStart();
        }

        public static void Main(string[] args)
        {
            new SocketTest();
        }

    }

}