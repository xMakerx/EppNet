/////////////////////////////////////////////
/// Filename: MultithreadedBuffer.cs
/// Date: August 6, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Logging;
using EppNet.Node;
using EppNet.Utilities;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace EppNet.Processes
{

    public interface IBufferEventHandler<T> where T : IBufferEvent
    {

        public bool Handle(ref T @event);
    }

    public interface IBufferEvent : IDisposable
    {

        public bool ShouldContinue { set; get; }

        public void Initialize();
        public void Cleanup();

        public bool IsDisposed();

    }

    public class BufferProcessStep<T> where T : IBufferEvent, new()
    {

        public readonly MultithreadedBuffer<T> Buffer;
        public BufferProcessStep<T> Next { private set; get; }

        private readonly List<IBufferEventHandler<T>> _handlers;

        public BufferProcessStep([NotNull] MultithreadedBuffer<T> buffer, [NotNull] IBufferEventHandler<T> handler)
        {
            Guard.AgainstNull(buffer, handler);
            this.Buffer = buffer;
            this._handlers = new() { handler };
            this.Next = null;
        }

        public async Task ExecuteAsync(T @event, CancellationTokenSource tokenSrc)
        {
            T currentEvent = Unsafe.AsRef(@event);
            CancellationToken token = tokenSrc.Token;

            Task taskRoot = null;

            if (_handlers.Count == 1)
                taskRoot = Task.Run(() => _handlers[0].Handle(ref currentEvent), token);
            else
            {
                taskRoot = Task.Run(() => Parallel.ForEachAsync(_handlers, Buffer.Options, async (handler, cancelToken) =>
                {
                    if (cancelToken.IsCancellationRequested || !handler.Handle(ref @event))
                        tokenSrc.Cancel();
                }));
            }

            await taskRoot;
        }

        public bool Execute(T @event, CancellationTokenSource tokenSrc, ParallelOptions options)
        {

            if (_handlers.Count == 1)
            {
                Task.Run(() => _handlers[0].Handle(ref @event));
                return true;
            }

            bool canContinue = false;

            Parallel.ForEach(Partitioner.Create(_handlers, EnumerablePartitionerOptions.NoBuffering), options, (IBufferEventHandler<T> handler) =>
            {
                canContinue = !tokenSrc.IsCancellationRequested && @event.ShouldContinue;
                if (!canContinue)
                    return;

                if (!handler.Handle(ref @event))
                {
                    canContinue = false;
                    tokenSrc.Cancel();
                }
            });

            return canContinue;
        }

        public BufferProcessStep<T> With([NotNull] IBufferEventHandler<T> handler)
        {
            Guard.AgainstNull(handler);
            _handlers.Add(handler);
            return this;
        }

        public BufferProcessStep<T> Then([NotNull] IBufferEventHandler<T> next)
        {
            Guard.AgainstNull(next);
            Next = new BufferProcessStep<T>(Buffer, next);
            return Next;
        }

        public void Clear() => _handlers.Clear();

    }

    public class MultithreadedBuffer<T> : IDisposable, ILoggable, INodeDescendant where T : IBufferEvent, new()
    {
        public NetworkNode Node { get => _node; }
        public ILoggable Notify => this;
        public BufferProcessStep<T> FirstStep { private set; get; }

        public Action OnCanceled;

        public readonly ParallelOptions Options;

        protected NetworkNode _node;
        protected Channel<T> _channel;
        protected ChannelReader<T> _reader;
        protected ChannelWriter<T> _writer;
        protected CancellationTokenSource _tokenSrc;
        protected CancellationToken _token;

        protected Thread _readerThread;
        protected bool _started;

        protected ConcurrentStack<T> _pool;
        protected SpinWait _waiter;

        public MultithreadedBuffer([NotNull] NetworkNode node, int poolSize = 256)
        {
            Guard.AgainstNull(node);
            this._node = node;

            this._channel = Channel.CreateUnbounded<T>();
            this._reader = _channel.Reader;
            this._writer = _channel.Writer;
            this._tokenSrc = new CancellationTokenSource();
            this._token = default;

            this._waiter = new SpinWait();
            this._readerThread = null;
            this._started = false;
            this.OnCanceled = null;
            this.Options = new();

            this._pool = new ConcurrentStack<T>();

            for (int i = 0; i < poolSize; i++)
                _pool.Push(new T());
        }

        public void Dispose()
        {
            Cancel();
            _tokenSrc.Dispose();
        }

        public void Start()
        {
            if (_started)
                return;

            // Start tasks
            _token = _tokenSrc.Token;
            _readerThread = new Thread(Read);
            _readerThread.Start();
            _started = true;
            Notify.Debug("Buffer started!");
        }

        public void Cancel()
        {
            if (!_started)
                return;

            _tokenSrc.Cancel();
            _readerThread?.Join();

            BufferProcessStep<T> step = FirstStep;

            while (step != null && !_token.IsCancellationRequested)
            {
                step.Clear();
                step = step.Next;
            }

            _started = false;
            OnCanceled?.Invoke();
            Notify.Debug("Buffer canceled!");
        }

        public BufferProcessStep<T> HandleEventsWith(params IBufferEventHandler<T>[] handlers)
        {
            Guard.AgainstNull(handlers);
            FirstStep = new BufferProcessStep<T>(this, handlers[0]);

            for (int i = 1; i < handlers.Length; i++)
                FirstStep.With(handlers[i]);

            return FirstStep;
        }

        public bool CreateAndWrite(Action<T> action)
        {
            bool success = _pool.TryPop(out T @event);

            if (!success)
            {
                Notify.Error("NO SPACE!");
                return false;
            }

            @event.Initialize();
            action?.Invoke(@event);
            TryWrite(@event);
            return true;
        }

        public bool TryWrite(in T @event)
        {
            if (@event == null)
            {
                Notify.Error("Cannot write a null event!");
                return false;
            }

            _writer.WriteAsync(@event, _token);
            return true;
        }

        public void Read()
        {
            CancellationTokenSource tokenSrc = new();
            while (!_token.IsCancellationRequested)
            {
                if (!_reader.TryRead(out T @event))
                {
                    Thread.Yield();
                    continue;
                }

                Task.Run(async () =>
                {

                    Options.CancellationToken = tokenSrc.Token;
                    BufferProcessStep<T> step = FirstStep;

                    while (!_token.IsCancellationRequested && step != null)
                    {
                        await step.ExecuteAsync(@event, tokenSrc);
                        step = step.Next;
                    }

                    @event.Dispose();
                    _pool.Push(@event);
                });

            }

        }

    }

}
