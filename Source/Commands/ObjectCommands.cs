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
using System.Diagnostics.CodeAnalysis;


namespace EppNet.Commands
{

    public class ObjectCallMethodCommand : ObjectUpdateFieldCommand
    {

        public ObjectCallMethodCommand() : base(false, Commands.Object_CallMethod) { }

        public ObjectCallMethodCommand(int index, params object[] args) : base(false, index, Commands.Object_CallMethod, args) { }
    }

    public class ObjectSetPropertyCommand : ObjectUpdateFieldCommand
    {

        public ObjectSetPropertyCommand() : base(true, Commands.Object_SetProperty) { }

        public ObjectSetPropertyCommand(int index, params object[] args) : base(true, index, Commands.Object_SetProperty, args) { }

    }

    public abstract class ObjectUpdateFieldCommand : ObjectCommand
    {

        public bool IsProperty { get; }
        public object[] Arguments { set; get; }
        public int Index { set; get; }

        protected ObjectUpdateFieldCommand(bool isProperty, SlottableEnum enumType) : base(enumType)
        {
            this.IsProperty = isProperty;
            this.Arguments = null;
            this.Index = -1;
        }

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

            ObjectRegistration registration = context.Slot.Object.Metadata;
            ObjectMemberDefinition mDef = IsProperty ? registration.GetProperty(Index) : registration.GetMethod(Index);

            if (mDef != null)
            {
                try
                {
                    mDef.Invoke(context.Slot.Object.UserObject, Arguments);
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

        public override void Dispose()
        {
            this.Arguments = null;
            this.Index = -1;
        }
    }

    public class ObjectSetParentCommand : ObjectCommand
    {
        public long ParentID { set; get; }

        public ObjectSetParentCommand() : base(Commands.Object_SetParent)
        {
            this.ParentID = -1;
        }

        public ObjectSetParentCommand(long parentID) : this()
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

            return context.Slot.Object.ReparentTo(ParentID);
        }

        public override void Dispose()
        {
            this.ParentID = -1;
        }

    }

    public class CreateObjectCommand : ObjectCommand
    {

        public int ObjectTypeId { set; get; }

        public CreateObjectCommand() : base(Commands.Create)
        {
            this.ObjectTypeId = -1;
        }

        public CreateObjectCommand(int objectTypeId) : this()
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

        public override void Dispose()
        {
            this.ObjectTypeId = -1;
        }

    }

    public class DeleteObjectCommand : ObjectCommand
    {
        public uint TicksUntilDeletion { set; get; }

        public DeleteObjectCommand() : base(Commands.Delete)
        {
            this.TicksUntilDeletion = 0;
        }

        public DeleteObjectCommand(uint ticksUntilDeletion) : this()
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

        public override void Dispose()
        {
            this.TicksUntilDeletion = 0;
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

        public ObjectCommandContext([NotNull] ICommandTarget target, TimeSpan? time, long id) : base(target, time)
        {
            if (Guard.IsNotNullOf(target, out ObjectSlot targetSlot))
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

        public ObjectCommandContext([NotNull] ICommandTarget target, [NotNull] NetworkNode node, TimeSpan? time, long id) : base(target, node, time)
        {
            if (Guard.IsNotNullOf(target, out ObjectSlot targetSlot))
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