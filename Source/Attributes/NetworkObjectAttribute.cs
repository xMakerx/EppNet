///////////////////////////////////////////////////////
/// Filename: NetworkObjectAttribute.cs
/// Date: September 14, 2022
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Attributes
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NetworkObjectAttribute : Attribute
    {

        /// <summary>
        /// Specify a creator method for generating a new instance of this object.
        /// </summary>
        public Func<object> Creator;

        /// <summary>
        /// Specify a destructor method for ensuring the object is cleaned up properly.
        /// </summary>
        public Action Destructor;

        public NetworkObjectAttribute()
        {
            this.Creator = null;
            this.Destructor = null;
        }

        public NetworkObjectAttribute(Func<object> creator, Action destructor)
        {
            this.Creator = creator;
            this.Destructor = destructor;
        }

    }

}