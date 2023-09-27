///////////////////////////////////////////////////////
/// Filename: UpdateQueue.cs
/// Date: September 26, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;
using EppNet.Utilities;

namespace EppNet.Objects
{

    public class UpdateQueue : LockQueue<Update>
    {

        public readonly bool IsSnapshotQueue;

        public UpdateQueue(bool isSnapshotQueue = false)
        {
            this.IsSnapshotQueue = isSnapshotQueue;
        }

        public override bool TryEnqueue(Update item)
        {
            if (item == null)
                return false;

            lock (_lock)
            {

                var netAttr = item.MemberDefinition.Attribute;
                bool snapshotUpdate = netAttr.Flags.IsFlagSet(Core.NetworkFlags.Snapshot);

                if ((snapshotUpdate && IsSnapshotQueue) || (!snapshotUpdate && !IsSnapshotQueue))
                {
                    if (Contains(item))
                        return false;

                    Enqueue(item);
                    return true;
                }

                return false;
            }

        }

    }


}
