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

    public class ObjectState : IEquatable<ObjectState>
    {

        public const int HeaderLength = 8;
        private static Random _rand = new Random();

        /// <summary>
        /// The header identifies this state. It's randomly generated [Aa-Zz, 0-9]
        /// </summary>
        public readonly string Header;
        public readonly ObjectAgent Object;
        public readonly ulong Time;

        protected SortedList<string, object> _method2Value;
        protected SortedList<string, object> _prop2Value;


        /// <summary>
        /// Captures the current state of this object and stores it
        /// </summary>
        /// <param name="object"></param>
        public ObjectState(ObjectAgent @object)
        {
            if (@object == null)
                @object.ObjectManager.Notify.Error("Tried to create a new ObjectState with a NULL ObjectAgent!", new ArgumentNullException(nameof(@object), "ObjectAgent argument cannot be null!"));

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

        public bool Equals(ObjectState other) => other != null && other.Header == Header;

        public override bool Equals(object obj) => obj is ObjectState objState && Equals(objState);

        public override int GetHashCode() => Header.GetHashCode();

        public static bool operator ==(ObjectState a, ObjectState b) => a.Equals(b);
        public static bool operator !=(ObjectState a, ObjectState b) => !a.Equals(b);

    }

}
