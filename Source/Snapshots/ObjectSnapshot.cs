///////////////////////////////////////////////////////
/// Filename: ObjectState.cs
/// Date: September 24, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Commands;
using EppNet.Logging;
using EppNet.Objects;
using EppNet.Time;
using EppNet.Utilities;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EppNet.Snapshots
{

    /// <summary>
    /// Represents a "snapshot" of a <see cref="EppNet.Sim.ISimUnit"/> at a particular time in the simulation.<br/>
    /// Each state contains a unique, randomly generated header of <see cref="HeaderLength"/> characters<br/>
    /// Network properties and network methods with implemented getters (usually snapshot methods) are stored here
    /// </summary>

    public class ObjectSnapshot : Snapshot
    {

        public readonly ObjectAgent Object;

        protected ConcurrentQueue<ObjectCommand> _commands;

        protected SortedList<string, object> _method2Value;
        protected SortedList<string, object> _prop2Value;

        public ObjectSnapshot(ObjectAgent @object, Snapshot parent) : base(parent)
        {
            Guard.AgainstNull(@object);
            this.Object = @object;

            this._method2Value = new(ObjectRegistration.StringSortComparer);
            this._prop2Value = new(ObjectRegistration.StringSortComparer);
        }

        /// <summary>
        /// Captures the current value of each network property and snapshot network method
        /// </summary>

        public override void RecordCurrent()
        {

            SortedList<string, ObjectMemberDefinition> list = Object.Metadata._methods;
            int index = 0;

            while (index < list.Count)
            {
                string key = list.GetKeyAtIndex(index);
                ObjectMemberDefinition memDef = list[key];

                if (memDef.IsProperty() || (memDef.IsMethod() && memDef.Attribute.Flags.IsFlagSet(NetworkFlags.Snapshot)))
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
        /// See <see cref="ulong.CompareTo(ulong)"/><br/>
        /// This function is similar but null or undefined comparisons return 1.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>

        public override int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            if (obj is ObjectSnapshot x)
                return CompareTo(x);

            Object.Service.Notify.Error("Invalid comparison!", new ArgumentException("Invalid comparison!", nameof(obj)));
            return 1;
        }
    }

}
