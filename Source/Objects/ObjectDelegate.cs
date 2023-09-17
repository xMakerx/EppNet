/////////////////////////////////////////////
/// Filename: ObjectDelegate.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Sim;

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

        public int TicksUntilDeletion { internal set; get; } = -1;

        public long ID { internal set; get; }

        public ObjectDelegate(ISimUnit userObject)
        {
            this.UserObject = userObject;

        }

    }

}
