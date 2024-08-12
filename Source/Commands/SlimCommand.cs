//////////////////////////////////////////////
/// Filename: SlimCommand.cs
/// Date: August 5, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Data;

using Microsoft.Extensions.ObjectPool;

using System;
using System.Diagnostics.CodeAnalysis;


namespace EppNet.Commands
{

    public interface ICommand : IDisposable
    {
        public EnumCommandResult Execute(in object context);
    }

    public interface ICommand<TContext> : ICommand where TContext : CommandContext
    {
        public EnumCommandResult Execute(in TContext context);
    }

    /// <summary>
    /// Slim Commands are meant to store as little data as possible as this class is meant
    /// to be stored in a collection. If every command stored all the context information (filled in by <see cref="CommandContext"/>),
    /// then we could very easily waste a TON of memory. If a command is stored in a collection, its
    /// executing context is most likely the same as all the other commands.
    /// </summary>

    public abstract class SlimCommand<T> : ICommand<T> where T : CommandContext
    {

        public readonly SlottableEnum EnumType;
        public readonly IObjectPool Pool;

        protected SlimCommand([NotNull] SlottableEnum enumType)
        {
            this.EnumType = enumType;

            Commands._cmdPools.TryGetValue(enumType, out Pool);
        }

        /// <summary>
        /// What is this garbage? A slim command is meant to only contain distinguishing data about
        /// a particular command to save memory.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public abstract EnumCommandResult Execute(in T context);

        public EnumCommandResult Execute(in object context) => Execute(context as T);
        public abstract void Dispose();

    }

}
