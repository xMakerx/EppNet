///////////////////////////////////////////////////////
/// Filename: NetworkMethodAttribute.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Core;

using System;
using System.Reflection;

namespace EppNet.Attributes
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NetworkMethodAttribute : NetworkMemberAttribute
    {

        /// <summary>
        /// <see cref="NetworkFlags.Snapshot"/> requires a getter method to
        /// retrieve the current value.
        /// </summary>
        public MethodInfo Getter { internal set; get; }

        public NetworkMethodAttribute() : this(NetworkFlags.None) { }

        public NetworkMethodAttribute(NetworkFlags flags)
        {
            this.Flags = flags;
            this.Getter = null;
        }
    }

}
