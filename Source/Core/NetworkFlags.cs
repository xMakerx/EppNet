///////////////////////////////////////////////////////
/// Filename: NetworkFlags.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Core
{

    public enum NetworkFlags : byte
    {
        /// <summary>
        /// Method calls orchestrated by the server.
        /// </summary>
        None            = 0,

        /// <summary>
        /// Method must be called to generate the object.
        /// </summary>
        Required        = 1 << 0,

        /// <summary>
        /// "If a tree falls in the forest and no one's there to hear it, does it make a sound?"
        /// The answer in this case is no. Only parties interested in the object at the time
        /// of the method call will be informed. Server is solely in charge of updates.
        /// </summary>
        Broadcast       = 1 << 1,

        /// <summary>
        /// Method can be sent by a client.
        /// </summary>
        ClientSend      = 1 << 2,

        /// <summary>
        /// Method can only be sent by the object's owner.
        /// </summary>
        OwnerSend       = 1 << 3,

        /// <summary>
        /// Method call is stored so it can be propagated to parties
        /// that gain interest in the object after the update.
        /// </summary>
        Persistant      = 1 << 4,

        /// <summary>
        /// Data passed to this method is recorded for interpolation and extrapolation between
        /// ticks. In addition, the current value is queried every tick to detect changes and
        /// propagate them.
        /// <br/>Implies <see cref="Persistant"/> as the latest snapshot value is
        /// sent to new parties that gain interest.
        /// </summary>
        Snapshot        = 1 << 5,

    }

}