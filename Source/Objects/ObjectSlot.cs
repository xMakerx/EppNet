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

    public class ObjectSlot : Pageable, IEquatable<ObjectSlot>, ICommandTarget
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

        public static bool operator ==(ObjectSlot left, ObjectSlot right) => left?.Equals(right) == true;
        public static bool operator !=(ObjectSlot left, ObjectSlot right) => left?.Equals(right) != true;

        #endregion

        /// <summary>
        /// The state associated with the <see cref="ObjectAgent"/> in this slot.
        /// </summary>
        public EnumObjectState State
        {

            internal set
            {
                if (value != _state)
                {
                    if (Object != null)
                        Object.State.Set(value);
                    else
                        _state = value;
                }
            }

            get => (Object != null ? Object.State.Value : _state);
        }

        /// <summary>
        /// A pointer to the <see cref="ObjectAgent"/> i.e. controller for the user object.
        /// </summary>
        public INetworkObject_Impl Object;

        private EnumObjectState _state;

        /// <summary>
        /// Instantiates a new default <see cref="ObjectSlot"/> with ID -1, <see cref="EnumObjectState.Unknown"/>,
        /// and a null <see cref="ObjectAgent"/>.
        /// </summary>

        public ObjectSlot()
        {
            this.Page = null;
            this.ID = -1L;
            this._state = EnumObjectState.Unknown;
            this.Object = null;
        }

        public override void Dispose()
        {
            if (Object is not null && Object is IDisposable disposable)
                disposable.Dispose();
            
            Object = null;

            _state = EnumObjectState.Unknown;
            base.Dispose();
        }

        /// <summary>
        /// Checks if the specified object is a <see cref="ObjectSlot"/> with the same ID
        /// as this one.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>The specified object is an ObjectSlot that shares our ID</returns>

        public override bool Equals(object obj)
        {
            if (obj is ObjectSlot otherObjSlot)
                return otherObjSlot.ID == ID && otherObjSlot.Object == Object;

            return false;
        }

        /// <summary>
        /// Checks if the other <see cref="ObjectSlot"/> is in the equivalent slot;<br/>
        /// has an equivalent ID.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Whether or not the provided ObjectSlot has an equivalent ID</returns>
        public bool Equals(ObjectSlot other) => other?.ID == ID && other?.Object == Object;

        /// <summary>
        /// Fetches the hash code associated with our ID.
        /// </summary>
        /// <returns>ID#GetHashCode()</returns>

        public override int GetHashCode() => ID.GetHashCode();
    }

}