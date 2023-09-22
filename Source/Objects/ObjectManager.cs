///////////////////////////////////////////////////////
/// Filename: ObjectManager.cs
/// Date: September 17, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Registers;
using EppNet.Sim;

using Serilog;

using System;
using System.Collections.Generic;

namespace EppNet.Objects
{

    public class ObjectManager
    {

        protected static ObjectManager _instance;
        public static ObjectManager Get() => _instance;

        protected readonly Simulation _sim;

        /// <summary>
        /// These are our created objects.
        /// </summary>
        protected Dictionary<long, ObjectDelegate> _objects;

        public ObjectManager()
        {
            if (_instance != null)
                return;

            ObjectManager._instance = this;
            this._sim = Simulation.Get();
            this._objects = new Dictionary<long, ObjectDelegate>();
        }

        public ObjectDelegate CreateObject<T>() where T : ISimUnit
        {
            ObjectRegistration reg = ObjectRegister.Get().Get(typeof(T)) as ObjectRegistration;

            if (reg == null)
                Log.Fatal($"[ObjectManager#CreateObject<T>()] Tried to create unknown object of Type {typeof(T).Name}.");

            long id = _AllocateId();
            ISimUnit unit = null;

            if (reg.ObjectAttribute.Creator != null)
                // The user has specified a generator
                unit = reg.ObjectAttribute.Creator();
            else
                unit = reg.NewInstance() as ISimUnit;

            ObjectDelegate objDel = new ObjectDelegate(unit);
            objDel.ID = id;
            return objDel;
        }

        protected long _AllocateId()
        {
            long id;
            do
            {
                id = Random.Shared.NextInt64();
            } while (_objects.ContainsKey(id));

            return id;
        }

        public ObjectDelegate GetObject(long id)
        {
            _objects.TryGetValue(id, out ObjectDelegate result);
            return result;
        }

    }

}
