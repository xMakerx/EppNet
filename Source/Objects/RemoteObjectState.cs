/////////////////////////////////////////////
/// Filename: RemoteObjectState.cs
/// Date: September 27, 2023
/// Author: Maverick Liberty
//////////////////////////////////////////////

namespace EppNet.Objects
{

    public enum RemoteObjectState : byte
    {
        /// <summary>
        /// Remote client has no idea what this object is
        /// </summary>
        Unknown                 = 0,

        /// <summary>
        /// Remote client is awaiting the most up-to-date state
        /// of the object.
        /// </summary>
        WaitingForState         = 1 << 0,

        /// <summary>
        /// Remote client has generated the object.
        /// </summary>
        Generated               = 1 << 1,

        /// <summary>
        /// Remote client has disabled the object.
        /// </summary>
        Disabled                = 1 << 2,

        /// <summary>
        /// Remote client is planning on deleting the object
        /// Datagrams should send the time (in ticks) until deletion
        /// </summary>
        PendingDelete           = 1 << 3,

        /// <summary>
        /// Remote client has deleted the object.
        /// </summary>
        Deleted                 = 1 << 4
    }

}
