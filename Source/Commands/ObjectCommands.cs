///////////////////////////////////////////////////////
/// Filename: ObjectCommands.cs
/// Date: August 4, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;
using EppNet.Logging;
using EppNet.Objects;
using EppNet.Utilities;

using System.Collections.Generic;


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

        public ObjectSetParentCommand(long id, long parentID) : base(id, ObjectCommands.Object_SetParent)
        {
            this.ParentID = parentID;
        }

        public override EnumCommandResult Execute(in CommandContext context)
        {
            if (ParentID == ID)
                return EnumCommandResult.BadArgument;

            EnumCommandResult lookMeUp = _Internal_LookupObject(in context, ID, out ObjectSlot slot);

            if (!lookMeUp.IsOk())
                return lookMeUp;

            return slot.Agent.ReparentTo(ParentID);
        }

    }

    public class CreateObjectCommand : ObjectCommand
    {

        public int ObjectTypeId { get; }

        public CreateObjectCommand(int objectTypeId) : this(objectTypeId, -1) { }

        public CreateObjectCommand(int objectTypeId, long id) : base(id, ObjectCommands.Create)
        {
            this.ObjectTypeId = objectTypeId;
        }

        public override EnumCommandResult Execute(in CommandContext context)
        {
            EnumCommandResult result = new();

            ObjectService service = context.Node.Services.GetService<ObjectService>();

            if (this.IsNotNull(arg: service, tmpMsg: new("Object Service could not be found!"), fatal: true))
                result = service.TryCreateObject(ObjectTypeId, out _, ID);

            return result;
        }

    }

    public class DeleteObjectCommand : ObjectCommand
    {
        public uint TicksUntilDeletion { get; }

        public DeleteObjectCommand(long id, uint ticksUntilDeletion) : base(id, ObjectCommands.Delete)
        {
            this.TicksUntilDeletion = ticksUntilDeletion;
        }

        public override EnumCommandResult Execute(in CommandContext context)
        {
            ObjectService service = context.Node.Services.GetService<ObjectService>();
            this.IsNotNull(arg: service, tmpMsg: new TemplatedMessage("Object Service could not be found!"), fatal: true);
            return service.TryRequestDelete(ID, TicksUntilDeletion);
        }

    }

    public abstract class ObjectCommand : SlimCommand
    {

        public long ID { get; }

        protected ObjectCommand(long id, SlottableEnum enumType) : base(enumType)
        {
            this.ID = id;
        }

    }

}