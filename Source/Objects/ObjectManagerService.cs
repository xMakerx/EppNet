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
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace EppNet.Objects
{

    public class ObjectManagerService : Service
    {

        /// <summary>
        /// Raised when a new object is created
        /// </summary>
        public Action<ObjectCreatedEvent> OnObjectCreated;

        /// <summary>
        /// Raised when an existing object is deleted
        /// </summary>
        public Action<ObjectDeletedEvent> OnObjectDeleted;

        protected enum EnumUserCodeType
        {
            OnCreate          = 0,
            OnDeleteRequested = 1,
            OnDelete          = 2,
            OnGenerate        = 3,
            AnnounceGenerate  = 4
        }

        protected readonly ConcurrentDictionary<long, ObjectSlot> _id2Slot;
        protected readonly ConcurrentDictionary<ISimUnit, ObjectSlot> _unit2Slot;

        /// <summary>
        /// This HashSet contains objects that are meant to be deleted later. <br/>
        /// This is not a thread-safe collection as it's only meant to be modified on one thread.
        /// </summary>
        protected readonly HashSet<ObjectSlot> _deleteLater;

        public ObjectManagerService(ServiceManager svcMgr) : base(svcMgr)
        {
            this._id2Slot = new();
            this._unit2Slot = new();
            this._deleteLater = new();
        }

        public ISimUnit GetSimUnitFor(ObjectSlot slot)
        {
            ISimUnit unit = slot.Agent?.UserObject;

            if (unit == null)
            {
                // We have to locate our unit
                foreach (KeyValuePair<ISimUnit, ObjectSlot> kvp in _unit2Slot)
                {
                    if (kvp.Value == slot)
                    {
                        unit = kvp.Key;
                        break;
                    }
                }
            }

            return unit;
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
        /// <returns>Whether or not the request was fulfilled</returns>

        public bool TryRequestDelete(long id, uint ticksUntilDeletion = 10)
        {

            if (id != -1 && _id2Slot.TryGetValue(id, out var slot))
            {
                // Great! We know what this object is!
                // We can only delete an object if it's in the generated or disabled state
                if (slot.State == EnumObjectState.Generated || slot.State == EnumObjectState.Disabled)
                {
                    // TODO: ObjectManagerService: Add better way to control how many ticks until deletion
                    slot.State = EnumObjectState.PendingDelete;
                    slot.Agent.TicksUntilDeletion = (int) ticksUntilDeletion;
                    _deleteLater.Add(slot);

                    // Running OnDeleteRequested user-code shouldn't brick the object manager.
                    // Call it wrapped in a try-catch to manage issues.
                    _Internal_SafeUserCodeCall(slot.Agent, EnumUserCodeType.OnDeleteRequested);
                    return true;
                }

                Notify.Warning(new TemplatedMessage("Cannot request deletion of Object ID {id} because it hasn't been generated yet.", id));
                return false;
            }

            // We have no idea what this object is
            Notify.Error(new TemplatedMessage("Tried to request deletion of unknown Object ID {id}. Maybe it's already deleted?", id));
            return false;
        }

        public bool IsIdAvailable(long id) => !_id2Slot.ContainsKey(id);

        public override bool Stop()
        {
            bool stopped = base.Stop();
            if (stopped)
            {
                // Let's delete our objects
                HashSet<ObjectSlot> toDelete = new(_id2Slot.Values);

                foreach (ObjectSlot slot in toDelete)
                    _Internal_DeleteObject(slot);
            }

            return stopped;
        }

        internal override void Update()
        {
            HashSet<ObjectSlot> clearThisTick = new();

            foreach (ObjectSlot slot in _deleteLater)
            {

                if (slot.Agent == null)
                {
                    Notify.Debug(new TemplatedMessage("Tried to process deletion of Object ID {id} with NULL ObjectAgent?", slot.ID));
                    clearThisTick.Add(slot);
                    continue;
                }

                if (--slot.Agent.TicksUntilDeletion < 1)
                    clearThisTick.Add(slot);
            }

            foreach (ObjectSlot slot in clearThisTick)
                _Internal_DeleteObject(slot);

            base.Update();
        }

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

            if (Status != ServiceState.Online)
            {
                // We weren't provided an object registration but were provided an ID
                var msg = new TemplatedMessage("Cannot create an Object while the ObjectManager Service is offline!");
                Notify.Fatal(msg);
                return null;
            }

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
                agent = new(this, registration, unit, id);

                ObjectSlot slot = new(id)
                {
                    Agent = agent,
                    State = EnumObjectState.WaitingForState
                };

                // Raise our event that a new object was created
                OnObjectCreated?.Invoke(new(slot));

                // Running OnCreate user-code shouldn't brick the object manager.
                // Call it wrapped in a try-catch to manage issues.
                _Internal_SafeUserCodeCall(agent, EnumUserCodeType.OnCreate);

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

        /// <summary>
        /// Safely calls user code or sends an exception to the <see cref="EppNet.Node.NetworkNode"/><br/>
        /// See <see cref="EnumUserCodeType"/> for the list of valid user functions in the <see cref="ISimUnit"/> interface.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="userCode"></param>
        /// <returns>Whether or not the user code ran without exceptions</returns>

        protected bool _Internal_SafeUserCodeCall(ObjectAgent agent, EnumUserCodeType userCode)
        {
            if (agent == null)
            {
                Notify.Error("Tried to call user code on a null ObjectAgent!");
                return false;
            }

            string codeName = string.Empty;

            // This code is kind of crap but it works so I don't violate DRY
            try
            {
                switch (userCode)
                {

                    case EnumUserCodeType.OnCreate:
                        codeName = "creation";
                        agent.UserObject.OnCreate(agent);
                        break;

                    case EnumUserCodeType.OnDeleteRequested:
                        codeName = "on delete requested";
                        agent.UserObject.OnDeleteRequested();
                        break;

                    case EnumUserCodeType.OnDelete:
                        codeName = "deletion";
                        agent.UserObject.OnDelete();
                        break;

                    case EnumUserCodeType.OnGenerate:
                        codeName = "generation";
                        agent.UserObject.OnGenerate();
                        break;

                    case EnumUserCodeType.AnnounceGenerate:
                        codeName = "announce generate";
                        agent.UserObject.AnnounceGenerate();
                        break;
                }

                // Handy debug function just in case.
                Notify.Debug($"Successfully callled user {codeName} function for Object ID {agent.ID}!");
            }
            catch (Exception e)
            {
                string msg = $"An error occurred while running user {codeName} function for Object ID {agent.ID}";
                Notify.Error(msg, e);
                _serviceMgr.Node.HandleException(e);
                return false;
            }

            return true;
        }

        protected bool _Internal_DeleteObject(ObjectSlot slot)
        {
            if (slot == default)
            {
                // This shouldn't happen.
                Notify.Warning("Tried to delete Object in the default slot??");
                return false;
            }

            ISimUnit unit = GetSimUnitFor(slot);
            bool success = true;

            success &= _id2Slot.TryRemove(slot, out var _);
            success &= _unit2Slot.TryRemove(unit, out var _);

            // Let's reset ticks left until deletion (if agent is valid)
            if (slot.Agent != null)
                slot.Agent.TicksUntilDeletion = -1;

            // Raise our event that an object was deleted
            OnObjectDeleted?.Invoke(new(slot));

            // Running user deletion code shouldn't brick the entire object manager.
            // This is wrapped with a try-catch to handle if something else goes wrong
            _Internal_SafeUserCodeCall(slot.Agent, EnumUserCodeType.OnDelete);

            // Let's set our state to deleted.
            slot.State = EnumObjectState.Deleted;

            // Remove from our delete later collection
            // This doesn't dictate the value of success because there will
            // be times when we skip the delete later and do it immediately.
            _deleteLater.Remove(slot);

            if (success)
                Notify.Debug(new TemplatedMessage("Successfully deleted Object ID {id}", slot.ID));
            else
                Notify.Warning(new TemplatedMessage("Deletion of Object ID {id} completed with unexpected behavior: a collection didn't manage it properly.", slot.ID));

            return success;
        }

    }

}