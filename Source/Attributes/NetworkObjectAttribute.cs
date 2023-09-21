///////////////////////////////////////////////////////
/// Filename: NetworkObjectAttribute.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Sim;
using EppNet.Utilities;

using Serilog;

using System;

namespace EppNet.Attributes
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NetworkObjectAttribute : Attribute
    {

        static NetworkObjectAttribute()
        {
            AttributeFetcher.AddType<NetworkObjectAttribute>(type =>
            {
                bool isValid = type.IsClass && typeof(ISimUnit).IsAssignableFrom(type);

                if (!isValid)
                    Log.Error($"[{type.Name}] Invalid use of NetworkObjectAttribute. Provided type does not extend ISimUnit!!");

                return isValid;
            });
        }

        public enum Distribution : int
        {
            /// <summary>
            /// Both the server and the client use this class for instances
            /// of the object.
            /// </summary>
            Both = 0,

            /// <summary>
            /// This object is only created on the server.
            /// </summary>
            Server = 1,

            /// <summary>
            /// This object is only created on the client.
            /// </summary>
            Client = 2
        }

        /// <summary>
        /// Specify a creator method for generating a new instance of this object.
        /// </summary>
        public Func<object> Creator;

        /// <summary>
        /// Specify a destructor method for ensuring the object is cleaned up properly.
        /// </summary>
        public Action Destructor;

        /// <summary>
        /// Whether or not instances of this object are global. (Not tied to a particular zone)
        /// </summary>
        public bool Global;

        public Distribution Dist;

        public NetworkObjectAttribute()
        {
            this.Creator = null;
            this.Destructor = null;
            this.Global = false;
            this.Dist = Distribution.Both;
        }

    }

}