///////////////////////////////////////////////////////
/// Filename: ConnectEvent.cs
/// Date: July 22, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;
using EppNet.Time;

namespace EppNet.Connections
{

    public readonly struct ConnectEvent : ITimestamped
    {

        public readonly Connection Connection;
        public readonly Timestamp Timestamp { get; }

        public ConnectEvent(Connection connection)
        {
            this.Connection = connection;
            this.Timestamp = new(connection.Time());
        }

    }

}
