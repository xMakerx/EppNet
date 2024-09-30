///////////////////////////////////////////////////////
/// Filename: ConnectEvent.cs
/// Date: July 22, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Time;

using System;

namespace EppNet.Connections
{

    public readonly struct ConnectEvent : ITimestamped
    {

        public readonly Connection Connection;

        /// <summary>
        /// Timestamp is the same as <see cref="Network.MonotonicTimestamp"/>
        /// </summary>
        public readonly TimeSpan Time { get; }

        public ConnectEvent(Connection connection)
        {
            this.Connection = connection;
            this.Time = TimeExtensions.MonoTime();
        }

    }

}
