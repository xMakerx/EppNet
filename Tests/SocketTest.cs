///////////////////////////////////////////////////////
/// Filename: SocketTests.cs
/// Date: SocketTest.cs
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Core;
using EppNet.Data;
using EppNet.Logging;
using EppNet.Node;
using EppNet.Sockets;

using System;
using System.Threading;

namespace EppNet.Tests
{

    public class Thing
    {
        string l = "lol";

        public Thing()
        {
            Console.WriteLine("HELLO!");
        }

        ~Thing() => Console.WriteLine("Goodbye!");
    }

    public class SocketTest
    {

        public SocketTest()
        {
            NetworkNodeBuilder builder = new NetworkNodeBuilder("SocketTest", Distribution.Server)
                .SetExceptionStrategy(Exceptions.ExceptionStrategy.LogOnly);

            NetworkNode node = builder.Build();

            using (node)
            {
                ServerSocket serverSocket = node.Socket as ServerSocket;
                serverSocket.Port = 4296;
                serverSocket.Notify.SetLogLevel(LogLevelFlags.All);

                node.TryStart();

                NetworkNodeManager._Internal_TryUnregisterNode(node);

                node.Set("thing", "3");
                Console.WriteLine("thing: " + node.Get("thing"));
                Console.WriteLine(node.GetOrCreateData());
            }

        }

        public static void Main(string[] args)
        {
            new SocketTest();
        }

    }

}