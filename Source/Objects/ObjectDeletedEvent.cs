///////////////////////////////////////////////////////
/// Filename: ObjectDeletedEvent.cs
/// Date: July 22, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Time;

namespace EppNet.Objects
{

    public readonly struct ObjectDeletedEvent
    {

        public readonly ObjectSlot Slot;
        public readonly Timestamp Time;

        public ObjectDeletedEvent(ObjectSlot slot)
        {
            this.Slot = slot;
            this.Time = Timestamp.FromMonoNow();
        }

    }

}
