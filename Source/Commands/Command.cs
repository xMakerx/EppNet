///////////////////////////////////////////////////////
/// Filename: Command.cs
/// Date: August 4, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;
using EppNet.Utilities;

using Microsoft.Extensions.ObjectPool;

using System;
using System.Collections;
using System.Collections.Generic;

namespace EppNet.Commands
{

    public static class Commands
    {

        public const int DefaultCommandPoolSize = 256;

        internal static readonly List<SlottableEnum> _cmdsList = new List<SlottableEnum>();
        internal static readonly Dictionary<SlottableEnum, IObjectPool> _cmdPools = new();

        public static readonly SlottableEnum None = SlottableEnum._Internal_CreateAndAddTo(_cmdsList, "None", 1);

        // Creation and deletion are in the same slot as they can undo each other
        public static readonly SlottableEnum Create = _Internal_CreatePoolableAndAddTo<CreateObjectCommand>(_cmdsList, "Create_Object", 1);
        public static readonly SlottableEnum Delete = _Internal_CreatePoolableAndAddTo<DeleteObjectCommand>(_cmdsList, "Delete_Object", 1);

        // Generate, Enable, and Disable are in the same slot as they can undo each other
        public static readonly SlottableEnum Object_Generate = SlottableEnum._Internal_CreateAndAddTo(_cmdsList, "Object_Generate", 2);
        public static readonly SlottableEnum Object_Enable = SlottableEnum._Internal_CreateAndAddTo(_cmdsList, "Object_Enable", 2);
        public static readonly SlottableEnum Object_Disable = SlottableEnum._Internal_CreateAndAddTo(_cmdsList, "Object_Disable", 2);

        public static readonly SlottableEnum Object_SetParent = _Internal_CreatePoolableAndAddTo<ObjectSetParentCommand>(_cmdsList, "Object_SetParent", 3);
        public static readonly SlottableEnum Object_SetProperty = _Internal_CreatePoolableAndAddTo<ObjectSetPropertyCommand>(_cmdsList, "Object_SetProperty", 4);
        public static readonly SlottableEnum Object_CallMethod = _Internal_CreatePoolableAndAddTo<ObjectCallMethodCommand>(_cmdsList, "Object_CallMethod", 5);
        public static readonly SlottableEnum Object_AckSnapshot = SlottableEnum._Internal_CreateAndAddTo(_cmdsList, "Object_AckSnapshot", 6);

        internal static SlottableEnum _Internal_CreatePoolableAndAddTo<T>(IList group, string name, uint slot) where T : class, ICommand, new()
        {
            SlottableEnum slottable = SlottableEnum._Internal_CreateAndAddTo(group, name, slot);
            _cmdPools[slottable] = new CommandObjectPool<T>(DefaultCommandPoolSize);
            return slottable;
        }

    }

    public interface IObjectPool
    {
        object Get();

        void Return(object obj);
    }

    public class CommandObjectPool<TCommand> : IObjectPool where TCommand : class, ICommand, new()
    {
        public DefaultObjectPool<TCommand> Pool { get; }

        public CommandObjectPool(int poolSize)
        {
            this.Pool = new(new DefaultPooledObjectPolicy<TCommand>(), poolSize);
        }

        public object Get()
            => Pool.Get();

        public void Return(object obj)
        {
            if (Guard.IsNotNullOf(obj, out TCommand command))
                Pool.Return(command);
        }

    }

    public interface ICommandTarget { }

}
