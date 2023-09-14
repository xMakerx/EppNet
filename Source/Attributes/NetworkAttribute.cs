///////////////////////////////////////////////////////
/// Filename: NetworkAttribute.cs
/// Date: September 14, 2022
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Core;

using System;

namespace EppNet.Attributes
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NetworkAttribute : Attribute
    {

        public NetworkFlags Flags { internal set; get; }

        public NetworkAttribute() : this(NetworkFlags.None) { }

        public NetworkAttribute(NetworkFlags flags)
        {
            this.Flags = flags;
        }
    }

}
