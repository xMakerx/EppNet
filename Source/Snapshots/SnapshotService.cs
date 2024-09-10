///////////////////////////////////////////////////////
/// Filename: SnapshotService.cs
/// Date: September 2, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Collections;
using EppNet.Services;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace EppNet.Snapshots
{

    public sealed class SnapshotService : Service
    {

        /// <summary>
        /// This number is multiplied by the <see cref="SnapshotsPerSecond"/>
        /// to create the size of the buffer
        /// </summary>
        public const float DefaultBufferMultiplier = 1.5f;

        /*
         * TODO: Handle desync events by storing when they occur, how long they last, and give option to
         * gracefully disconnect after X seconds. We know we lost connection to the server if X seconds
         * go by without a snapshot.
         */

        /// <summary>
        /// The flow of snapshots has been interrupted. This is called the first step interval where
        /// reading is unable to occur
        /// </summary>
        public Action OnDesyncEvent;

        /// <summary>
        /// The flow of snapshots has been restored. This is called once we receive another snapshot.
        /// </summary>
        public Action OnDesyncEventEnded;

        /// <summary>
        /// How many snapshots to take per second
        /// </summary>

        public int SnapshotsPerSecond { get; }

        public int SnapshotBufferSize { get; }

        public long GlobalSequenceNumber
        {
            private set
            {
                _globSequenceNum = value;
            }

            get => _globSequenceNum;
        }

        public SequenceNumber? SequenceNumber
        {
            get
            {
                if (_localSequenceNum > -1)
                    return new SequenceNumber(_localSequenceNum);

                return null;
            }
        }

        public Snapshot Current
        {
            get
            {
                try
                {
                    _lock.EnterReadLock();
                    return _currentSnapshot;
                }
                finally { _lock.ExitReadLock(); }
            }

        }

        /// <summary>
        /// Whether or not we're currently synchronized
        /// </summary>
        public bool IsSynchronized { private set; get; }

        internal SortedSet<Snapshot> _snapshots;
        internal ReaderWriterLockSlim _lock;
        internal Snapshot _currentSnapshot;

        private float _timeIval;
        private float _elapsedTimeSinceTick;
        private long _globSequenceNum;
        private int _localSequenceNum;

        public SnapshotService(ServiceManager svcMgr, int snapshotsPerSecond)
            : base(svcMgr, sortOrder: -999)
        {
            this.OnDesyncEvent = null;
            this.OnDesyncEventEnded = null;


            this.SnapshotsPerSecond = snapshotsPerSecond;
            this.SnapshotBufferSize = (int) Math.Floor(snapshotsPerSecond * DefaultBufferMultiplier);
            this.GlobalSequenceNumber = -1;
            this.IsSynchronized = false;
            this._localSequenceNum = -1;
            this._timeIval = _elapsedTimeSinceTick = 0f;
            this._lock = new();
            this._snapshots = new SortedSet<Snapshot>();
            this._currentSnapshot = null;
        }

        public bool TryAddSnapshot(Snapshot snapshot)
        {
            if (snapshot == null)
                return false;

            long earliestAllowedTicks = Node.Time.Ticks - (long)Math.Floor(Node.Time.Ticks * DefaultBufferMultiplier);

            if (_currentSnapshot != null && _currentSnapshot.Timestamp > snapshot.Timestamp)
                return false;

            if (snapshot.Timestamp < new TimeSpan(earliestAllowedTicks))
                // TODO: SnapshotService: Handle when given a really old snapshot
                return false;

            Snapshot min = _snapshots.Min;

            // Snapshot is too old
            if (min != null && min.Timestamp > snapshot.Timestamp)
                return false;

            // TODO: Finish
            return true;
        }

        public override bool Start()
        {
            bool started = base.Start();

            if (started)
            {
                this.GlobalSequenceNumber = -1;
                this._localSequenceNum = -1;
                this._timeIval = 1f / SnapshotsPerSecond;
                this._elapsedTimeSinceTick = 0f;
            }

            return started;
        }

        public override bool Tick(float dt)
        {
            _elapsedTimeSinceTick += dt;

            if (_elapsedTimeSinceTick >= _timeIval)
            {
                _Internal_Step();
                _elapsedTimeSinceTick -= _timeIval;
            }

            return true;
        }

        private void _Internal_Step()
        {
            Interlocked.Increment(ref _globSequenceNum);

            if (_localSequenceNum > -1)
            {
                int next = (int)SequenceNumber?.Next().Value;
                Interlocked.Exchange(ref _localSequenceNum, next);
            }
            else
                Interlocked.Increment(ref _localSequenceNum);

            if (Node.Distro == Distribution.Server)
                _Internal_CreateSnapshot();

            else if (Node.Distro == Distribution.Client)
                // Read next
                return;
        }

        private void _Internal_CreateSnapshot()
        {
            try
            {
                _lock.EnterWriteLock();

                Snapshot previous = _currentSnapshot;

                _currentSnapshot = new Snapshot(this, _globSequenceNum, SequenceNumber.Value);
                _currentSnapshot.Previous = previous;

                if (previous != null)
                {
                    previous.Next = _currentSnapshot;
                    // Send previous snapshot out
                }

                _currentSnapshot.RecordCurrent();

                // Let's ensure our buffer doesn't get too big
                Iterator<Snapshot> iterator = _snapshots.Iterator(); 
                while (_snapshots.Count + 1 > SnapshotBufferSize)
                {
                    Snapshot snapshot = iterator.Next();
                    _snapshots.Remove(snapshot);
                }

                _snapshots.Add(Current);
            }
            finally { _lock.ExitWriteLock(); }
        }

    }

}