///////////////////////////////////////////////////////
/// Filename: ObjectState.cs
/// Date: September 24, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Logging;
using EppNet.Utilities;

using System;
using System.Collections.Generic;

namespace EppNet.Objects
{

    /// <summary>
    /// Represents a "snapshot" of a <see cref="EppNet.Sim.ISimUnit"/> at a particular time in the simulation.<br/>
    /// Each state contains a unique, randomly generated header of <see cref="HeaderLength"/> characters<br/>
    /// Network properties and network methods with implemented getters (usually snapshot methods) are stored here
    /// </summary>

    public class ObjectSnapshot : IEquatable<ObjectSnapshot>, IComparable<ObjectSnapshot>, IComparable
    {

        #region Static Access and Operators

        private static Random _rand = new Random();

        public static bool operator ==(ObjectSnapshot a, ObjectSnapshot b) => a.Equals(b);
        public static bool operator !=(ObjectSnapshot a, ObjectSnapshot b) => !a.Equals(b);

        public static bool operator <(ObjectSnapshot a, ObjectSnapshot b) => a.CompareTo(b) < 0;
        public static bool operator <=(ObjectSnapshot a, ObjectSnapshot b) => a.CompareTo(b) <= 0;

        public static bool operator >(ObjectSnapshot a, ObjectSnapshot b) => a.CompareTo(b) > 0;
        public static bool operator >=(ObjectSnapshot a, ObjectSnapshot b) => a.CompareTo(b) >= 0;

        #endregion

        public const int HeaderLength = 8;

        /// <summary>
        /// The header identifies this state. It's randomly generated [Aa-Zz, 0-9]
        /// </summary>
        public readonly string Header;
        public readonly ObjectAgent Object;
        public readonly ulong Time;

        protected SortedList<string, object> _method2Value;
        protected SortedList<string, object> _prop2Value;

        /// <summary>
        /// Creates and records the current state of the specified <see cref="ObjectAgent"/> at the current time
        /// </summary>
        /// <param name="object"></param>
        public ObjectSnapshot(ObjectAgent @object)
        {
            if (@object == null)
                @object.ObjectManager.Notify.Error("Tried to create a new ObjectState with a NULL ObjectAgent!", 
                    new ArgumentNullException(nameof(@object), "ObjectAgent argument cannot be null!"));

            this.Header = _Internal_GenerateHeader();
            this.Object = @object;
            this.Time = 0L; // TODO: ObjectState: Acquire current time/tick

            this._method2Value = new(ObjectRegistration.StringSortComparer);
            this._prop2Value = new(ObjectRegistration.StringSortComparer);

            this.RecordCurrent();
        }

        /// <summary>
        /// Generates a random header for this state <br/>
        /// Thanks to Dan Rigby: https://stackoverflow.com/a/1344258/26439978
        /// </summary>
        /// <returns>Randomly generated header<br/>
        /// - Example: 6Q4j9CgP
        /// </returns>
        private string _Internal_GenerateHeader()
        {
            string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] chars = new char[HeaderLength];

            for (int i = 0; i < chars.Length; i++)
                chars[i] = validChars[_rand.Next(validChars.Length)];

            return new string(chars);
        }

        /// <summary>
        /// Captures the current value of each network property and snapshot network method
        /// </summary>

        public void RecordCurrent()
        {

            SortedList<string, ObjectMemberDefinition> list = Object.Metadata._methods;
            int index = 0;

            while (index < list.Count)
            {
                string key = list.GetKeyAtIndex(index);
                ObjectMemberDefinition memDef = list[key];

                if (memDef.IsProperty() || (memDef.IsMethod() && memDef.Attribute.Flags.IsFlagSet(Core.NetworkFlags.Snapshot)))
                {

                    object currentValue = memDef.InvokeGetter(Object);

                    if (memDef.IsProperty())
                        _prop2Value.Add(key, currentValue);
                    else
                        _method2Value.Add(key, currentValue);
                }

                // After we're done looking at our method values, let's look at our properties 
                if (++index == list.Count && ReferenceEquals(list, Object.Metadata._methods))
                {
                    list = Object.Metadata._props;
                    index = 0;
                }
            }
        }

        /// <summary>
        /// Checks if our time is equivalent to the time of the provided <see cref="ObjectSnapshot"/>
        /// </summary>
        /// <param name="other"></param>
        /// <returns>True/false</returns>

        public bool Equals(ObjectSnapshot other) => other != null && other.Time == Time;

        /// <summary>
        /// Checks if the other object is a <see cref="ObjectSnapshot"/>, and our time is equivalent.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True/false</returns>

        public override bool Equals(object obj) => obj is ObjectSnapshot objState && Equals(objState);

        /// <summary>
        /// The hash code of our header
        /// </summary>
        /// <returns>Hash code of our header</returns>

        public override int GetHashCode() => Header.GetHashCode();

        /// <summary>
        /// See <see cref="ulong.CompareTo(ulong)"/><br/>
        /// This function is similar but null or undefined comparisons return 1.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Time comparison</returns>

        public int CompareTo(ObjectSnapshot other)
        {
            if (other == null)
                return 1;

            return Time.CompareTo(other.Time);
        }

        /// <summary>
        /// See <see cref="ulong.CompareTo(ulong)"/><br/>
        /// This function is similar but null or undefined comparisons return 1.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            if (obj is ObjectSnapshot x)
                return CompareTo(x);

            Object.ObjectManager.Notify.Error("Invalid comparison!", new ArgumentException("Invalid comparison!", nameof(obj)));
            return 1;
        }
    }

}
