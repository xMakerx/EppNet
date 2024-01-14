///////////////////////////////////////////////////////
/// Filename: Update.cs
/// Date: September 25, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data.Datagrams;
using EppNet.Utilities;

using System;

namespace EppNet.Objects
{

    public class Update : IPoolable
    {

        private static UpdatePool _updatePool = new UpdatePool();

        /// <summary>
        /// See <see cref="ObjectPool{T}.SetCapacity(int)"/>
        /// </summary>
        /// <param name="capacity"></param>

        public static void SetPoolCapacity(int capacity) => _updatePool.SetCapacity(capacity);


        /// <summary>
        /// See <see cref="ObjectPool{T}.SetMaxCapacity(int)"/>
        /// </summary>
        /// <param name="maxCapacity"></param>

        public static void SetMaxPoolCapacity(int maxCapacity) => _updatePool.SetMaxCapacity(maxCapacity);

        /// <summary>
        /// See <see cref="ObjectPool{T}.Get"/>
        /// </summary>
        /// <returns></returns>
        private static Update _GetFromPoolOrInstantiate() => _updatePool.Get() as Update;

        public static Update For(ObjectDelegate objDelegate, ObjectMemberDefinition mDef, object[] args)
        {
            Update update = _GetFromPoolOrInstantiate();
            update.Initialize(objDelegate, mDef, args);
            return update;
        }

        public static Update For(ObjectDelegate objDelegate, string updateName, object[] args)
        {
            Update update = _GetFromPoolOrInstantiate();
            update.Initialize(objDelegate, updateName, args);
            return update;
        }

        public static Update From(ObjectDelegate objDelegate, Datagram datagram)
        {
            if (objDelegate == null)
                throw new ArgumentNullException("Delegate cannot be null!");

            if (datagram == null)
                throw new ArgumentNullException("Cannot read from a null Datagram!");

            ObjectRegistration reg = objDelegate.Metadata;
            byte updateId = datagram.ReadByte();
            bool isProperty = updateId.IsBitOn(7);

            if (isProperty)
                updateId = updateId.ResetBit(7);

            ObjectMemberDefinition mDef = (isProperty) ? reg.GetProperty((int)updateId) : reg.GetMethod((int)updateId);

            if (mDef == null)
                throw new ArgumentException($"[Update#From()] Could not deserialize update for Object of Type {reg.GetRegisteredType().Name} with ID {objDelegate.ID}. No member definition found!");

            int numArgs = mDef.ParameterTypes.Length;
            object[] args = null;

            if (numArgs > 0)
            {
                args = new object[numArgs];
                int dgArgs = datagram.ReadByte();

                if (dgArgs != numArgs)
                    throw new ArgumentException($"[Update#From()] Could not deserialize update for Object of Type {reg.GetRegisteredType().Name} with ID {objDelegate.ID}. Incompatible arguments!");

                for (int i = 0; i < numArgs; i++)
                {
                    Type type = mDef.ParameterTypes[i];
                    object o = datagram.TryRead(type);

                    if (o == null)
                        throw new FormatException($"[Update#WriteTo()] Datagram does not know how to deserialize {type.Name}!");

                    args[i] = o;
                }
            }

            Update update = _GetFromPoolOrInstantiate();
            update.ObjectDelegate = objDelegate;
            update.Registration = reg;
            update.MemberDefinition = mDef;
            update.Arguments = args;
            update._initialized = true;
            return update;
        }

        public ObjectDelegate ObjectDelegate { private set; get; }
        public ObjectRegistration Registration { private set; get; }

        public ObjectMemberDefinition MemberDefinition { private set; get; }
        public object[] Arguments { private set; get; }

        private bool _initialized;

        internal Update()
        {
            this.ObjectDelegate = null;
            this.Registration = null;
            this.MemberDefinition = null;
            this.Arguments = null;
            this._initialized = false;
        }

        public void Initialize(ObjectDelegate objDelegate, ObjectMemberDefinition mDef, object[] args)
        {
            this.ObjectDelegate = objDelegate;
            this.Registration = objDelegate.Metadata;
            this.MemberDefinition = mDef;
            this.Arguments = args;
            this._initialized = true;
        }

        public void Initialize(ObjectDelegate objDelegate, string updateName, object[] args)
        {
            if (objDelegate == null) 
                throw new ArgumentNullException("Delegate cannot be null!");

            this.ObjectDelegate = objDelegate;
            this.Registration = objDelegate.Metadata;
            this.MemberDefinition = Registration.GetMemberByName(updateName);

            if (MemberDefinition == null)
                throw new ArgumentException($"Update {updateName} on Object of Type {objDelegate.Metadata.GetRegisteredType().Name} is invalid! Did you type it correctly? Is it registered?");

            this.Arguments = args;
            this._initialized = true;
        }

        public void Invoke()
        {
            if (!_initialized)
                throw new ArgumentException("Update has not been initialized correct! Did you forget to call #Initialize()?");

            if (Arguments == null)
                MemberDefinition.Invoke(ObjectDelegate);
            else
                MemberDefinition.Invoke(ObjectDelegate, Arguments);
        }

        public void WriteTo(Datagram datagramIn)
        {
            if (!_initialized)
                throw new ArgumentException("Update has not been initialized correct! Did you forget to call #Initialize()?");

            if (datagramIn == null)
                throw new ArgumentNullException("Cannot write to a null Datagram!");

            byte updateId = (byte)MemberDefinition.Index;

            if (MemberDefinition.IsProperty())
                // Properties have bit 7 enabled
                updateId = updateId.EnableBit(7);

            // Encoded member ID
            datagramIn.WriteByte(updateId);

            int numArgs = Arguments?.Length ?? 0;

            datagramIn.WriteByte((byte)numArgs);
            
            for (int i = 0; i < numArgs; i++)
            {
                bool written = datagramIn.TryWrite(Arguments[i]);

                if (!written)
                    throw new FormatException($"[Update#WriteTo()] Datagram does not know how to serialize {Arguments[i].GetType()}!");
            }
        }

        public bool IsInitialized() => _initialized;

        public void Dispose()
        {
            this.ObjectDelegate = null;
            this.Registration = null;
            this.MemberDefinition = null;
            this.Arguments = null;
            this._initialized = false;

            // Let's try to add back to the pool.
            _updatePool.TryReturnToPool(this);
        }

    }
}
