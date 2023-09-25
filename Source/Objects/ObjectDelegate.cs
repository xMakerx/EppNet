/////////////////////////////////////////////
/// Filename: ObjectDelegate.cs
/// Date: September 14, 2023
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

    public class ObjectDelegate
    {

        public readonly ISimUnit UserObject;
        public readonly ObjectRegistration Metadata;
        public readonly long ID;

        public int TicksUntilDeletion { internal set; get; }

        internal SortedDictionary<ulong, ObjectState> _savedStates;
        internal Queue<Update> _enqueuedUpdates;

        internal ObjectDelegate(ObjectRegistration registration, ISimUnit userObject, long id)
        {
            this.Metadata = registration;
            this.UserObject = userObject;
            this.ID = id;

            this.TicksUntilDeletion = -1;
            this._savedStates = new SortedDictionary<ulong, ObjectState>();
            this._enqueuedUpdates = new Queue<Update>();
        }

        public ObjectState GetStateAt(ulong time)
        {
            _savedStates.TryGetValue(time, out ObjectState state);
            return state;
        }

    }

}
