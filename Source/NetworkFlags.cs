///////////////////////////////////////////////////////
/// Filename: NetworkFlags.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;

using System.Collections.Generic;

namespace EppNet
{

    public static class NetworkFlags
    {

        private static readonly List<SlottableEnum> _flagsList = new();

        /// <summary>
        /// Method calls orchestrated by the server.
        /// </summary>
        public static readonly SlottableEnum None = SlottableEnum._Internal_CreateAndAddTo(_flagsList, "None", 1);

        /// <summary>
        /// Method must be called to generate the object.
        /// </summary>
        public static readonly SlottableEnum Required = SlottableEnum._Internal_CreateAndAddTo(_flagsList, "Required", 2);

        /// <summary>
        /// "If a tree falls in the forest and no one's there to hear it, does it make a sound?"
        /// The answer in this case is no. Only parties interested in the object at the time
        /// of the method call will be informed. Server is solely in charge of updates.
        /// </summary>
        public static readonly SlottableEnum Broadcast = SlottableEnum._Internal_CreateAndAddTo(_flagsList, "Broadcast", 3);

        /// <summary>
        /// Method can be sent by a client.
        /// </summary>
        public static readonly SlottableEnum ClientSend = SlottableEnum._Internal_CreateAndAddTo(_flagsList, "ClientSend", 4);

        /// <summary>
        /// Method can only be sent by the object's owner.
        /// </summary>
        public static readonly SlottableEnum OwnerSend = SlottableEnum._Internal_CreateAndAddTo(_flagsList, "OwnerSend", 4);

        /// <summary>
        /// Method call is stored so it can be propagated to parties
        /// that gain interest in the object after the update.
        /// </summary>
        public static readonly SlottableEnum Persistent = SlottableEnum._Internal_CreateAndAddTo(_flagsList, "Persistent", 5);

        /// <summary>
        /// Data passed to this method is recorded for interpolation and extrapolation between
        /// ticks. In addition, the current value is queried every tick to detect changes and
        /// propagate them.
        /// <br/>Implies <see cref="Persistent"/> as the latest snapshot value is
        /// sent to new parties that gain interest.
        /// </summary>
        public static readonly SlottableEnum Snapshot = SlottableEnum._Internal_CreateAndAddTo(_flagsList, "Snapshot", 5);
    }

}