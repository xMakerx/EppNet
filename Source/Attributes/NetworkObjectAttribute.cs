///////////////////////////////////////////////////////
/// Filename: NetworkObjectAttribute.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Core;
using EppNet.Sim;

using System;

namespace EppNet.Attributes
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NetworkObjectAttribute : Attribute
    {

        /// <summary>
        /// Specify a creator method for generating a new instance of this object.
        /// </summary>
        public Func<ISimUnit> Creator;

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
            this.Dist = Distribution.Shared;
        }

    }

}