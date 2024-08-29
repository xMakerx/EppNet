///////////////////////////////////////////////////////
/// Filename: ObjectService.cs
/// Date: July 16, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Collections;
using EppNet.Commands;
using EppNet.Logging;
using EppNet.Registers;
using EppNet.Services;
using EppNet.Sim;
using EppNet.Utilities;

using System;
using System.Collections.Generic;

namespace EppNet.Objects
{

    public class ObjectService : Service
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

        protected readonly PageList<ObjectSlot> _objects;

        /// <summary>
        /// This HashSet contains objects that are meant to be deleted later. <br/>
        /// This is not a thread-safe collection as it's only meant to be modified on one thread.
        /// </summary>
        protected readonly HashSet<ObjectSlot> _deleteLater;

        public ObjectService(ServiceManager svcMgr) : base(svcMgr)
        {
            this._objects = new(256);
            this._deleteLater = new();
        }

        public bool TryGetById(long id, out ObjectSlot slot) => _objects.TryGetById(id, out slot);

        public ISimUnit GetSimUnitFor(ObjectSlot slot)
        {
            ISimUnit unit = slot.Agent?.UserObject;

            if (unit == null && _objects.TryGetById(slot.ID, out ObjectSlot found))
                unit = found.Agent?.UserObject;

            return unit;
        }

        public ObjectSlot GetSlotFor(ISimUnit unit)
        {
            if (unit == null)
            {
                Notify.Debug(new TemplatedMessage("Tried to fetch ObjectSlot for a null ISimUnit??"));
                return default;
            }

            if (!_objects.TryGetById(unit.ID, out ObjectSlot slot))
                Notify.Warning(new TemplatedMessage("Tried to fetch ObjectSlot for unknown ISimUnit??"));

            return slot;
        }

        public ObjectAgent GetAgentById(long id)
        {
            ObjectAgent agent = null;

            if (_objects.TryGetById(id, out ObjectSlot slot))
                agent = slot.Agent;

            return agent;
        }

        public ObjectAgent GetAgentFor(ISimUnit unit)
        {
            if (!this.IsNotNull(unit, "Tried to fetch ObjectAgent for a null ISimUnit??"))
                return null;

            ObjectSlot slot = GetSlotFor(unit);
            return slot.Agent;
        }

        /// <summary>
        /// Tries to create a new instance of the specified object type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>

        public EnumCommandResult TryCreateObject<T>(out ObjectSlot slot, long id = -1) where T : ISimUnit => TryCreateObject(typeof(T), out slot, id);

        public EnumCommandResult TryCreateObject(Type type, out ObjectSlot slot, long id = -1)
        {
            slot = null;
            EnumCommandResult result = EnumCommandResult.BadArgument;

            if (ObjectRegister.Get().Get(type) is ObjectRegistration registration)
                result = _Internal_CreateObject(registration, out slot, id);

            return result;
        }

        public EnumCommandResult TryCreateObject(int typeId, out ObjectSlot slot, long id = -1)
        {
            slot = null;
            EnumCommandResult result = EnumCommandResult.BadArgument;

            if (ObjectRegister.Get().Get(typeId) is ObjectRegistration registration)
                result = _Internal_CreateObject(registration, out slot, id);

            return result;
        }

        /// <summary>
        /// Tries to request the deletion of an object by ID
        /// </summary>
        /// <returns>Whether or not the request was fulfilled</returns>

        public EnumCommandResult TryRequestDelete(long id, uint ticksUntilDeletion = 10)
        {

            if (id != -1 && _objects.TryGetById(id, out ObjectSlot slot))
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
                    return EnumCommandResult.Ok;
                }

