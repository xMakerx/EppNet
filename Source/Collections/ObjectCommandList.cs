///////////////////////////////////////////////////////
/// Filename: CommandList.cs
/// Date: August 13, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Commands;
using EppNet.Objects;
using EppNet.Snapshots;
using EppNet.Utilities;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace EppNet.Collections
{

    public class ObjectCommandList : IDisposable
    {

        public readonly ObjectAgent Object;
        public readonly ObjectSnapshot Snapshot;
        public ObjectSnapshot RelativeTo { get => _relativeTo; }

        public int ReliableCommands { get => _reliableCmds; }

        public bool Absolute => RelativeTo == null;

        protected List<ICommand> _list;
        protected ReaderWriterLockSlim _lock;

        protected int _reliableCmds;
        protected ObjectSnapshot _relativeTo;

        public ObjectCommandList([NotNull] ObjectSnapshot snapshot)
        {
            Guard.AgainstNull(snapshot);
            this.Object = snapshot.Object;
            this.Snapshot = snapshot;

            this._list = new();
            this._lock = new();
            this._reliableCmds = 0;
            this._relativeTo = null;
        }

        public void SetRelativeTo([NotNull] ObjectSnapshot other)
        {
            Guard.AgainstNull(other);

        }

        public void Dispose()
        {
            _list?.Clear();
            _list = null;

            _lock?.Dispose();
            _lock = null;

            _reliableCmds = 0;
        }

    }

}