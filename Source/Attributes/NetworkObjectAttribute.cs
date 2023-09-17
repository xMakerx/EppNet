﻿///////////////////////////////////////////////////////
/// Filename: NetworkObjectAttribute.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Attributes
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NetworkObjectAttribute : Attribute
    {

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