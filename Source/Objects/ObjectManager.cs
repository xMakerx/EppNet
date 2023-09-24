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

        protected ObjectDelegate _Internal_CreateObject(ObjectRegistration reg, long id = -1)
        {
            ISimUnit unit = null;
            ObjectDelegate objDel = null;
            string distroName = Simulation.Get().DistroType.ToString();

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
                    unit = reg.ObjectAttribute.Creator();
                else
                    unit = reg.NewInstance() as ISimUnit;

                if (unit != null)
                {
                    id = (id == -1) ? _AllocateId() : id;
                    objDel = new ObjectDelegate(reg, unit, id);

                    _objects.Add(id, objDel);
                    Log.Verbose($"[ObjectManager#CreateObject()] Created new Object Instance of Type {typeName} assigned with ID {id}.");
                    return objDel;
                }

                // Something went wrong and we didn't get a valid unit back.
                Log.Fatal($"[ObjectManager#CreateObject()] Failed to create new instance of Type {typeName}. ISimUnit is null. Custom Constructor: {customGenerator}.");
            }
            catch (Exception ex)
            {
                Log.Fatal($"[ObjectManager#CreateObject()] Failed to create new instance of Type {typeName}. Exception: {ex.Message}");
                Log.Fatal($"[ObjectManager#CreateObject()] Exception Stack Trace: {ex.StackTrace}");
            }

            return objDel;
        }

        public ObjectDelegate CreateObject<T>() where T : ISimUnit
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

        public bool IsIdAvailable(long id) => !_objects.ContainsKey(id);

        public ObjectDelegate GetObject(long id)
        {
            _objects.TryGetValue(id, out ObjectDelegate result);
            return result;
        }

    }

}
