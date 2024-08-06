/////////////////////////////////////////////
/// Filename: ManagedDisruptor.cs
/// Date: July 14, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using Disruptor;
using Disruptor.Dsl;

using EppNet.Processes.Events;
using EppNet.Logging;
using EppNet.Node;

using System;
using System.Threading.Tasks;
using System.Threading;

namespace EppNet.Processes
{

    public class ManagedDisruptor<T> : Disruptor<T>, ILoggable, INodeDescendant where T : RingBufferEvent
    {

        public ILoggable Notify { get => this; }
        public NetworkNode Node { get; }
        public RingBuffer Buffer { get => this.RingBuffer; }

        /// <summary>
        /// See <see cref="Disruptor{T}.Disruptor(Func{T}, int)"/>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="eventFactory"></param>
        /// <param name="ringBufferSize"></param>
        public ManagedDisruptor(NetworkNode node, Func<T> eventFactory, int ringBufferSize) : base(eventFactory, ringBufferSize)
        {
            this.Node = node;
        }

        /// <summary>
        /// See <see cref="Disruptor{T}.Disruptor(Func{T}, int, IWaitStrategy)"/>
        /// </summary>

        public ManagedDisruptor(NetworkNode node, Func<T> eventFactory, int ringBufferSize, IWaitStrategy waitStrategy) : base(eventFactory, ringBufferSize, waitStrategy)
        {
            this.Node = node;
        }

        /// <summary>
        /// See <see cref="Disruptor{Task}.Disruptor(Func{Task}, int, TaskScheduler, ProducerType, IWaitStrategy)"/>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="eventFactory"></param>
        /// <param name="ringBufferSize"></param>
        /// <param name="taskScheduler"></param>
        /// <param name="producerType"></param>
        /// <param name="waitStrategy"></param>

        public ManagedDisruptor(NetworkNode node, Func<T> eventFactory, int ringBufferSize, TaskScheduler taskScheduler, ProducerType producerType, IWaitStrategy waitStrategy)
            : base(eventFactory, ringBufferSize, taskScheduler, producerType, waitStrategy)
        {
            this.Node = node;
        }

        /// <summary>
        /// Tries to fetch and publish a <see cref="RingBufferEvent"/>.
        /// </summary>
        /// <returns>Whether or not an event was obtained</returns>

        public bool TryGetAndPublishEvent(Action<T> setupAction)
        {
            if (!Buffer.TryNext(out long sequenceId))
            {
                Notify.Error($"Failed to prepare event for ring buffer. Buffer full! Increase limit from {Buffer.BufferSize}!");
                return false;
            }

            // Great! We fetched an event!
            T @event = this[sequenceId];

            // Provide the event with the fetched sequence id.
            @event._Internal_Preinitialize(sequenceId);
            setupAction?.Invoke(@event);

            Buffer.Publish(sequenceId);
            Notify.Debug($"Published {@event.GetType().Name} with Sequence ID {@event.SequenceID}");
            return true;
        }

        public async Task<bool> TryGetAndPublishEventAsync(Action<T> setupAction, long timeoutMs)
        {
            if (timeoutMs == 0)
                Notify.Warning("It's very wasteful to call the async version with no timeout!");

            Task<T> fetchTask = _Internal_TryFetchEventAsync(timeoutMs);
            await fetchTask;
            
            T @event = fetchTask.Result;

            if (@event != null)
            {
                setupAction?.Invoke(@event);

                Buffer.Publish(@event.SequenceID);
                Notify.Debug($"Published {@event.GetType().Name} with Sequence ID {@event.SequenceID}");
            }

            return @event != null;
        }

        protected Task<T> _Internal_TryFetchEventAsync(long timeoutMs)
        {
            return Task.Factory.StartNew(() =>
            {
                SpinWait waiter = new();
                long expiration = ENet.Library.Time + timeoutMs;
                T @event = null;

                do
                {

                    if (Buffer.TryNext(out long sequenceId))
                    {
                        // We retrieved an event. Let's set it up
                        @event = this[sequenceId];
                        @event._Internal_Preinitialize(sequenceId);
                        break;
                    }

                    waiter.SpinOnce();
                    Notify.Verbose("Failed to prepare event for ring buffer. Retrying...");

                } while (ENet.Library.Time < expiration);

                if (@event == null)
                    Notify.Error($"Failed to fetch event after {timeoutMs} ms. Buffer full! Increase limit from {Buffer.BufferSize} or set a longer timeout!");

                return @event;
            });
        }

    }

}