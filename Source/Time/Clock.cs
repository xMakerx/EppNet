///////////////////////////////////////////////////////
/// Filename: Clock.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data.Datagrams;
using EppNet.Node;
using EppNet.Services;
using EppNet.Sockets;
using EppNet.Utilities;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace EppNet.Time
{

    public interface IClock : IRunnable, IDisposable
    {
        
        /// <summary>
        /// The current locally calculated time<br/>
        /// Ensure this is thread-safe!
        /// </summary>
        public TimeSpan Time { get; }
        public uint SyncIvalMs { set; get; }
        public uint DesyncToleranceMs { set; get; }
        public double SmoothingFactor { set; get; }

        public void RequestSynchronize();
        public void Synchronize(TimeSpan remoteTimeSpan);
    }


    public class Clock : INodeDescendant, IClock
    {

        public const int DefaultSyncIvalMs = 5000;
        public const int MaxAllowedDesyncMs = 100;
        public const double DefaultSmoothingFactor = 0.1d;

        public TimeSpan Time
        {
            get
            {
                try
                {
                    _lock.EnterReadLock();

                    TimeSpan localElapsed = _stopwatch.Elapsed;
                    return localElapsed + _localTimeOffset;

                }
                finally { _lock.ExitReadLock(); }
            }
        }

        public uint SyncIvalMs
        {
            set
            {
                if (_stopwatch.IsRunning)
                    throw new InvalidOperationException("Cannot change the sync rate while the clock is running!");

                if (value == 0)
                    throw new ArgumentOutOfRangeException("Cannot send sync requests every tick!");

                _syncIvalMs = value;
            }

            get => _syncIvalMs;
        }

        public uint DesyncToleranceMs
        {
            set
            {
                if (_stopwatch.IsRunning)
                    throw new InvalidOperationException("Cannot change the desync tolerance while the clock is running!");

                if (value == 0)
                    throw new ArgumentOutOfRangeException("Cannot have a zero millisecond desync tolerance!");

                _desyncToleranceMs = value;
            }

            get => _desyncToleranceMs;
        }

        public double SmoothingFactor
        {
            set
            {
                if (_stopwatch.IsRunning)
                    throw new InvalidOperationException("Cannot change smoothing factor while the clock is running!");

                if (value <= 0 || value >= 1d)
                    throw new ArgumentOutOfRangeException("Invalid smoothing factor! Must be between (0, 1)");

                _smoothingFactor = value;
            }

            get => _smoothingFactor;
        }

        public NetworkNode Node { get => _socket.Node; }
        protected BaseSocket _socket;

        private readonly Stopwatch _stopwatch;
        private readonly ReaderWriterLockSlim _lock;

        private CancellationTokenSource _cancelTokenSrc;
        
        private TimeSpan _localTimeOffset;
        private Task _syncTask;
        private uint _syncIvalMs, _desyncToleranceMs;
        private double _smoothingFactor;

        public Clock([NotNull] BaseSocket socket)
        {
            Guard.AgainstNull(socket);
            this._socket = socket;

            this._localTimeOffset = TimeSpan.Zero;
            this._stopwatch = new();
            this._lock = new();
            this._cancelTokenSrc = socket.IsServer() ? null : new();
            this._syncTask = null;
            this._syncIvalMs = DefaultSyncIvalMs;
            this._desyncToleranceMs = MaxAllowedDesyncMs;
            this._smoothingFactor = DefaultSmoothingFactor;
        }

        public void RequestSynchronize()
        {
            // If we're trying to cancel or on the server, do nothing.
            if (_socket.IsServer() || _cancelTokenSrc.IsCancellationRequested)
                return;

            PingDatagram datagram = new()
            {
                SentTime = (ulong)this.Time.Milliseconds
            };

            Node.SendInstant(datagram);
        }

        public void Synchronize(TimeSpan remoteTimeSpan)
        {
            try
            {
                _lock.EnterWriteLock();

                TimeSpan localElapsed = _stopwatch.Elapsed;

                TimeSpan expectedRemoteTime = localElapsed + _localTimeOffset;
                double desyncMs = (remoteTimeSpan - expectedRemoteTime).TotalMilliseconds;

                if (Math.Abs(desyncMs) > _desyncToleranceMs)
                    _localTimeOffset += TimeSpan.FromMilliseconds(desyncMs);
                else
                    _localTimeOffset += TimeSpan.FromMilliseconds(desyncMs * _smoothingFactor);
            }
            finally { _lock.ExitWriteLock(); }
        }

        public bool Start()
        {
            if (_stopwatch.IsRunning)
                return false;

            bool isServer = _socket.IsServer();

            // Let's begin our clock
            _stopwatch.Start();
            _localTimeOffset = TimeSpan.Zero;

            if (!isServer)
            {
                if (_cancelTokenSrc.IsCancellationRequested)
                {
                    // We need to create a new source
                    _cancelTokenSrc.Dispose();
                    _cancelTokenSrc = null;

                    _cancelTokenSrc = new();
                }

                _syncTask = Task.Run(async () =>
                {
                    int delayMs = (int)_syncIvalMs;
                    while (!_cancelTokenSrc.IsCancellationRequested)
                    {
                        await Task.Delay(delayMs);
                        RequestSynchronize();
                    }
                }, _cancelTokenSrc.Token);
            }

            return true;
        }

        public bool Stop()
        {
            if (!_stopwatch.IsRunning)
                return false;

            if (!_socket.IsServer())
            {
                // Let's get our synchronization task offline
                _cancelTokenSrc.Cancel();
                _syncTask.Wait();
                _syncTask = null;
            }

            // Let's stop our stopwatch
            _stopwatch.Stop();
            return true;
        }

        public void Dispose()
        {
            Stop();
            _lock.Dispose();
            _cancelTokenSrc?.Dispose();
        }
    }

}

