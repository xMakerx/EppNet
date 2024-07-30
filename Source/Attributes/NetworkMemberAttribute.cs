///////////////////////////////////////////////////////
/// Filename: NetworkMemberAttribute.cs
/// Date: September 23, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Attributes
{
    public abstract class NetworkMemberAttribute : Attribute
    {

        public NetworkFlags Flags { internal set; get; }

        protected NetworkMemberAttribute() : this(NetworkFlags.None) { }

        protected NetworkMemberAttribute(NetworkFlags flags)
        {
            this.Flags = flags;
        }
    }

}
