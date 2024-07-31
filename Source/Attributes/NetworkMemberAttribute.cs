///////////////////////////////////////////////////////
/// Filename: NetworkMemberAttribute.cs
/// Date: September 23, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;

using System;

namespace EppNet.Attributes
{
    public abstract class NetworkMemberAttribute : Attribute
    {

        public SlottableEnum Flags { internal set; get; }

        protected NetworkMemberAttribute() : this(NetworkFlags.None) { }

        protected NetworkMemberAttribute(SlottableEnum flags)
        {
            this.Flags = flags;
        }
    }

}
