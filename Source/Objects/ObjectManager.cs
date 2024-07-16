///////////////////////////////////////////////////////
/// Filename: ObjectManager.cs
/// Date: September 17, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Core;
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

        protected readonly Distribution _distro_type;

        /// <summary>
        /// These are our created objects.
        /// </summary>
        protected Dictionary<long, ObjectAgent> _id2Delegate;

        protected Dictionary<ISimUnit, ObjectAgent> _unit2Delegate;

        public ObjectManager()
        {
            if (_instance != null)
                return;

            ObjectManager._instance = this;
            this._sim = Simulation.Get();
            this._id2Delegate = new Dictionary<long, ObjectAgent>();
            this._unit2Delegate = new Dictionary<ISimUnit, ObjectAgent>();
            this._distro_type = _sim.DistroType;
        }

        public bool Delete(ObjectAgent objDelegate)
        {
            if (objDelegate == null)
                return false;

            long id = objDelegate.ID;
            bool removed = _id2Delegate.Remove(id);
            _unit2Delegate.Remove(objDelegate.UserObject);

            // Call the onDelete method
            objDelegate.UserObject.OnDelete();

            return removed;
        }

        public ObjectAgent GetDelegateFor(ISimUnit unit)
        {
            if (unit == null)
                return null;

            _unit2Delegate.TryGetValue(unit, out ObjectAgent objDelegate);
            return objDelegate;
        }

        protected ObjectAgent _Internal_CreateObject(ObjectRegistration reg, long id = -1)
        {
            ISimUnit unit = null;
            ObjectAgent objDel = null;
            string distroName = nameof(_distro_type);

            if (reg == null)
            {
                if (id != -1)
                    Log.Fatal($"[ObjectManager#CreateObject()] Unable to create object with ID {id}. Is it registered? Does it have a {distroName} Distribution? IdAvailable={IsIdAvailable(id)}");
                else
                    Log.Fatal($"[ObjectManager#CreateObject()] Unable to create object. Is it registered? Does it have a {distroName} Distribution?");

                return null;
            }

            Type type = reg.GetRegisteredType();
            string typeName = type.Name;

            if (id != -1 && !IsIdAvailable(id))
            {
                Log.Fatal($"[ObjectManager#CreateObject()] Cannot create object of Type {typeName} with ID {id} as the ID is unavailable!");
                return null;
            }

            try
            {
                bool customGenerator = reg.ObjectAttribute.Creator != null;

                if (customGenerator)
                    // The user has specified a generator
                    unit = reg.ObjectAttribute.Creator.Invoke();
                else
                    unit = reg.NewInstance() as ISimUnit;

                if (unit == null)
                {
                    // Something went wrong and we didn't get a valid unit back.
                    Log.Fatal($"[ObjectManager#CreateObject()] Failed to create new instance of Type {typeName}. ISimUnit is null. Custom Constructor: {customGenerator}.");
                    return null;
                }

                id = (id == -1) ? _AllocateId() : id;
                objDel = new ObjectAgent(reg, unit, id);

                _id2Delegate.Add(id, objDel);
                _unit2Delegate.Add(unit, objDel);
                Log.Verbose($"[ObjectManager#CreateObject()] Created new Object Instance of Type {typeName} assigned with ID {id}.");
                return objDel;

            }
            catch (Exception ex)
            {
                Log.Fatal($"[ObjectManager#CreateObject()] Failed to create new instance of Type {typeName}. Exception: {ex.Message}");
                Log.Fatal($"[ObjectManager#CreateObject()] Exception Stack Trace: {ex.StackTrace}");
            }

            return objDel;
        }

        public ObjectAgent CreateObject<T>() where T : ISimUnit
        {
            ObjectRegistration reg = ObjectRegister.Get().Get(typeof(T)) as ObjectRegistration;
            return _Internal_CreateObject(reg);
        }

        protected long _AllocateId()
        {
            long id;
            do
            {
                id = Random.Shared.NextInt64();
            } while (!IsIdAvailable(id));

            return id;
        }

        public bool IsIdAvailable(long id) => !_id2Delegate.ContainsKey(id);

        public ObjectAgent GetObject(long id)
        {
            _id2Delegate.TryGetValue(id, out ObjectAgent result);
            return result;
        }

    }

}
