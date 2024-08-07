/////////////////////////////////////////////
/// Filename: MultithreadedBuffer.cs
/// Date: August 6, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Logging;
using EppNet.Node;
using EppNet.Utilities;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace EppNet.Processes;


public interface IBufferEventHandler<T> where T : IBufferEvent
{
    public bool Handle(T @event);
}

public interface IBufferEvent : IDisposable
{

    public bool ShouldContinue { set; get; }

    public void Initialize();
    public void Cleanup();

    public bool IsDisposed();

}

public sealed class MultithreadedBufferBuilder<T> where T : class, IBufferEvent, new()
{
    private readonly List<IReadOnlyList<IBufferEventHandler<T>>> _handlers = [];
    private readonly NetworkNode _node;
    private readonly int _poolSize;

    public MultithreadedBufferBuilder(NetworkNode node, int poolSize = 256)
    {
        ArgumentNullException.ThrowIfNull(node);

        _node = node;
        _poolSize = poolSize;
    }

    public MultithreadedBufferBuilder<T> ThenUseHandlers(params IBufferEventHandler<T>[] handlers)
    {
        _handlers.Add(handlers);
        return this;
    }

    public MultithreadedBuffer<T> Build() =>
        new(_node, _poolSize, _handlers);
}

public sealed class MultithreadedBuffer<T> : IDisposable, ILoggable, INodeDescendant where T : class, IBufferEvent, new()
{
    public NetworkNode Node { get; }
    public ILoggable Notify => this;

    public Action OnCanceled;

    private readonly Channel<T> _channel;
    private readonly ChannelReader<T> _reader;
    private readonly ChannelWriter<T> _writer;
    private readonly CancellationTokenSource _tokenSrc;

    private readonly DefaultObjectPool<T> _pool;
    private readonly DefaultObjectPool<EventProcessor> _processors;
    private readonly IReadOnlyList<IReadOnlyList<IBufferEventHandler<T>>> _handlers;

    private Task _readerTask;

    public MultithreadedBuffer(NetworkNode node, int poolSize, IReadOnlyList<IReadOnlyList<IBufferEventHandler<T>>> handlers)
    {
        Node = node;
        _handlers = handlers;

        _channel = Channel.CreateUnbounded<T>();
        _reader = _channel.Reader;
        _writer = _channel.Writer;
        _tokenSrc = new CancellationTokenSource();

        OnCanceled = null;

        _pool = new(new DefaultPooledObjectPolicy<T>(), poolSize);
        _processors = new(new DefaultPooledObjectPolicy<EventProcessor>(), poolSize);
    }

    public void Dispose()
    {
        Cancel();
        _tokenSrc.Dispose();
    }

    public void Start()
    {
        if (_readerTask is not null)
            return;

        // Start tasks
        _readerTask = Task.Run(Read);
        Notify.Debug("Buffer started!");
    }

    public void Cancel()
    {
        if (_readerTask is null)
            return;

        _tokenSrc.Cancel();
        _readerTask.GetAwaiter().GetResult();

        OnCanceled?.Invoke();
        Notify.Debug("Buffer canceled!");
    }

    public bool CreateAndWrite(Action<T> action)
    {
        var @event = _pool.Get();
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

        _writer.WriteAsync(@event, _tokenSrc.Token);
        return true;
    }

    private async Task Read()
    {
        while (!_tokenSrc.IsCancellationRequested)
        {
            var @event = await _reader.ReadAsync(_tokenSrc.Token);

            var processor = _processors.Get();
            processor.Buffer = this;
            processor.Event = @event;
            _ = Task.Run(processor.ProcessEvent);
        }
    }

    private sealed class EventProcessor
    {
        public MultithreadedBuffer<T> Buffer { get; set; }
        public T Event { get; set; }

        private CancellationTokenSource _tokenSource;
        private bool _processing;

        public void ProcessEvent()
        {
            try
            {
                _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(Buffer._tokenSrc.Token);
                _processing = true;

                var options = new ParallelOptions { CancellationToken = _tokenSource.Token };

                foreach (var step in Buffer._handlers)
                {
                    if (!_processing)
                        break;

                    Parallel.ForEach(
                        step,
                        options,
                        HandleEvent
                    );
                }
            }
            finally
            {
                Buffer._pool.Return(Event);
                Buffer._processors.Return(this);
            }
        }

        private void HandleEvent(IBufferEventHandler<T> handler, ParallelLoopState state)
        {
            if (!handler.Handle(Event))
            {
                _processing = false;
                _tokenSource.Cancel();
            }
        }
    }
}
