//////////////////////////////////////////////
/// Filename: SlimCommand.cs
/// Date: August 5, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Data;
using EppNet.Objects;
using EppNet.Utilities;

using System.Diagnostics.CodeAnalysis;


namespace EppNet.Commands
{

    /// <summary>
    /// Slim Commands are meant to store as little data as possible as this class is meant
    /// to be stored in a collection. If every command stored all the context information (filled in by <see cref="CommandContext"/>),
    /// then we could very easily waste a TON of memory. If a command is stored in a collection, its
    /// executing context is most likely the same as all the other commands.
    /// </summary>

    public abstract class SlimCommand<T> where T : CommandContext
    {

        public readonly SlottableEnum EnumType;

        protected SlimCommand([NotNull] SlottableEnum enumType)
        {
            this.EnumType = enumType;
        }

        /// <summary>
        /// What is this garbage? A slim command is meant to only contain distinguishing data about
        /// a particular command to save memory.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public abstract EnumCommandResult Execute(in T context);

        protected EnumCommandResult _Internal_LookupObject(in T context, long id, out ObjectSlot slot)
        {
            ObjectService service = context.Node.Services.GetService<ObjectService>();
            slot = null;

            if (!this.IsNotNull(arg: service, tmpMsg: new("Object Service could not be found!"), fatal: true))
                return EnumCommandResult.NoService;

            if (service.TryGetById(id, out slot))
                return EnumCommandResult.Ok;

            return EnumCommandResult.NotFound;
        }

    }

}
