///////////////////////////////////////////////////////
/// Filename: UpdateQueue.cs
/// Date: September 26, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Utilities;

using System.Collections.Generic;

namespace EppNet.Objects
{

    public class UpdateQueue
    {

        public readonly bool IsSnapshotQueue;

        protected Queue<Update> _queue;
        protected object _lock;

        public UpdateQueue(bool isSnapshotQueue = false)
        {
            this.IsSnapshotQueue = isSnapshotQueue;
            this._queue = new Queue<Update>();
            this._lock = new object();
        }

        public bool TryEnqueue(Update item)
        {
            if (item == null)
                return false;

            lock (_lock)
            {

                var netAttr = item.MemberDefinition.Attribute;
                bool snapshotUpdate = netAttr.Flags.IsFlagSet(Core.NetworkFlags.Snapshot);

                if ((snapshotUpdate && IsSnapshotQueue) || (!snapshotUpdate && !IsSnapshotQueue))
                {
                    if (_queue.Contains(item))
                        return false;

                    _queue.Enqueue(item);
                    return true;
                }

                return false;
            }

        }

        public List<Update> FlushQueue()
        {
            lock (_lock)
            {
                List<Update> updates = new List<Update>();
                
                for (int i = 0; i < _queue.Count; i++)
                    updates.Add(_queue.Dequeue());

                return updates;
            }
        }

    }


}
