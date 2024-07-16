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

    public class ObjectAgent
    {

        public readonly ISimUnit UserObject;
        public readonly ObjectRegistration Metadata;
        public readonly long ID;

        public int TicksUntilDeletion { internal set; get; }

        internal SortedDictionary<ulong, ObjectState> _savedStates;

        public UpdateQueue OutgoingReliableUpdates { protected set; get; }
        public UpdateQueue OutgoingSnapshotUpdates { protected set; get; }

        internal ObjectAgent(ObjectRegistration registration, ISimUnit userObject, long id)
        {
            this.Metadata = registration;
            this.UserObject = userObject;
            this.ID = id;

            this.TicksUntilDeletion = -1;
            this._savedStates = new SortedDictionary<ulong, ObjectState>();
            this.OutgoingReliableUpdates = new UpdateQueue();
            this.OutgoingSnapshotUpdates = new UpdateQueue(isSnapshotQueue: true);
        }

        public bool EnqueueOutgoing(Update update)
        {
            if (update == null)
                return false;

            bool addedToReliable = OutgoingReliableUpdates.TryEnqueue(update);

            if (!addedToReliable)
                return OutgoingSnapshotUpdates.TryEnqueue(update);

            return addedToReliable;
        }

        public ObjectState GetStateAt(ulong time)
        {
            _savedStates.TryGetValue(time, out ObjectState state);
            return state;
        }

        /// <summary>
        /// Checks that the specified <see cref="ObjectAgent"/> isn't null AND
        /// <br/>isn't equivalent to the calling instance.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Whether or not the other instance is valid</returns>

        public bool IsOtherValid(ObjectAgent other) => other != null && other != this;

    }

}
