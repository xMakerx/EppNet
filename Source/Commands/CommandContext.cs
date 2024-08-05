//////////////////////////////////////////////
/// Filename: CommandContext.cs
/// Date: August 5, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////
using EppNet.Node;
using EppNet.Objects;
using EppNet.Time;
using EppNet.Utilities;

using System;
using System.Diagnostics.CodeAnalysis;

namespace EppNet.Commands
{
    /// <summary>
    /// Command contexts are necessary for <see cref="SlimCommand"/>s to function. They provide
    /// the context necessary for a <see cref="SlimCommand"/> to execute;<br/> without this, the
    /// <see cref="SlimCommand"/> can't execute anything.
    /// </summary>
    public class CommandContext : INodeDescendant
    {

        public NetworkNode Node { get => _node; }
        public readonly ICommandTarget Target;

        public Timestamp Timestamp { private set; get; }

        protected readonly NetworkNode _node;

        /// <summary>
        /// Creates a new command context assuming that the specified <see cref="ICommandTarget"/> derives from
        /// <see cref="INodeDescendant"/>. Uses <see cref="NetworkNode.Time"/> if no Timestamp provided. <br/>
        /// <b>NOTE:</b> Throws <see cref="InvalidOperationException"/> if <see cref="ICommandTarget"/> does not derive from <see cref="INodeDescendant"/>.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="time"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentNullException"></exception>

        public CommandContext([NotNull] ICommandTarget target, Timestamp? time)
        {
            Guard.AgainstNull(target);
            this.Target = target;

            if (target is INodeDescendant nDesc)
                this._node = nDesc.Node;
            else
                throw new InvalidOperationException("Must specify NetworkNode explicitly in separate constructor!");

            this.Timestamp = time ?? _node.Time;
        }

        /// <summary>
        /// Creates a new command context with the specified <see cref="ICommandTarget"/> and <see cref="NetworkNode"/>.
        /// Uses <see cref="NetworkNode.Time"/> if no Timestamp provided. <br/>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="node"></param>
        /// <param name="time"></param>
        /// <exception cref="InvalidOperationException"></exception>

        public CommandContext([NotNull] ICommandTarget target, [NotNull] NetworkNode node, Timestamp? time)
        {
            Guard.AgainstNull(target, node);
            this.Target = target;
            this._node = node;
            this.Timestamp = time ?? _node.Time;
        }

        protected EnumCommandResult _Internal_LookupObject(long id, out ObjectSlot slot)
        {
            ObjectService service = Node.Services.GetService<ObjectService>();
            slot = null;

            if (!this.IsNotNull(arg: service, tmpMsg: new("Object Service could not be found!"), fatal: true))
                return EnumCommandResult.NoService;

            if (service.TryGetById(id, out slot))
                return EnumCommandResult.Ok;

            return EnumCommandResult.NotFound;
        }

    }

}
