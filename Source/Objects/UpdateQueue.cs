///////////////////////////////////////////////////////
/// Filename: UpdateQueue.cs
/// Date: September 26, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Utilities;

using System.Collections.Concurrent;

namespace EppNet.Objects
{

    public class UpdateQueue : ConcurrentQueue<Update>
    {

        public readonly bool IsSnapshotQueue;

        public UpdateQueue(bool isSnapshotQueue = false) : base()
        {
            this.IsSnapshotQueue = isSnapshotQueue;
        }

        public bool TryEnqueue(Update item)
        {
            if (item == null)
                return false;

            var netAttr = item.MemberDefinition.Attribute;
            bool snapshotUpdate = netAttr.Flags.IsFlagSet(Core.NetworkFlags.Snapshot);

            // Ensure that the update is valid
            if ((snapshotUpdate && IsSnapshotQueue) || (!snapshotUpdate && !IsSnapshotQueue))
            {
                Enqueue(item);
                return true;
            }

            return false;

        }

    }


}
