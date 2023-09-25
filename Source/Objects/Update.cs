///////////////////////////////////////////////////////
/// Filename: Update.cs
/// Date: September 25, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data.Datagrams;
using EppNet.Utilities;

using System;
using System.Collections.Generic;

namespace EppNet.Objects
{

    public class Update : IDisposable
    {

        public static readonly int DefaultPoolCapacity = 100;
        public static readonly int MinimumMaxPoolCapacity = 100;
        public static readonly int MaxPoolCapacity = 500;

        private static Queue<Update> _updatePool;
        private static int _maxPoolCapacity = MaxPoolCapacity;

        /// <summary>
        /// Sets the capacity of the update pool and fills it
        /// </summary>
        /// <param name="capacity"></param>

        public static void SetPoolCapacity(int capacity)
        {
            _updatePool = new Queue<Update>(capacity);

            for (int i = 0; i < capacity; i++)
                _updatePool.Enqueue(new Update());
        }

        /// <summary>
        /// Sets the maximum amount of instances in the object pool.
        /// NOTE: If the object pool has already been instantiated and the new maximum capacity
        /// is greater than the amount in the pool, the extras will be dequeued.
        /// </summary>
        /// <param name="maxCapacity"></param>
        /// <exception cref="ArgumentOutOfRangeException">Maximum capacity is less than <see cref="MinimumMaxPoolCapacity"/></exception>

        public static void SetMaxPoolCapacity(int maxCapacity)
        {
            if (maxCapacity < MinimumMaxPoolCapacity)
                throw new ArgumentOutOfRangeException($"Update Pool Maximum Capacity must be at least {MinimumMaxPoolCapacity}!");

            _maxPoolCapacity = maxCapacity;

            if (_updatePool != null && _maxPoolCapacity < _updatePool.Count)
            {
                int toRemove = _updatePool.Count - _maxPoolCapacity;

                for (int i = 0; i < toRemove; i++)
                    _updatePool.Dequeue();
            }
        }

        private static Update _GetFromPoolOrInstantiate()
        {
            if (_updatePool == null)
                SetPoolCapacity(DefaultPoolCapacity);

            if (_updatePool.Count == 0)
                return new Update();
            else
                return _updatePool.Dequeue();
        }

        private static void _TryEnqueue(Update update)
        {
            if (update == null)
                return;

            if (_updatePool == null)
            {
                SetPoolCapacity(DefaultPoolCapacity - 1);
                _updatePool.Enqueue(update);
            }

            if (_updatePool.Count < _maxPoolCapacity)
                _updatePool.Enqueue(update);
        }

        public static Update For(ObjectDelegate objDelegate, ObjectMemberDefinition mDef, object[] args)
        {
            Update update = _GetFromPoolOrInstantiate();
            update.Initialize(objDelegate, mDef, args);
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

            byte dgArgs = datagram.ReadByte();
            int numArgs = mDef.ParameterTypes.Length;

            if (dgArgs != numArgs)
                throw new ArgumentException($"[Update#From()] Could not deserialize update for Object of Type {reg.GetRegisteredType().Name} with ID {objDelegate.ID}. Incompatible arguments!");

            object[] args = new object[numArgs];
            for (int i = 0; i < numArgs; i++)
            {
                Type type = mDef.ParameterTypes[i];
                object o = datagram.TryRead(type);

                if (o == null)
                    throw new FormatException($"[Update#WriteTo()] Datagram does not know how to deserialize {type.Name}!");

                args[i] = o;
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

        private Update()
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

            this.MemberDefinition.Invoke(ObjectDelegate, Arguments);
        }

        public void WriteTo(Datagram datagramIn)
        {
            if (!_initialized)
                throw new ArgumentException("Update has not been initialized correct! Did you forget to call #Initialize()?");

            if (datagramIn == null)
                throw new ArgumentNullException("Cannot write to a null Datagram!");

            byte updateId = (byte)MemberDefinition.Index;
            if (MemberDefinition.IsProperty())
            {
                // Properties have bit 7 enabled
                updateId = updateId.EnableBit(7);
            }

            // Encoded member ID
            datagramIn.WriteByte(updateId);

            if (this.Arguments.Length > 0)
            {
                datagramIn.WriteByte((byte) Arguments.Length);

                for (int i = 0; i < Arguments.Length; i++)
                {
                    bool written = datagramIn.TryWrite(Arguments[i]);

                    if (!written)
                        throw new FormatException($"[Update#WriteTo()] Datagram does not know how to serialize {Arguments[i].GetType()}!");
                }
            }
        }

        public void Dispose()
        {
            this.ObjectDelegate = null;
            this.Registration = null;
            this.MemberDefinition = null;
            this.Arguments = null;
            this._initialized = false;

            // Let's try to add back to the pool.
            _TryEnqueue(this);
        }

    }
}
