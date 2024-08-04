///////////////////////////////////////////////////////
/// Filename: ObjectCommands.cs
/// Date: August 4, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;
using EppNet.Logging;
using EppNet.Node;
using EppNet.Objects;
using EppNet.Utilities;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;


namespace EppNet.Commands
{

    public static class ObjectCommands
    {

        private static readonly List<SlottableEnum> _cmdsList = Commands._cmdsList;

        // Creation and deletion are in the same slot as they can undo each other
        public static readonly SlottableEnum Create = SlottableEnum._Internal_CreateAndAddTo(_cmdsList, "Create_Object", 1);
        public static readonly SlottableEnum Delete = SlottableEnum._Internal_CreateAndAddTo(_cmdsList, "Delete_Object", 1);

        // Generate, Enable, and Disable are in the same slot as they can undo each other
        public static readonly SlottableEnum Object_Generate = SlottableEnum._Internal_CreateAndAddTo(_cmdsList, "Object_Generate", 2);
        public static readonly SlottableEnum Object_Enable = SlottableEnum._Internal_CreateAndAddTo(_cmdsList, "Object_Enable", 2);
        public static readonly SlottableEnum Object_Disable = SlottableEnum._Internal_CreateAndAddTo(_cmdsList, "Object_Disable", 2);

        public static readonly SlottableEnum Object_SetParent = SlottableEnum._Internal_CreateAndAddTo(_cmdsList, "Object_SetParent", 3);
        public static readonly SlottableEnum Object_SetProperty = SlottableEnum._Internal_CreateAndAddTo(_cmdsList, "Object_SetProperty", 4);
        public static readonly SlottableEnum Object_CallMethod = SlottableEnum._Internal_CreateAndAddTo(_cmdsList, "Object_CallMethod", 5);
        public static readonly SlottableEnum Object_AckSnapshot = SlottableEnum._Internal_CreateAndAddTo(_cmdsList, "Object_AckSnapshot", 6);

    }

    public class ObjectSetParentCommand : ObjectCommand
    {

        public long ParentID { get; }

        public ObjectSetParentCommand([NotNull] NetworkNode node, long id, long parentID) : base(node, id, ObjectCommands.Object_SetParent)
        {
            if (parentID == id)
            {
                ArgumentException exp = new($"Tried to set Object ID {id}'s parent to itself!");
                node.Fatal(new TemplatedMessage("Tried to set Object ID {id}'s parent to itself!", ID), exp);
                return;
            }

            this.ParentID = parentID;
        }

        public override CommandResult Execute()
        {

            CommandResult result;

            if (_slot == null)
            {

                CommandResult lookMeUp = _Internal_LookupObject(ID);

                if (!lookMeUp.Success)
                    return lookMeUp;

                _slot = lookMeUp.Target as ObjectSlot;
                _agent = _slot.Agent;
            }

            result = _agent.ReparentTo(ParentID);

            if (!result.Message.Empty())
                _agent.Notify.Debug(result.Message);

            return result;
        }

    }

    public class CreateObjectCommand : Command
    {

        public int ObjectTypeId { get; }
        public long ID { get; }

        public CreateObjectCommand([NotNull] NetworkNode node, int objectTypeId) : this(node, objectTypeId, -1) { }

        public CreateObjectCommand([NotNull] NetworkNode node, int objectTypeId, long id) : base(node, ObjectCommands.Create)
        {
            this.ID = id;
            this.ObjectTypeId = objectTypeId;
        }

        public override CommandResult Execute()
        {
            CommandResult result = new();

            ObjectService service = Node.Services.GetService<ObjectService>();
            TemplatedMessage msg = new("Object Service could not be found!");

            if (this.IsNotNull(arg: service, tmpMsg: msg, fatal: true))
                result = service.TryCreateObject(ObjectTypeId, ID);
            else
                result.Message = msg;

            return result;
        }

    }

    public class DeleteObjectCommand : Command
    {

        public long ID { get; }
        public uint TicksUntilDeletion { get; }

        public DeleteObjectCommand([NotNull] NetworkNode node, long id, uint ticksUntilDeletion) : base(node, ObjectCommands.Delete)
        {
            this.ID = id;
            this.TicksUntilDeletion = ticksUntilDeletion;
        }

        public override CommandResult Execute()
        {
            ObjectService service = Node.Services.GetService<ObjectService>();
            this.IsNotNull(arg: service, tmpMsg: new TemplatedMessage("Object Service could not be found!"), fatal: true);
            return service.TryRequestDelete(ID, TicksUntilDeletion);
        }

    }

    public abstract class ObjectCommand : Command
    {

        public ObjectSlot Slot { get => _slot; }
        public ObjectAgent Agent { get => _agent; }
        public long ID { protected set; get; }

        protected ObjectSlot _slot;
        protected ObjectAgent _agent;

        protected ObjectCommand([NotNull] NetworkNode node, long id, SlottableEnum cmdType) : base(node, cmdType)
        {
            this.ID = id;
        }

        protected ObjectCommand([NotNull] ObjectSlot slot, SlottableEnum cmdType) : base(slot?.Agent, cmdType)
        {
            Guard.AgainstNull(slot);
            this._slot = slot;
            this._agent = slot.Agent;
        }

        protected CommandResult _Internal_LookupObject(long id)
        {
            CommandResult result = new();
            ObjectService service = Node.Services.GetService<ObjectService>();
            TemplatedMessage msg = new("Object Service could not be found!");

            bool hasService = this.IsNotNull(arg: service, tmpMsg: msg, fatal: true);

            if (hasService && service.TryGetById(id, out ObjectSlot slot))
            {
                result.Target = slot;
                result.Success = true;
            }

            result.Message = (hasService && result.Target == null) ? new("Object could not be found!") : msg;
            return result;
        }

    }

}