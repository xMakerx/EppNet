/////////////////////////////////////////////
/// Filename: MalformedMessageReceivedException.cs
/// Date: July 12, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Connections;

using System;

namespace EppNet.Source.Messaging
{
    public sealed class MalformedMessageReceivedException : Exception
    {

        public const string MalformedMessageFormat = "Received malformed message from ";

        public static MalformedMessageReceivedException New(Connection connection, byte[] bytes)
        {
            string message = MalformedMessageFormat + connection;
            return new(connection, bytes, message);
        }

        public readonly Connection Connection;
        public readonly byte[] Bytes;

        private MalformedMessageReceivedException(Connection connection, byte[] bytes, string message) : base(message)
        {
            this.Connection = connection;
            this.Bytes = bytes;
        }


    }

}
