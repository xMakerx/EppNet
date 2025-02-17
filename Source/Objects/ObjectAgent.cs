/////////////////////////////////////////////
/// Filename: ObjectDelegate.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Collections;
using EppNet.Commands;
using EppNet.Logging;
using EppNet.Node;
using EppNet.Sim;
using EppNet.Utilities;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EppNet.Objects
{

    /// <summary>
    /// To make things as simple as possible for the end user, we abstract the
    /// innerworkings of objects in the system into this class.
    /// </summary>

    public class ObjectAgent : IEquatable<ObjectAgent>, ILoggable, INodeDescendant, IDisposable
    {

        public ILoggable Notify { get => this; }

        public readonly ObjectService Service;
        public readonly ISimUnit UserObject;
        public readonly ObjectRegistration Metadata;
        public readonly long ID;

        /// <summary>
        /// Events
        /// </summary>

        public Action<StateChangedEvent> OnStateChanged;

        // Parent-child relationship events
        public Action<ParentChangedEvent> OnParentChanged;
        public Action<ChildAddedEvent> OnChildAdded;
        public Action<ChildRemovedEvent> OnChildRemoved;

        public NetworkNode Node { get => Service.Node; }
        public ObjectAgent Parent
        {

            internal set
            {

                if (value != _parent)
                {
                    var oldParent = _parent;
                    _parent = value;

                    if (oldParent != null)
                        oldParent.OnStateChanged -= _Internal_OnParentStateChanged;

                    if (_parent != null)
                        _parent.OnStateChanged += _Internal_OnParentStateChanged;

                    OnParentChanged?.GlobalInvoke(new(value, oldParent));
                }

            }

            get => _parent;

        }

        public EnumObjectState State
        {
            internal set
            {
                if (value != _state)
                {
                    var oldState = _state;
                    _state = value;
                    OnStateChanged?.GlobalInvoke(new StateChangedEvent(this, value, oldState));
                }
            }

            get => _state;
        }

        public int TicksUntilDeletion { internal set; get; }

        public UpdateQueue OutgoingReliableUpdates { protected set; get; }
        public UpdateQueue OutgoingSnapshotUpdates { protected set; get; }

        internal ObjectAgent _parent;
        internal List<ObjectAgent> _children;
        internal EnumObjectState _state;

        internal ObjectAgent([NotNull] ObjectService objService, [NotNull] ObjectRegistration registration, [NotNull] ISimUnit userObject, long id)
        {
            Guard.AgainstNull(objService, registration, userObject);

            this.Service = objService;
            this.Metadata = registration;
            this.UserObject = userObject;
            this.UserObject.ID = id;

            this.ID = id;

            this._parent = null;
            this._children = null;

            this.TicksUntilDeletion = -1;
            this.OutgoingReliableUpdates = new UpdateQueue();
            this.OutgoingSnapshotUpdates = new UpdateQueue(isSnapshotQueue: true);
        }

        public EnumCommandResult ReparentTo(long id)
        {
            if (ID == id)
                return EnumCommandResult.BadArgument;

            if (id == -1)
                return ReparentTo(null);

            else if (Service.TryGetById(id, out ObjectSlot parentSlot))
                return ReparentTo(parentSlot.Object);

            return EnumCommandResult.NotFound;
        }

        public EnumCommandResult ReparentTo(ObjectAgent newParent)
        {

            // Ensure we aren't trying to reparent to ourself
            if (_parent == newParent)
                return EnumCommandResult.BadArgument;

            // Ensure we can update our parent
            if (State > EnumObjectState.Generated)
                return EnumCommandResult.InvalidState;

            // Ensure the new parent is capable of receiving a new child
            if (newParent != null && newParent.State > EnumObjectState.Generated)
                return EnumCommandResult.InvalidState;

            ObjectAgent existingParent = _parent;

            // Let's let the existing parent know we're moving
            if (EnumCommandResultExtensions.IsOk(existingParent?._Internal_RemoveChild(this)))
                existingParent.OnChildRemoved?.GlobalInvoke(new(this));

            // Parent cannot be set to call add child
            _parent = null;
            if (EnumCommandResultExtensions.IsOk(newParent?._Internal_AddChild(this)))
                newParent.OnChildAdded?.GlobalInvoke(new(this));

            Parent = newParent;
            return EnumCommandResult.Ok;
        }

        public bool AddChild([NotNull] ObjectAgent child)
        {
            bool added = _Internal_AddChild(child).IsOk();

            if (added)
            {
                OnChildAdded?.GlobalInvoke(new(child));
                child.Parent = this;
            }

            return added;
        }

        public bool RemoveChild([NotNull] ObjectAgent child)
        {
            bool removed = _Internal_RemoveChild(child).IsOk();

            if (removed)
            {
                OnChildRemoved?.GlobalInvoke(new(child));
                child.Parent = null;
            }

            return removed;
        }

        public bool HasChild(ObjectAgent child)
            => child != null && _children?.Contains(child) == true;

        public void ClearAllChildren()
        {
            if (_children == null)
                return;

            Iterator<ObjectAgent> iterator = _children.Iterator();

            while (iterator.HasNext())
                RemoveChild(iterator.Next());
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

        public bool IsOtherValid(ObjectAgent other)
            => other != null && other != this;

        public override bool Equals(object obj)
        {
            if (obj is ObjectAgent other)
                return Equals(other);

            return false;
        }

        public bool Equals(ObjectAgent other)
            => Metadata == other.Metadata &&
            UserObject == other.UserObject
            && ID == other.ID;

        public override int GetHashCode()
        {
            int hashCode = Metadata.GetHashCode();
            hashCode ^= UserObject.GetHashCode();
            hashCode ^= ID.GetHashCode();
            return hashCode;
        }

        public void Dispose()
        {
            Parent?.RemoveChild(this);
            ClearAllChildren();
            _children = null;
        }

        protected void _Internal_OnParentStateChanged(StateChangedEvent @event)
        {
            this.State = @event.NewState;
        }

        protected EnumCommandResult _Internal_AddChild([NotNull] ObjectAgent child)
        {
            if (!this.IsNotNull(child, message: "Cannot add a null child!"))
                return EnumCommandResult.BadArgument;

            const EnumCommandResult result = EnumCommandResult.InvalidState;

            if (State > EnumObjectState.Generated)
                return result;

            if (child.State > EnumObjectState.Generated)
                return result;

            if (child._parent != null)
                return result;

            if (_children != null && _children.Contains(child))
                return result;

            if (_children == null)
                _children = new();

            _children.Add(child);
            Notify.Debug(new TemplatedMessage("Added child Object ID {id}", child.ID));
            return EnumCommandResult.Ok;
        }

        protected EnumCommandResult _Internal_RemoveChild([NotNull] ObjectAgent child)
        {
            if (!this.IsNotNull(child, message: "Cannot remove a null child!"))
                return EnumCommandResult.BadArgument;

            if (_children == null)
                // I don't have any kids. Not paying child support.
                return EnumCommandResult.InvalidState;

            bool removed = _children.Remove(child);

            if (removed)
                Notify.Debug(new TemplatedMessage("Removed child Object ID {id}", child.ID));

            return removed ? EnumCommandResult.Ok : EnumCommandResult.NotFound;
        }
    }

}
