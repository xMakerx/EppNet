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
            int toSet = 13;
            ulong n = 0L;
            
            for (int i = 0; i < toSet; i++)
            {
                n |= (1UL << i);
            }

            //Console.WriteLine(Convert.ToString((long)n, 2).PadLeft(64, '0'));

            for (int i = 63; i > -1; i--)
            {
                if ((n & (1UL << i)) != 0)
                {
                    Console.Write("1");
                }
                else
                {
                    Console.Write("0");
                }
            }

            Console.WriteLine();
            int front = System.Numerics.BitOperations.LeadingZeroCount(n);
            int firstAvailableIndex = 64 - front;
            Console.WriteLine("First available " + firstAvailableIndex);

            int back = System.Numerics.BitOperations.TrailingZeroCount(n);



            //Console.WriteLine(Convert.ToString((long) n, 2));

            /*
            for (int i = 0; i < result; i++)
            {
                Console.Write(0);
            }

            for (int i = result; i < 64; i++)
            {
                Console.Write(1);
            }

            Console.WriteLine();
            Console.WriteLine(result);*/

            /*
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
            }*/

        }

        public static void Main(string[] args)
        {
            new SocketTest();
        }

    }

}