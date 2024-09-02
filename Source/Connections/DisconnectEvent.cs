///////////////////////////////////////////////////////
/// Filename: DisconnectEvent.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Connections
{

    public readonly struct DisconnectEvent
    {

        public readonly Connection Connection;
        public readonly DisconnectReason Reason;

        /// <summary>
        /// Timestamp is the same as <see cref="Network.MonotonicTimestamp"/>
        /// </summary>
        public readonly TimeSpan Time;

        public DisconnectEvent(Connection connection, DisconnectReason reason)
        {
            this.Connection = connection;
            this.Reason = reason;
            this.Time = connection.Node.Time;
        }

    }

}
