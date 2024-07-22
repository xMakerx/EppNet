///////////////////////////////////////////////////////
/// Filename: ConnectEvent.cs
/// Date: July 22, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Time;

namespace EppNet.Connections
{

    public readonly struct ConnectEvent
    {

        public readonly Connection Connection;

        /// <summary>
        /// Timestamp is the same as <see cref="Network.MonotonicTimestamp"/>
        /// </summary>
        public readonly Timestamp Timestamp;

        public ConnectEvent(Connection connection)
        {
            this.Connection = connection;
            this.Timestamp = Timestamp.FromMonoNow();
        }

    }

}
