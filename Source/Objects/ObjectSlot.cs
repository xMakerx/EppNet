///////////////////////////////////////////////////////
/// Filename: ObjectSlot.cs
/// Date: July 16, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Collections;

using System;

namespace EppNet.Objects
{

    public class ObjectSlot : IPageable, IEquatable<ObjectSlot>
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

        public IPage Page { set; get; }

        /// <summary>
        /// The ID associated with this slot<br/>
        /// If this represents a valid ObjectSlot, the ID would be positive.
        /// </summary>
        public long ID { set; get; }

        /// <summary>
        /// The state associated with the <see cref="ObjectAgent"/> in this slot.
        /// </summary>
        public EnumObjectState State
        {

            internal set
            {
                if (value != _state)
                {
                    if (Agent != null)
                        Agent.State = value;
                    else
                        _state = value;
                }
            }

            get => (Agent != null ? Agent.State : _state);
        }

        /// <summary>
        /// A pointer to the <see cref="ObjectAgent"/> i.e. controller for the user object.
        /// </summary>
        public ObjectAgent Agent;

        private EnumObjectState _state;

        internal bool _TESTS_ForceUsed = false;

        /// <summary>
        /// Instantiates a new default <see cref="ObjectSlot"/> with ID -1, <see cref="EnumObjectState.Unknown"/>,
        /// and a null <see cref="ObjectAgent"/>.
        /// </summary>

        public ObjectSlot()
        {
            this.Page = null;
            this.ID = -1L;
            this._state = EnumObjectState.Unknown;
            this.Agent = null;
        }

        public ObjectSlot(IPage page, long id) : this()
        {
            this.Page = page;
            this.ID = id;
        }

        public void Dispose()
        {
            Agent?.Dispose();
            Agent = null;

            _state = EnumObjectState.Unknown;
        }

        public bool IsFree() => !_TESTS_ForceUsed || (!_TESTS_ForceUsed && Agent == null);

        /// <summary>
        /// Checks if the specified object is a <see cref="ObjectSlot"/> with the same ID
        /// as this one.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>The specified object is an ObjectSlot that shares our ID</returns>

        public override bool Equals(object obj)
        {
            if (obj is ObjectSlot otherObjSlot)
                return otherObjSlot.ID == ID && otherObjSlot.Agent == Agent;

            return false;
        }

        /// <summary>
        /// Checks if the other <see cref="ObjectSlot"/> is in the equivalent slot;<br/>
        /// has an equivalent ID.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Whether or not the provided ObjectSlot has an equivalent ID</returns>
        public bool Equals(ObjectSlot other) => other?.ID == ID && other?.Agent == Agent;

        /// <summary>
        /// Fetches the hash code associated with our ID.
        /// </summary>
        /// <returns>ID#GetHashCode()</returns>

        public override int GetHashCode() => ID.GetHashCode();
    }

}