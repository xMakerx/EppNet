///////////////////////////////////////////////////////
/// Filename: ObjectEvents.cs
/// Date: July 30, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Time;

namespace EppNet.Objects
{

    public readonly struct ObjectCreatedEvent
    {

        public readonly ObjectSlot Slot;
        public readonly Timestamp Timestamp;

        public ObjectCreatedEvent(ObjectService service, ObjectSlot slot)
        {
            this.Slot = slot;
            this.Timestamp = service.Time();
        }

    }

    public readonly struct ObjectDeletedEvent
    {
        public readonly ObjectSlot Slot;
        public readonly Timestamp Timestamp;

        public ObjectDeletedEvent(ObjectService service, ObjectSlot slot)
        {
            this.Slot = slot;
            this.Timestamp = service.Time();
        }
    }

    public readonly struct StateChangedEvent
    {
        public readonly EnumObjectState NewState;
        public readonly EnumObjectState OldState;

        public readonly Timestamp Timestamp;

        public StateChangedEvent(ObjectAgent agent, EnumObjectState newState, EnumObjectState oldState)
        {
            this.NewState = newState;
            this.OldState = oldState;
            this.Timestamp = TimeExtensions.DetermineCurrentTime(agent);
        }

    }

    public readonly struct ParentChangedEvent
    {
        public readonly ObjectAgent NewParent;
        public readonly ObjectAgent OldParent;
        public readonly Timestamp Timestamp;

        public ParentChangedEvent(ObjectAgent newParent, ObjectAgent oldParent)
        {
            this.NewParent = newParent;
            this.OldParent = oldParent;
            this.Timestamp = TimeExtensions.DetermineCurrentTime(newParent, oldParent);
        }
    }

    public readonly struct ChildAddedEvent
    {

        public readonly ObjectAgent Object;
        public readonly Timestamp Timestamp;

        public ChildAddedEvent(ObjectAgent @object)
        {
            this.Object = @object;
            this.Timestamp = TimeExtensions.DetermineCurrentTime(@object);
        }

    }

    public readonly struct ChildRemovedEvent
    {

        public readonly ObjectAgent Object;
        public readonly Timestamp Timestamp;

        public ChildRemovedEvent(ObjectAgent @object)
        {
            this.Object = @object;
            this.Timestamp = TimeExtensions.DetermineCurrentTime(@object);
        }

    }

}
