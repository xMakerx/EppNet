///////////////////////////////////////////////////////
/// Filename: ObjectCreatedEvent.cs
/// Date: July 22, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Time;

namespace EppNet.Objects
{

    public readonly struct ObjectCreatedEvent
    {

        public readonly ObjectSlot Slot;
        public readonly Timestamp Time;

        public ObjectCreatedEvent(ObjectSlot slot)
        {
            this.Slot = slot;
            this.Time = Timestamp.FromMonoNow();
        }

    }

}