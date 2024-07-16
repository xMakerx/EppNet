///////////////////////////////////////////////////////
/// Filename: ObjectManagerService.cs
/// Date: July 16, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Logging;
using EppNet.Registers;
using EppNet.Services;
using EppNet.Sim;

using System;
using System.Collections.Concurrent;

namespace EppNet.Objects
{

    public class ObjectManagerService : Service
    {

        protected ConcurrentDictionary<long, ObjectSlot> _id2Slot;
        protected ConcurrentDictionary<ISimUnit, ObjectSlot> _unit2Slot;

        protected ConcurrentQueue<ObjectSlot> _deleteLaterQueue;

        public ObjectManagerService(ServiceManager svcMgr) : base(svcMgr)
        {
            this._id2Slot = new();
            this._unit2Slot = new();
            this._deleteLaterQueue = new();
        }

        public ObjectSlot GetSlotFor(ISimUnit unit)
        {
            if (unit == null)
            {
                Notify.Debug(new TemplatedMessage("Tried to fetch ObjectSlot for a null ISimUnit??"));
                return default;
            }

            if (!_unit2Slot.TryGetValue(unit, out var slot))
                Notify.Warning(new TemplatedMessage("Tried to fetch ObjectSlot for unknown ISimUnit??"));

            return slot;
        }

        public ObjectAgent GetAgentBySlot(ObjectSlot slot) => GetAgentById(slot);

        public ObjectAgent GetAgentById(long id)
        {
            _id2Slot.TryGetValue(id, out var slot);
            return slot.Agent;
        }

        public ObjectAgent GetAgentFor(ISimUnit unit)
        {
            if (unit == null)
            {
                Notify.Debug(new TemplatedMessage("Tried to fetch ObjectAgent for a null ISimUnit??"));
                return null;
            }

            ObjectSlot slot = GetSlotFor(unit);
            return slot.Agent;
        }

        /// <summary>
        /// Tries to create a new instance of the specified object type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>

        public ObjectAgent TryCreateObject<T>() where T : ISimUnit
        {
            ObjectRegistration registration = ObjectRegister.Get().Get(typeof(T)) as ObjectRegistration;
            return _Internal_CreateObject(registration);
        }

        /// <summary>
        /// Tries to request the deletion of an object by ID
        /// </summary>
        /// <param name="agent"></param>
        /// <returns>Whether or not the request was fulfilled</returns>

        public bool TryRequestDelete(long ID)
        {

            if (_id2Slot.TryGetValue(ID, out var slot))
            {
                // Great! We know what this object is!
                // We can only delete an object if it's in the generated or disabled state
                if (slot.State == EnumObjectState.Generated || slot.State == EnumObjectState.Disabled)
                {
                    slot.State = EnumObjectState.PendingDelete;
                    _deleteLaterQueue.Enqueue(slot);

                    // TODO: Add ticks until deletion or whatever
                    Notify.Debug(new TemplatedMessage("Requested deletion of Object ID {id}!", ID));
                    return true;
                }

                Notify.Warning(new TemplatedMessage("Cannot request deletion of Object ID {id} because it hasn't been generated yet.", ID));
                return false;
            }

            // We have no idea what this object is
            Notify.Error(new TemplatedMessage("Tried to request deletion of unknown Object ID {id}. Maybe it's already deleted?", ID));
            return false;
        }

        public bool IsIdAvailable(long id) => !_id2Slot.ContainsKey(id);

        protected long _Internal_AllocateId()
        {
            long id;
            do
            {
                id = Random.Shared.NextInt64();
            } while (!IsIdAvailable(id));

            return id;
        }

        protected ObjectAgent _Internal_CreateObject(ObjectRegistration registration, long id = -1)
        {
            ISimUnit unit = null;
            ObjectAgent agent = null;
            string distroName = nameof(_serviceMgr.Node.Distro);

            if (registration == null)
            {
                if (id != -1)
                {
                    // We weren't provided an object registration but were provided an ID
                    var msg = new TemplatedMessage("Unable to create Object with ID {id}. Is it registered? Does it have a {distro} Distribution? IdAvailable={Available}",
                        id, distroName, IsIdAvailable(id));
                    Notify.Fatal(msg);
                }
                else
                {
                    // We weren't provided anything :(
                    var msg = new TemplatedMessage("Unable to create Object with ID {id}. Is it registered? Does it have a {distro} Distribution?",
                        id, distroName);
                    Notify.Fatal(msg);
                }

                return null;
            }

            Type type = registration.GetRegisteredType();
            string typeName = type.Name;

            if (id != -1 && !IsIdAvailable(id))
            {
                TemplatedMessage msg = new("Unable to create Object of Type {typeName} with ID {id} as the ID is unavailable!",
                    typeName, id);
                Notify.Fatal(msg);
                return null;
            }

            bool customGenerator = registration.ObjectAttribute.Creator != null;

            try
            {

                if (customGenerator)
                    // Object has a specified custom generator
                    unit = registration.ObjectAttribute.Creator.Invoke();
                else
                    unit = registration.NewInstance() as ISimUnit;

                if (unit == null)
                {
                    // Something went wrong and we were unable to generate an instance of the object.
                    Notify.Fatal(new TemplatedMessage("Unable to create Object of Type {typeName} with ID {id}! Dynamic object generation failed! Custom Constructor={custom}",
                        typeName, id, customGenerator));
                    return null;
                }

                id = (id == -1) ? _Internal_AllocateId() : id;
                agent = new(registration, unit, id);

                ObjectSlot slot = new()
                {
                    ID = id,
                    Agent = agent,
                };

                _id2Slot.TryAdd(id, slot);
                _unit2Slot.TryAdd(unit, slot);

                Notify.Debug(new TemplatedMessage("Created new Object of Type {typeName} with ID {id}! Custom Constructor={custom}",
                    typeName, id, customGenerator));
            }
            catch (Exception e)
            {
                // Something went wrong somewhere else?
                Notify.Fatal(new TemplatedMessage("Failed to create Object of Type {typeName} with ID {id}! Dynamic object generation failed! Custom Constructor={custom}",
                        typeName, id, customGenerator), e);
                _serviceMgr.Node.HandleException(e);
            }

            return agent;
        }

    }

}