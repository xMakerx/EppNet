///////////////////////////////////////////////////////
/// Filename: Command.cs
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

    public static class Commands
    {

        internal static readonly List<SlottableEnum> _cmdsList = new List<SlottableEnum>();

        public static readonly SlottableEnum None = SlottableEnum._Internal_CreateAndAddTo(_cmdsList, "None", 1);
    }

    public struct CommandResult
    {

        public ICommandTarget Target;
        public bool Success;
        public Exception Error;
        public TemplatedMessage Message;

        public CommandResult()
        {
            this.Target = null;
            this.Success = false;
            this.Error = null;
            this.Message = default;
        }

        public CommandResult(Exception error) : this()
        {
            this.Error = error;
        }

        public bool Successful() => Success && Error == null;
    }

    public interface ICommandTarget { }

    public abstract class Command : INodeDescendant
    {

        public readonly SlottableEnum EnumType;
        public readonly Timestamp Timestamp;
        public NetworkNode Node { get => _node; }

        protected readonly NetworkNode _node;

        protected Command([NotNull] NetworkNode node, SlottableEnum cmdType)
        {
            Guard.AgainstNull(node);
            this._node = node;

            this.EnumType = cmdType;
            this.Timestamp = _node.Time;
        }

        protected Command([NotNull] NetworkNode node, SlottableEnum cmdType, Timestamp timestamp)
        {
            Guard.AgainstNull(node);
            this._node = node;

            this.EnumType = cmdType;
            this.Timestamp = _node.Time;
        }

        protected Command([NotNull] INodeDescendant descendant, SlottableEnum cmdType)
        {
            Guard.AgainstNull(descendant);
            this._node = descendant.Node;

            this.EnumType = cmdType;
            this.Timestamp = _node.Time;
        }

        protected Command([NotNull] INodeDescendant descendant, SlottableEnum cmdType, Timestamp timestamp)
        {
            Guard.AgainstNull(descendant);
            this._node = descendant.Node;

            this.EnumType = cmdType;
            this.Timestamp = timestamp;
        }

        public abstract CommandResult Execute();

    }


}