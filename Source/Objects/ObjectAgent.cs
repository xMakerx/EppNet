/////////////////////////////////////////////
/// Filename: ObjectDelegate.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Collections;
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

                    OnParentChanged?.Invoke(new(value, oldParent));
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
                    OnStateChanged?.Invoke(new StateChangedEvent(this, value, oldState));
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

        public bool ReparentTo(ObjectAgent newParent)
        {
            if (_parent == newParent)
                return false;

            ObjectAgent existingParent = _parent;

            // Let's let the existing parent know we're moving
            if (existingParent?._Internal_RemoveChild(this) == true)
                existingParent.OnChildRemoved?.Invoke(new(this));

            // Parent cannot be set to call add child
            _parent = null;
            if (newParent?._Internal_AddChild(this) == true)
                newParent.OnChildAdded?.Invoke(new(this));

            // Update the parent (internally sends out the event)
            Parent = newParent;
            return true;
        }

        public bool AddChild([NotNull] ObjectAgent child)
        {
            bool added = _Internal_AddChild(child);

            if (added)
            {
                OnChildAdded?.Invoke(new(child));
                child.Parent = this;
            }

            return added;
        }

        public bool RemoveChild([NotNull] ObjectAgent child)
        {
            bool removed = _Internal_RemoveChild(child);

            if (removed)
            {
                OnChildRemoved?.Invoke(new(child));
                child.Parent = null;
            }

            return removed;
        }

        public bool HasChild(ObjectAgent child) => child != null && _children?.Contains(child) == true;

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

        public bool IsOtherValid(ObjectAgent other) => other != null && other != this;

        public override bool Equals(object obj)
        {
            if (obj is ObjectAgent other)
                return Equals(other);

            return false;
        }

        public bool Equals(ObjectAgent other) => Metadata == other.Metadata && UserObject == other.UserObject && ID == other.ID;

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

        protected bool _Internal_AddChild([NotNull] ObjectAgent child)
        {
            if (!this.IsNotNull(child, message: "Cannot add a null child!"))
                return false;

            if (child._parent != null)
                // Child already has a parent
                return false;

            if (_children != null && _children.Contains(child))
                // Already have this child
                return false;

            if (_children == null)
                _children = new();

            _children.Add(child);
            Notify.Debug(new TemplatedMessage("Added child Object ID {id}", child.ID));
            return true;
        }

        protected bool _Internal_RemoveChild([NotNull] ObjectAgent child)
        {
            if (!this.IsNotNull(child, message: "Cannot remove a null child!"))
                return false;

            if (_children == null)
                // I don't have any kids. Not paying child support.
                return false;

            bool removed = _children.Remove(child);

            if (removed)
                Notify.Debug(new TemplatedMessage("Removed child Object ID {id}", child.ID));

            return removed;
        }
    }

}
