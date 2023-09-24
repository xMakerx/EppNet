///////////////////////////////////////////////////////
/// Filename: NetworkMemberAttribute.cs
/// Date: September 23, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Core;

using System;

namespace EppNet.Attributes
{
    public abstract class NetworkMemberAttribute : Attribute
    {

        public NetworkFlags Flags { internal set; get; }

        public NetworkMemberAttribute() : this(NetworkFlags.None) { }

        public NetworkMemberAttribute(NetworkFlags flags)
        {
            this.Flags = flags;
        }
    }

}
