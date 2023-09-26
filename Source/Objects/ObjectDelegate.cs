/////////////////////////////////////////////
/// Filename: ObjectDelegate.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Sim;

using System.Collections.Generic;
using System.Runtime.InteropServices;

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
        protected Queue<Update> _enqueuedUpdates;

        protected object _queueLock;

        internal ObjectDelegate(ObjectRegistration registration, ISimUnit userObject, long id)
        {
            this.Metadata = registration;
            this.UserObject = userObject;
            this.ID = id;

            this.TicksUntilDeletion = -1;
            this._savedStates = new SortedDictionary<ulong, ObjectState>();
            this._enqueuedUpdates = new Queue<Update>();
            this._queueLock = new object();
        }

        public bool EnqueueUpdate(Update update)
        {
            if (update == null)
                return false;

            lock (_queueLock)
            {
                if (_enqueuedUpdates.Contains(update))
                    return false;

                _enqueuedUpdates.Enqueue(update);
                return true;
            }

        }

        public List<Update> GetAndClearUpdateQueue()
        {
            lock (_queueLock)
            {
                List<Update> updates = new List<Update>();
                int queued = _enqueuedUpdates.Count;

                for (int i = 0; i < queued; i++)
                    updates.Add(_enqueuedUpdates.Dequeue());

                return updates;
            }
        }

        public ObjectState GetStateAt(ulong time)
        {
            _savedStates.TryGetValue(time, out ObjectState state);
            return state;
        }

    }

}
