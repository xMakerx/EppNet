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
        /// <typeparam name="T"></typeparam>
        /// <param name="disruptor"></param>
        /// <param name="event">A valid <see cref="RingBufferEvent"/> instance or NULL</param>
        /// <returns>Whether or not an event was obtained</returns>

        public bool TryGetEvent(out T @event)
        {
            bool retrieved = Buffer.TryNext(out long sequenceId);
            @event = null;

            if (!retrieved)
            {
                Notify.Error($"Failed to prepare event for ring buffer. Buff full! Increase limit from {Buffer.BufferSize}!");
                return false;
            }

            try
            {
                // Great! We fetched an event!
                @event = this[sequenceId];

                // Provide the event with the fetched sequence id.
                @event._Internal_Preinitialize(sequenceId);
            }
            catch (Exception e)
            {
                Notify.Error($"Failed to prepare event for ring buffer. Error: {e.Message}\nStack Trace: {e.StackTrace}");
                Node.HandleException(e);
                return false;
            }

            return retrieved;
        }

        /// <summary>
        /// Tries to publish a <see cref="RingBufferEvent"/> to the Disruptor.
        /// </summary>
        /// <returns>Whether or not the <see cref="RingBufferEvent"/> was successfully published</returns>

        public bool TryPublishEvent(T @event)
        {
            if (@event == null)
            {
                Notify.Error("Tried to publish a NULL event. Did you forget to obtain one?");
                return false;
            }

            if (@event.Disposed)
            {
                Notify.Error("Tried to publish a disposed event. Did you forget to initialize it?");
                return false;
            }

            try
            {
                // Let's try to publish this event.
                Buffer.Publish(@event.SequenceID);
                Notify.Debug($"Published {@event.GetType().Name} with Sequence ID {@event.SequenceID}");
                return true;
            }
            catch (Exception e)
            {
                // Something went wrong somewhere
                Notify.Error($"Failed to publish event. Error: {e.Message}\nStack Trace: {e.StackTrace}");
                Node.HandleException(e);
                return false;
            }
        }

    }

}