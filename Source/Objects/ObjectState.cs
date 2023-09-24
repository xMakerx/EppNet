///////////////////////////////////////////////////////
/// Filename: ObjectState.cs
/// Date: September 24, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Utilities;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EppNet.Objects
{

    public class ObjectState
    {

        public readonly ObjectDelegate WrappedObject;
        public readonly ulong Time;

        public ReadOnlyDictionary<string, object> Members2Value { private set; get; }

        public ObjectState(ObjectDelegate objDelegate, ulong time)
        {
            this.WrappedObject = objDelegate;
            this.Time = time;
            this.Members2Value = null;
        }

        public void RecordCurrent()
        {
            if (Members2Value != null)
            {
                Serilog.Log.Warning($"[ObjectState#RecordCurrent()] Object Type {WrappedObject.UserObject.GetType().Name} with ID {WrappedObject.ID}: Duplicate call!");
                return;
            }

            Dictionary<string, object> dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, ObjectMemberDefinition> kvp in WrappedObject.Metadata._methods)
            {
                if (!kvp.Value.Attribute.Flags.IsFlagSet(Core.NetworkFlags.Snapshot))
                    continue;

                object current = kvp.Value.InvokeGetter(WrappedObject);
                dict.Add(kvp.Key, current);
            }

            this.Members2Value = new ReadOnlyDictionary<string, object>(dict);
        }

    }

}
