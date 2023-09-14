///////////////////////////////////////////////////////
/// Filename: NetworkFlags.cs
/// Date: September 14, 2022
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Core
{

    public enum NetworkFlags : byte
    {
        /// <summary>
        /// "If a tree falls in the forest and no one's there to hear it, does it make a sound?"
        /// The answer in this case is no. Only parties interested in the object at the time
        /// of the method call will be informed. Server is solely in charge of updates.
        /// </summary>
        None                = 0,

        /// <summary>
        /// Method must be called to generate the object.
        /// </summary>
        Required            = 1 << 0,

        /// <summary>
        /// Method will be propagated to other interested parties.
        /// </summary>
        Broadcast           = 1 << 1,

        /// <summary>
        /// Method can be sent by a client.
        /// </summary>
        ClientSend          = 1 << 2,

        /// <summary>
        /// Method can only be sent by the object's owner.
        /// </summary>
        OwnerSend           = 1 << 3,

        /// <summary>
        /// Method call is stored so it can be propagated to parties
        /// that gain interest in the object after the update.
        /// </summary>
        Persistant          = 1 << 4

    }

}
