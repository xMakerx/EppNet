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

    public class ObjectDelegate<T> where T : ISimUnit
    {

        public readonly T UserObject;
        public Dictionary<>

        public ObjectDelegate(T user_object)
        {
            this.UserObject = user_object;

        }

    }

}
