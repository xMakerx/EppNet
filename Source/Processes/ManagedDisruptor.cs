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
        /// Tries to fetch a <see cref="RingBufferEvent"/> from the Disruptor.
        /// </summary>
        /// <param name="event">A valid <see cref="RingBufferEvent"/> instance or NULL</param>
        /// <returns>Whether or not an event was obtained</returns>

        public bool TryGetAndPublishEvent(Action<T> setupAction, long timeoutMs = 0)
        {
            SpinWait waiter = new();
            long expiration = ENet.Library.Time + timeoutMs;
            long sequenceId;

            bool retrieved;
            bool canRetry;

            do
            {
                retrieved = Buffer.TryNext(out sequenceId);
                canRetry = ENet.Library.Time < expiration;

                if (canRetry)
                {
                    Notify.Verbose("Failed to prepare event for ring buffer. Retrying...");
                    waiter.SpinOnce();
                }

            } while (canRetry);

            if (!retrieved)
            {
                Notify.Error($"Failed to prepare event for ring buffer. Buffer full! Increase limit from {Buffer.BufferSize} or set a timeout!");
                return false;
            }

            // Great! We fetched an event!
            T @event = this[sequenceId];

            // Provide the event with the fetched sequence id.
            @event._Internal_Preinitialize(sequenceId);
            setupAction?.Invoke(@event);

            Buffer.Publish(sequenceId);
            Notify.Debug($"Published {@event.GetType().Name} with Sequence ID {@event.SequenceID}");

            return retrieved;
        }

    }

}