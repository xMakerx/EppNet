///////////////////////////////////////////////////////
/// Filename: DisconnectEvent.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Core;

namespace EppNet.Connections
{

    public class DisconnectEvent
    {

        public readonly Connection Connection;
        public readonly DisconnectReason Reason;

        /// <summary>
        /// Timestamp is the same as <see cref="Network.MonotonicTimestamp"/>
        /// </summary>
        public readonly Timestamp Timestamp;

        public DisconnectEvent(Connection connection, DisconnectReason reason)
        {
            this.Connection = connection;
            this.Reason = reason;
            this.Timestamp = Timestamp.FromMonoNow();
        }

    }

}
