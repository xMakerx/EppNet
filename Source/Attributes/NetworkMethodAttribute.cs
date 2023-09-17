///////////////////////////////////////////////////////
/// Filename: NetworkMethodAttribute.cs
/// Date: September 14, 2022
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Core;

using System;

namespace EppNet.Attributes
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NetworkMethodAttribute : Attribute
    {

        public NetworkFlags Flags { internal set; get; }

        /// <summary>
        /// <see cref="NetworkFlags.Snapshot"/> requires a getter method to
        /// retrieve the current value.
        /// </summary>
        public Action<object> Getter { internal set; get; }

        public NetworkMethodAttribute() : this(NetworkFlags.None) { }

        public NetworkMethodAttribute(NetworkFlags flags)
        {
            this.Flags = flags;
            this.Getter = null;
        }
    }

}
