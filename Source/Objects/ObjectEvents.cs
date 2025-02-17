///////////////////////////////////////////////////////
/// Filename: ObjectEvents.cs
/// Date: July 30, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Events;
using EppNet.Node;

namespace EppNet.Objects
{

    public abstract class ObjectEvent : EventBase<ObjectSlot>
    {

        public ObjectEvent(ObjectSlot slot, ObjectService service) : base(service.Node, slot, service) { }
    }

    public sealed class ObjectCreatedEvent : ObjectEvent
    {
        public ObjectCreatedEvent(ObjectSlot slot, ObjectService service) : base(slot, service) { }
    }

    public sealed class ObjectDeletedEvent : ObjectEvent
    {
        public ObjectDeletedEvent(ObjectSlot slot, ObjectService service) : base(slot, service) { }
    }

    public sealed class StateChangedEvent : EventBase<(EnumObjectState, EnumObjectState)>
    {
        public EnumObjectState State { get => Subject.Item1; }
        public EnumObjectState OldState { get => Subject.Item2; }

        public StateChangedEvent(INodeDescendant nDesc, EnumObjectState newState, EnumObjectState oldState, object sender) :
            base(nDesc.Node, (newState, oldState), sender)
        { }

        public StateChangedEvent(INodeDescendant nDesc, EnumObjectState newState, EnumObjectState oldState) :
            base(nDesc.Node, (newState, oldState), null)
        { }
    }

    public sealed class ParentChangedEvent : EventBase<(INetworkObject_Impl, INetworkObject_Impl)>
    {
        public INetworkObject_Impl Parent { get => Subject.Item1; }
        public INetworkObject_Impl OldParent { get => Subject.Item2; }

        public ParentChangedEvent(INodeDescendant nDesc, INetworkObject_Impl newParent, INetworkObject_Impl oldParent, object sender) :
            base(nDesc.Node, (newParent, oldParent), sender)
        { }
    }

    public sealed class ChildAddedEvent : EventBase<INetworkObject_Impl>
    {

        public INetworkObject_Impl Child { get => Subject; }

        public ChildAddedEvent(INodeDescendant nDesc, INetworkObject_Impl child, object sender) :
            base(nDesc.Node, child, sender)
        { }
    }

    public sealed class ChildRemovedEvent : EventBase<INetworkObject_Impl>
    {

        public INetworkObject_Impl Child { get => Subject; }

        public ChildRemovedEvent(INodeDescendant nDesc, INetworkObject_Impl child, object sender) :
            base(nDesc.Node, child, sender)
        { }
    }

}
