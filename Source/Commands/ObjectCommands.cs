///////////////////////////////////////////////////////
/// Filename: ObjectCommands.cs
/// Date: August 4, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;
using EppNet.Logging;
using EppNet.Node;
using EppNet.Objects;
using EppNet.Time;
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

    public class ObjectCallMethodCommand : ObjectUpdateFieldCommand
    {
        public ObjectCallMethodCommand(int index, params object[] args) : base(false, index, ObjectCommands.Object_CallMethod, args) { }
    }

    public class ObjectSetPropertyCommand : ObjectUpdateFieldCommand
    {

        public ObjectSetPropertyCommand(int index, params object[] args) : base(true, index, ObjectCommands.Object_SetProperty, args) { }

    }

    public abstract class ObjectUpdateFieldCommand : ObjectCommand
    {

        public bool IsProperty { get; }
        public object[] Arguments { get; }
        public int Index { get; }

        protected ObjectUpdateFieldCommand(bool isProperty, int index, SlottableEnum enumType, params object[] args) : base(enumType)
        {
            this.IsProperty = isProperty;
            this.Arguments = args;
            this.Index = index;
        }

        public override EnumCommandResult Execute(in ObjectCommandContext context)
        {
            Guard.AgainstNull(context);

            if (!context.FetchObjectResult.IsOk())
                return context.FetchObjectResult;

            ObjectRegistration registration = context.Slot.Agent.Metadata;
            ObjectMemberDefinition mDef = IsProperty ? registration.GetProperty(Index) : registration.GetMethod(Index);

            if (mDef != null)
            {
                try
                {
                    mDef.Invoke(context.Slot.Agent.UserObject, Arguments);
                    return EnumCommandResult.Ok;
                }
                catch (Exception e)
                {
                    context.Node.HandleException(e);
                    return EnumCommandResult.InvalidState;
                }
            }

            return EnumCommandResult.BadArgument;
        }
    }

    public class ObjectSetParentCommand : ObjectCommand
    {
        public long ParentID { get; }

        public ObjectSetParentCommand(long parentID) : base(ObjectCommands.Object_SetParent)
        {
            this.ParentID = parentID;
        }

        public override EnumCommandResult Execute(in ObjectCommandContext context)
        {
            Guard.AgainstNull(context);

            if (ParentID == context.ID)
                return EnumCommandResult.BadArgument;

            if (!context.FetchObjectResult.IsOk())
                return context.FetchObjectResult;

            return context.Slot.Agent.ReparentTo(ParentID);
        }

    }

    public class CreateObjectCommand : ObjectCommand
    {

        public int ObjectTypeId { get; }

        public CreateObjectCommand(int objectTypeId) : base(ObjectCommands.Create)
        {
            this.ObjectTypeId = objectTypeId;
        }

        public override EnumCommandResult Execute(in ObjectCommandContext context)
        {
            Guard.AgainstNull(context);
            EnumCommandResult result = EnumCommandResult.NoService;

            ObjectService service = context.Node.Services.GetService<ObjectService>();

            if (this.IsNotNull(arg: service, tmpMsg: new("Object Service could not be found!"), fatal: true))
                result = service.TryCreateObject(ObjectTypeId, out _, context.ID);

            return result;
        }

    }

    public class DeleteObjectCommand : ObjectCommand
    {
        public uint TicksUntilDeletion { get; }

        public DeleteObjectCommand(uint ticksUntilDeletion) : base(ObjectCommands.Delete)
        {
            this.TicksUntilDeletion = ticksUntilDeletion;
        }

        public override EnumCommandResult Execute(in ObjectCommandContext context)
        {
            Guard.AgainstNull(context);

            ObjectService service = context.Node.Services.GetService<ObjectService>();
            this.IsNotNull(arg: service, tmpMsg: new TemplatedMessage("Object Service could not be found!"), fatal: true);
            return service.TryRequestDelete(context.ID, TicksUntilDeletion);
        }

    }

    public abstract class ObjectCommand : SlimCommand<ObjectCommandContext>
    {
        protected ObjectCommand(SlottableEnum enumType) : base(enumType) { }

    }

    public class ObjectCommandContext : CommandContext
    {
        public ObjectSlot Slot { get; }
        public long ID { get; }

        public EnumCommandResult FetchObjectResult { get; }

        public ObjectCommandContext([NotNull] ICommandTarget target, Timestamp? time, long id) : base(target, time)
        {
            if (target is ObjectSlot targetSlot)
            {
                this.ID = id;
                this.Slot = targetSlot;
                this.FetchObjectResult = EnumCommandResult.Ok;
            }
            else
            {
                this.ID = id;
                this.FetchObjectResult = this._Internal_LookupObject(id, out ObjectSlot slot);
                this.Slot = slot;
            }
        }

        public ObjectCommandContext([NotNull] ICommandTarget target, [NotNull] NetworkNode node, Timestamp? time, long id) : base(target, node, time)
        {
            if (target is ObjectSlot targetSlot)
            {
                this.ID = id;
                this.Slot = targetSlot;
                this.FetchObjectResult = EnumCommandResult.Ok;
            }
            else
            {
                this.ID = id;
                this.FetchObjectResult = this._Internal_LookupObject(id, out ObjectSlot slot);
                this.Slot = slot;
            }
        }
    }

}