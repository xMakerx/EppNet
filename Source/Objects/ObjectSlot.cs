///////////////////////////////////////////////////////
/// Filename: ObjectSlot.cs
/// Date: July 16, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Collections;
using EppNet.Commands;

using System;

namespace EppNet.Objects
{

    public struct ObjectSlot : IPageable, IEquatable<ObjectSlot>, ICommandTarget, IDisposable
    {
        #region Operators

        /// <summary>
        /// Implicit operator that fetches the ID associated with this slot.
        /// </summary>
        /// <param name="objectSlot"></param>

        public static implicit operator long(ObjectSlot objectSlot) => objectSlot.ID;

        /// <summary>
        /// Explicit operator that creates a new <see cref="ObjectSlot"/> from a positive long<br/>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">ID must be positive</exception>
        /// <param name="id"></param>

        public static explicit operator ObjectSlot(long id)
        {
            if (id < 0)
                throw new ArgumentOutOfRangeException(nameof(id), "ID must be positive!");

            return new()
            {
                ID = id
            };
        }

        public static bool operator ==(ObjectSlot left, ObjectSlot right)
            => left.Equals(right);
        public static bool operator !=(ObjectSlot left, ObjectSlot right)
            => !left.Equals(right);

        #endregion

        public long ID { set; get; }

        public IPage Page { set; get; }

        public bool Allocated { set; get; }

        /// <summary>
        /// The state associated with the <see cref="ObjectAgent"/> in this slot.
        /// </summary>
        public readonly EnumObjectState State
        {
            get => Object != null ? Object.State.Value : EnumObjectState.Unknown;
        }

        /// <summary>
        /// A pointer to the <see cref="ObjectAgent"/> i.e. controller for the user object.
        /// </summary>
        public INetworkObject_Impl Object;

        public ObjectSlot(IPage page, long id, INetworkObject_Impl @object)
        {
            this.Page = page;
            this.ID = id;
            this.Object = @object;
            this.Allocated = false;
        }

        public void Dispose()
        {
            if (Object is not null && Object is IDisposable disposable)
                disposable.Dispose();
            
            Object = null;
        }

        /// <summary>
        /// Checks if the specified object is a <see cref="ObjectSlot"/> with the same ID
        /// as this one.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>The specified object is an ObjectSlot that shares our ID</returns>

        public readonly override bool Equals(object obj)
            => obj is ObjectSlot slot &&
            Equals(slot);

        /// <summary>
        /// Checks if the other <see cref="ObjectSlot"/> is in the equivalent slot;<br/>
        /// has an equivalent ID.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Whether or not the provided ObjectSlot has an equivalent ID</returns>
        public readonly bool Equals(ObjectSlot other)
            => other.ID == ID &&
            other.Object == Object;

        /// <summary>
        /// Fetches the hash code associated with our ID.
        /// </summary>
        /// <returns>ID#GetHashCode()</returns>

        public readonly override int GetHashCode()
            => ID.GetHashCode() ^ 
            (Object != null ? Object.GetHashCode() : 1);
    }

}