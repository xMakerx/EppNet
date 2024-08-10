///////////////////////////////////////////////////////
/// Filename: DisconnectEvent.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Time;

namespace EppNet.Connections
{

    public readonly struct DisconnectEvent(Connection connection, DisconnectReason reason)
    {

        public readonly Connection Connection = connection;
        public readonly DisconnectReason Reason = reason;

        /// <summary>
        /// Timestamp is the same as <see cref="Network.MonotonicTimestamp"/>
        /// </summary>
        public readonly Timestamp Timestamp = Timestamp.FromMonoNow();
    }

}
