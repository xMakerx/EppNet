/////////////////////////////////////////////
/// Filename: ObjectDelegate.cs
/// Date: September 14, 2022
/// Author: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Sim;

using System.Collections.Generic;

namespace EppNet.Objects
{

    /// <summary>
    /// To make things as simple as possible for the end user, we abstract the
    /// innerworkings of objects in the system into this class.
    /// </summary>
    /// <typeparam name="T"></typeparam>

    public class ObjectDelegate
    {

        public readonly ISimUnit UserObject;
        protected readonly ObjectRegistration _metadata;

        public ObjectDelegate(ISimUnit user_object)
        {
            this.UserObject = user_object;

        }

    }

}
