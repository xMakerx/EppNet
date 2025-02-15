///////////////////////////////////////////////////////
/// Filename: ObjectEvents.cs
/// Date: July 30, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Events;
using EppNet.Time;

using System;

namespace EppNet.Objects
{

    public abstract class ObjectEvent : EventBase<ObjectSlot>
    {

        public ObjectEvent(ObjectSlot slot, ObjectService service) : base(service.Node, slot) { }
    }

    public sealed class ObjectCreatedEvent : ObjectEvent
    {
        public ObjectCreatedEvent(ObjectSlot slot, ObjectService service) : base(slot, service) { }
    }

    public readonly struct ObjectDeletedEvent
    {
        public readonly ObjectSlot Slot;
        public readonly TimeSpan Timestamp;

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

        public readonly TimeSpan Timestamp;

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
        public readonly TimeSpan Timestamp;

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
        public readonly TimeSpan Timestamp;

        public ChildAddedEvent(ObjectAgent @object)
        {
            this.Object = @object;
            this.Timestamp = TimeExtensions.DetermineCurrentTime(@object);
        }

    }

    public readonly struct ChildRemovedEvent
    {

        public readonly ObjectAgent Object;
        public readonly TimeSpan Timestamp;

        public ChildRemovedEvent(ObjectAgent @object)
        {
            this.Object = @object;
            this.Timestamp = TimeExtensions.DetermineCurrentTime(@object);
        }

    }

}
