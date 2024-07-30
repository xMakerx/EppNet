/////////////////////////////////////////////
/// Filename: ObjectDelegate.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Collections;
using EppNet.Sim;

using System;
using System.Diagnostics.CodeAnalysis;

namespace EppNet.Objects
{

    /// <summary>
    /// To make things as simple as possible for the end user, we abstract the
    /// innerworkings of objects in the system into this class.
    /// </summary>

    public class ObjectAgent : OrderedDictionary<long, ObjectAgent>, IEquatable<ObjectAgent>
    {

        public readonly ObjectService Service;
        public readonly ISimUnit UserObject;
        public readonly ObjectSlot Slot;

        public readonly ObjectRegistration Metadata;

        public readonly long ID;

        public int TicksUntilDeletion { internal set; get; }

        public UpdateQueue OutgoingReliableUpdates { protected set; get; }
        public UpdateQueue OutgoingSnapshotUpdates { protected set; get; }

        internal ObjectAgent([NotNull] ObjectService objService, [NotNull] ObjectRegistration registration, [NotNull] ISimUnit userObject, long id)
        {
            object[] validate = new object[3] { objService, registration, userObject };
            foreach (object o in validate)
            {
                if (o == null)
                {
                    var exception = new ArgumentNullException(nameof(o));

                    if (objService != null)
                        objService.Node.HandleException(exception);
                    else
                        throw exception;
                }
            }

            this.Service = objService;
            this.Metadata = registration;
            this.UserObject = userObject;
            this.ID = id;

            this.TicksUntilDeletion = -1;
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

        /// <summary>
        /// Checks that the specified <see cref="ObjectAgent"/> isn't null AND
        /// <br/>isn't equivalent to the calling instance.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Whether or not the other instance is valid</returns>

        public bool IsOtherValid(ObjectAgent other) => other != null && other != this;

        public bool Equals(ObjectAgent other) => UserObject == other.UserObject && ID == other.ID;
    }

}