                Notify.Warning(new TemplatedMessage("Cannot request deletion of Object ID {id} because it hasn't been generated yet.", id));
                return EnumCommandResult.InvalidState;
            }

            // We have no idea what this object is
            Notify.Error(new TemplatedMessage("Tried to request deletion of unknown Object ID {id}. Maybe it's already deleted?", id));
            return EnumCommandResult.NotFound;
        }

        public bool IsIdAvailable(long id) => _objects.IsAvailable(id);

        public override bool Stop()
        {
            bool stopped = base.Stop();
            if (stopped)
            {
                // Let's delete our objects
                _objects.DoOnActive((ObjectSlot slot) => _Internal_DeleteObject(slot));
            }

            return stopped;
        }

        public override void Dispose(bool disposing)
        {
            OnObjectCreated = null;
            OnObjectDeleted = null;
            _objects.Dispose();
        }

        public override bool Tick(float dt)
        {
            if (!Started)
                return false;

            Update(dt);
            base.Tick(dt);
            return true;
        }

        internal void Update(float dt)
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
        }

        protected EnumCommandResult _Internal_CreateObject(ObjectRegistration registration, out ObjectSlot slot, long id = -1)
        {
            slot = null;

            if (Status != ServiceState.Online)
            {
                var msg = new TemplatedMessage("Cannot create an Object while the ObjectManager Service is offline!");
                Notify.Fatal(msg);
                return EnumCommandResult.NoService;
            }

            ISimUnit unit = null;
            ObjectAgent agent = null;
            const string distroName = nameof(_serviceMgr.Node.Distro);

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

                return EnumCommandResult.BadArgument;
            }

            Type type = registration.GetRegisteredType();
            string typeName = type.Name;

            if (id != -1 && !IsIdAvailable(id))
            {
                TemplatedMessage msg = new("Unable to create Object of Type {typeName} with ID {id} as the ID is unavailable!",
                    typeName, id);
                Notify.Fatal(msg);
                return EnumCommandResult.Unavailable;
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
                    return EnumCommandResult.InvalidState;
                }

                bool allocated;

                if (id == -1)
                    allocated = _objects.TryAllocate(out slot);
                else
                    allocated = _objects.TryAllocate(id, out slot);

                if (!allocated)
                    return EnumCommandResult.Unavailable;

                id = slot.ID;
                agent = new(this, registration, unit, id);

                slot.Agent = agent;
                slot.State = EnumObjectState.WaitingForState;

                // Raise our event that a new object was created
                OnObjectCreated?.GlobalInvoke(new(this, slot));

                // Running OnCreate user-code shouldn't brick the object manager.
                // Call it wrapped in a try-catch to manage issues.
                _Internal_SafeUserCodeCall(agent, EnumUserCodeType.OnCreate);

                Notify.Debug(new TemplatedMessage("Created new Object of Type {typeName} with ID {id}! Custom Constructor={custom}",
                    typeName, id, customGenerator));

                return EnumCommandResult.Ok;
            }
            catch (Exception e)
            {
                // Something went wrong somewhere else?
                Notify.Fatal(new TemplatedMessage("Failed to create Object of Type {typeName} with ID {id}! Dynamic object generation failed! Custom Constructor={custom}",
                        typeName, id, customGenerator), e);
            }

            return EnumCommandResult.InvalidState;
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
            if (!this.IsNotNull(agent, message: "Tried to call user code on a null ObjectAgent!"))
                return false;

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
                return false;
            }

            return true;
        }

        protected bool _Internal_DeleteObject(ObjectSlot slot)
        {
            if (slot == null)
            {
                // This shouldn't happen.
                Notify.Warning("Tried to delete Object in the default slot??");
                return false;
            }

            // Let's set our state to deleted.
            slot.State = EnumObjectState.Deleted;

            // Let's reset ticks left until deletion (if agent is valid)
            if (slot.Agent != null)
                slot.Agent.TicksUntilDeletion = -1;

            // Raise our event that an object was deleted
            OnObjectDeleted?.Invoke(new(this, slot));

            // Running user deletion code shouldn't brick the entire object manager.
            // This is wrapped with a try-catch to handle if something else goes wrong
            _Internal_SafeUserCodeCall(slot.Agent, EnumUserCodeType.OnDelete);

            // Remove from our delete later collection
            // This doesn't dictate the value of success because there will
            // be times when we skip the delete later and do it immediately.
            _deleteLater.Remove(slot);

            bool success = _objects.TryFree(slot);

            if (success)
                Notify.Debug(new TemplatedMessage("Successfully deleted Object ID {id}", slot.ID));
            else
                Notify.Warning(new TemplatedMessage("Deletion of Object ID {id} completed with unexpected behavior: a collection didn't manage it properly.", slot.ID));

            return success;
        }

    }

}