///////////////////////////////////////////////////////
/// Filename: NetworkObjectAttribute.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Attributes
{

    /// <summary>
    /// Denotes a class as a network object which can communicate updates
    /// across the wire. This attribute is NOT inherited and must be added
    /// to the exact class you want distributed.
    /// </summary>

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NetworkObjectAttribute : Attribute
    {

        const string Empty = "";

        /// <summary>
        /// Specify a creator method for generating a new instance of this object.<br/>
        /// This should specify the name of an accessible static method that returns the type<br/>
        /// decorated by this attribute
        /// </summary>
        public string Creator;

        /// <summary>
        /// Specify a destructor method for ensuring the object is cleaned up properly.
        /// </summary>
        public string Destructor;

        /// <summary>
        /// Whether or not instances of this object are global. (Not tied to a particular zone)
        /// </summary>
        public bool Global;

        public Distribution Dist;

        public NetworkObjectAttribute(string creatorMethodName = Empty, 
            string destructorMethodName = Empty, 
            bool isGlobal = false, 
            Distribution dist = Distribution.Shared)
        {
            this.Creator = creatorMethodName;
            this.Destructor = destructorMethodName;
            this.Global = isGlobal;
            this.Dist = dist;
        }

    }

}